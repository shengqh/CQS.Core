using CQS.Genome.Feature;
using CQS.Genome.Mapping;
using CQS.Genome.Sam;
using RCPA.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CQS.Genome.SmallRNA
{
  public class TGIRTCountProcessor : AbstractSmallRNACountProcessor<TGIRTCountProcessorOptions>
  {
    private TGIRTCountProcessorOptions _options;
    public TGIRTCountProcessor(TGIRTCountProcessorOptions options)
      : base(options)
    {
      this._options = options;
    }

    protected override void FilterAlignedItems(List<SAMAlignedItem> result)
    {
      base.FilterAlignedItems(result);

      result.RemoveAll(m =>
      {
        if (m.Sequence.Length <= _options.MaximumLengthOfShortRead)
        {
          if (m.Locations.First().NumberOfMismatch > _options.MaximumMismatchForShortRead)
          {
            return true;
          }
        }

        //no insertion allowed
        if (m.Locations.First().Cigar.Contains("I"))
        {
          return true;
        }

        //only 1 deletion allowed
        if (m.Locations.First().Cigar.Count(l => l.Equals('D')) > 1)
        {
          return true;
        }

        return false;
      });
    }

    public override IEnumerable<string> Process()
    {
      var result = new List<string>();

      //read regions      
      var featureLocations = options.GetSequenceRegions();
      Progress.SetMessage("There are {0} coordinate entries", featureLocations.Count);
      if (featureLocations.Count == 0)
      {
        throw new Exception(string.Format("No coordinate found in file {1}", options.CoordinateFile));
      }

      var trnaLocations = featureLocations.Where(l => l.Category.Equals(SmallRNAConsts.tRNA)).ToList();
      var mirnaLocations = featureLocations.Where(l => l.Category.Equals(SmallRNAConsts.miRNA)).ToList();
      var notTrnaLocations = featureLocations.Where(l => !l.Category.Equals(SmallRNAConsts.tRNA)).ToList();

      var resultFilename = options.OutputFile;
      result.Add(resultFilename);

      Progress.SetMessage("Parsing tRNA alignment result ...");

      //Parsing reads
      List<QueryInfo> trnaQueries;
      var trnaReads = ParseCandidates(options.InputFiles, resultFilename, out trnaQueries);
      SmallRNAUtils.InitializeSmallRnaNTA(trnaReads);

      var hasNTA = trnaReads.Any(l => l.NTA.Length > 0);

      List<QueryInfo> otherrnaQueries;
      var otherRNAReads = ParseCandidates(options.OtherFile, resultFilename + ".other", out otherrnaQueries);
      SmallRNAUtils.InitializeSmallRnaNTA(otherRNAReads);

      var featureGroups = new List<FeatureItemGroup>();
      var mappedfile = resultFilename + ".mapped.xml";

      if (File.Exists(mappedfile) && options.NotOverwrite)
      {
        Progress.SetMessage("Reading mapped feature items...");
        featureGroups = new FeatureItemGroupXmlFormat().ReadFromFile(mappedfile);
      }
      else
      {
        Progress.SetMessage("Mapping to tRNA...");

        //Draw tRNA mapping position graph
        Progress.SetMessage("Drawing tRNA position pictures...");
        var tRNAPositionFile = Path.ChangeExtension(options.OutputFile, SmallRNAConsts.tRNA + ".position");
        if (!options.NotOverwrite || !File.Exists(tRNAPositionFile))
        {
          DrawPositionImage(trnaReads, trnaLocations, "tRNA", tRNAPositionFile);
        }

        //Map reads to tRNA
        MapReadToSequenceRegion(trnaLocations, trnaReads, hasNTA);

        var trnaMapped = trnaLocations.GroupByName();
        trnaMapped.RemoveAll(m => m.GetEstimatedCount() == 0);
        trnaMapped.ForEach(m => m.CombineLocations());

        var trnaGroups = trnaMapped.GroupByIdenticalQuery();
        if (trnaGroups.Count > 0)
        {
          Progress.SetMessage("Writing tRNA count ...");
          var trnaCountFile = Path.ChangeExtension(resultFilename, "." + SmallRNAConsts.tRNA + ".count");

          OrderFeatureItemGroup(trnaGroups);
          new FeatureItemGroupTIGRTCountWriter().WriteToFile(trnaCountFile, trnaGroups);
          result.Add(trnaCountFile);

          featureGroups.AddRange(trnaGroups);
        }

        //Get all queries mapped to tRNA
        var tRNAreads = new HashSet<string>(from read in SmallRNAUtils.GetMappedReads(trnaLocations)
                                            select read.OriginalQname);

        //Remove all reads mapped to tRNA
        otherRNAReads.RemoveAll(m => tRNAreads.Contains(m.OriginalQname));

        //Draw miRNA mapping position graph
        Progress.SetMessage("Drawing miRNA position pictures...");
        var miRNAPositionFile = Path.ChangeExtension(options.OutputFile, SmallRNAConsts.miRNA + ".position");
        if (!options.NotOverwrite || !File.Exists(miRNAPositionFile))
        {
          DrawPositionImage(otherRNAReads, mirnaLocations, "miRNA", miRNAPositionFile);
        }

        //Map reads to not tRNA
        MapReadToSequenceRegion(notTrnaLocations, otherRNAReads, hasNTA);

        var notTrnaMapped = notTrnaLocations.GroupByName();
        notTrnaMapped.RemoveAll(m => m.GetEstimatedCount() == 0);
        notTrnaMapped.ForEach(m => m.CombineLocations());

        var mirnaGroups = notTrnaMapped.Where(m => m.Name.StartsWith(SmallRNAConsts.miRNA)).GroupBySequence();
        if (mirnaGroups.Count > 0)
        {
          Progress.SetMessage("writing miRNA count ...");
          OrderFeatureItemGroup(mirnaGroups);

          var mirnaCountFile = Path.ChangeExtension(resultFilename, "." + SmallRNAConsts.miRNA + ".count");
          new SmallRNACountMicroRNAWriter(options.Offsets).WriteToFile(mirnaCountFile, mirnaGroups);
          result.Add(mirnaCountFile);
          featureGroups.AddRange(mirnaGroups);
        }

        var otherGroups = notTrnaMapped.Where(m => !m.Name.StartsWith(SmallRNAConsts.miRNA)).GroupByIdenticalQuery();
        if (otherGroups.Count > 0)
        {
          Progress.SetMessage("writing other smallRNA count ...");
          var otherCountFile = Path.ChangeExtension(resultFilename, ".other.count");

          OrderFeatureItemGroup(otherGroups);
          new FeatureItemGroupTIGRTCountWriter().WriteToFile(otherCountFile, otherGroups);
          result.Add(otherCountFile);

          featureGroups.AddRange(otherGroups);
        }

        Progress.SetMessage("writing all smallRNA count ...");
        new FeatureItemGroupTIGRTCountWriter().WriteToFile(resultFilename, featureGroups);
        result.Add(resultFilename);

        Progress.SetMessage("writing mapping details...");
        new FeatureItemGroupXmlFormat().WriteToFile(mappedfile, featureGroups);
        result.Add(mappedfile);
      }

      var readSummary = GetReadSummary(featureGroups, new HashSet<string>(), trnaReads.Union(otherRNAReads).ToList(), trnaQueries.Union(otherrnaQueries).ToList());

      var totalQueryCount = (from q in trnaQueries.Union(otherrnaQueries) select q.Name.StringBefore(SmallRNAConsts.NTA_TAG)).Distinct().Sum(m => Counts.GetCount(m));
      var totalMappedCount = (from q in trnaReads select q.OriginalQname).Union(from q in otherRNAReads select q.OriginalQname).Distinct().Sum(m => Counts.GetCount(m));

      var infoFile = Path.ChangeExtension(resultFilename, ".info");
      WriteSummaryFile(infoFile, readSummary, featureGroups);
      result.Add(infoFile);

      Progress.End();

      return result;
    }

    protected void MapReadToSequenceRegion(List<FeatureLocation> mapped, List<SAMAlignedItem> reads, bool hasNTA)
    {
      var chrStrandMatchedMap = BuildStrandMap(reads);

      List<IReadMapper> mappers = new List<IReadMapper>();
      mappers.Add(new SmallRNAMapperTRNA(options, false, null) { Progress = this.Progress });
      mappers.Add(new SmallRNAMapperNTAFilter(hasNTA, false, true, null));

      mappers.Add(new SmallRNAMapperMicroRNA(options, hasNTA) { Progress = this.Progress });
      mappers.Add(new SmallRNAMapperNTAFilter(hasNTA, false, false, null));

      mappers.Add(new SmallRNAMapper("otherSmallRNA", options, feature =>
        !feature.Category.Equals(SmallRNABiotype.miRNA.ToString()) &&
        !feature.Category.Equals(SmallRNABiotype.tRNA.ToString()) &&
        !feature.Category.Equals(SmallRNABiotype.lincRNA.ToString()) &&
        !feature.Category.Equals(SmallRNABiotype.lncRNA.ToString()) &&
        !feature.Name.Contains("SILVA_"))
      { Progress = this.Progress });

      mappers.Add(new SmallRNAMapperLincRNA(SmallRNABiotype.lincRNA.ToString(), options) { Progress = this.Progress });
      mappers.Add(new SmallRNAMapperLincRNA(SmallRNABiotype.lncRNA.ToString(), options) { Progress = this.Progress });

      foreach (var mapper in mappers)
      {
        mapper.MapReadToFeatureAndRemoveFromMap(mapped, chrStrandMatchedMap);
      }
    }

    protected override void WriteOptions(StreamWriter sw)
    {
      base.WriteOptions(sw);
      sw.WriteLine("#maximumMismatchForShortRead\t{0}", options.MaximumMismatchForShortRead);
    }
  }
}