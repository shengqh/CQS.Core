using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CQS.Genome.Fastq;
using CQS.Genome.Sam;
using CQS.Genome.Mirna;
using CQS.Genome.Feature;
using CQS.Genome.Mapping;
using RCPA.Utils;

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
      HashSet<string> trnaQueries;
      var trnaReads = ParseCandidates(options.InputFiles, resultFilename, out trnaQueries);
      trnaReads.AssignOriginalName();

      HashSet<string> otherrnaQueries;
      var otherRNAReads = ParseCandidates(options.OtherFile, resultFilename + ".other", out otherrnaQueries);
      otherRNAReads.AssignOriginalName();

      var allmapped = new List<FeatureItemGroup>();
      var mappedfile = resultFilename + ".mapped.xml";

      if (File.Exists(mappedfile) && options.NotOverwrite)
      {
        Progress.SetMessage("Reading mapped feature items...");
        allmapped = new FeatureItemGroupXmlFormat().ReadFromFile(mappedfile);
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
        MapReadToSequenceRegion(trnaLocations, trnaReads);

        var trnaMapped = trnaLocations.GroupByName();
        trnaMapped.RemoveAll(m => m.EstimateCount == 0);
        trnaMapped.ForEach(m => m.CombineLocations());

        var trnaGroups = trnaMapped.GroupByIdenticalQuery();
        if (trnaGroups.Count > 0)
        {
          Progress.SetMessage("Writing tRNA count ...");
          var trnaCountFile = Path.ChangeExtension(resultFilename, "." + SmallRNAConsts.tRNA + ".count");

          OrderFeatureItemGroup(trnaGroups);
          new FeatureItemGroupTIGRTCountWriter().WriteToFile(trnaCountFile, trnaGroups);
          result.Add(trnaCountFile);

          allmapped.AddRange(trnaGroups);
        }

        //Get all queries mapped to tRNA
        var tRNAreads = new HashSet<string>(from read in GetMappedReads(trnaLocations)
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
        MapReadToSequenceRegion(notTrnaLocations, otherRNAReads);

        var notTrnaMapped = notTrnaLocations.GroupByName();
        notTrnaMapped.RemoveAll(m => m.EstimateCount == 0);
        notTrnaMapped.ForEach(m => m.CombineLocations());

        var mirnaGroups = notTrnaMapped.Where(m => m.Name.StartsWith(SmallRNAConsts.miRNA)).GroupBySequence();
        if (mirnaGroups.Count > 0)
        {
          Progress.SetMessage("writing miRNA count ...");
          OrderFeatureItemGroup(mirnaGroups);

          var mirnaCountFile = Path.ChangeExtension(resultFilename, "." + SmallRNAConsts.miRNA + ".count");
          new SmallRNACountMicroRNAWriter(options.Offsets).WriteToFile(mirnaCountFile, mirnaGroups);
          result.Add(mirnaCountFile);
          allmapped.AddRange(mirnaGroups);
        }

        var otherGroups = notTrnaMapped.Where(m => !m.Name.StartsWith(SmallRNAConsts.miRNA)).GroupByIdenticalQuery();
        if (otherGroups.Count > 0)
        {
          Progress.SetMessage("writing other smallRNA count ...");
          var otherCountFile = Path.ChangeExtension(resultFilename, ".other.count");

          OrderFeatureItemGroup(otherGroups);
          new FeatureItemGroupTIGRTCountWriter().WriteToFile(otherCountFile, otherGroups);
          result.Add(otherCountFile);

          allmapped.AddRange(otherGroups);
        }

        Progress.SetMessage("writing all smallRNA count ...");
        new FeatureItemGroupTIGRTCountWriter().WriteToFile(resultFilename, allmapped);
        result.Add(resultFilename);

        Progress.SetMessage("writing mapping details...");
        new FeatureItemGroupXmlFormat().WriteToFile(mappedfile, allmapped);
        result.Add(mappedfile);
      }

      var totalQueryCount = trnaQueries.Union(otherrnaQueries).Sum(m => Counts.GetCount(m));
      var totalMappedCount = (from q in trnaReads select q.OriginalQname).Union(from q in otherRNAReads select q.OriginalQname).Sum(m => Counts.GetCount(m));

      var infoFile = Path.ChangeExtension(resultFilename, ".info");
      WriteSummaryFile(allmapped, totalQueryCount, totalMappedCount, infoFile);
      result.Add(infoFile);

      Progress.End();

      return result;
    }

    protected void MapReadToSequenceRegion(List<FeatureLocation> mapped, List<SAMAlignedItem> reads)
    {
      var chrStrandMatchedMap = BuildStrandMap(reads);

      //First of all, mapping to tRNA 
      var tRNAs = mapped.Where(m => m.Category.Equals(SmallRNAConsts.tRNA)).ToList();
      if (tRNAs.Count > 0)
      {
        Progress.SetMessage("Mapping reads to {0} tRNA entries.", tRNAs.Count);

        MapReadsToSmallRNA(tRNAs, chrStrandMatchedMap);

        var mappedReads = GetMappedReads(tRNAs);
        Progress.SetMessage("There are {0} SAM entries mapped to tRNA entries.", mappedReads.Count);

        //remove reads mapped to tRNA
        RemoveReadsFromMap(chrStrandMatchedMap, mappedReads);
      }

      //Second of all, mapping to miRNA
      var miRNAs = mapped.Where(m => m.Category.Equals(SmallRNAConsts.miRNA)).ToList();
      if (miRNAs.Count > 0)
      {
        Progress.SetMessage("Mapping reads to {0} miRNA entries.", miRNAs.Count);

        MapReadsToMiRNA(miRNAs, chrStrandMatchedMap);

        var mappedReads = GetMappedReads(miRNAs);
        Progress.SetMessage("There are {0} SAM entries mapped to miRNA entries.", mappedReads.Count);

        //remove reads mapped to miRNA
        RemoveReadsFromMap(chrStrandMatchedMap, mappedReads);
      }

      //mapping to other smallRNA
      var otherRNAs = mapped.Where(m => !m.Category.Equals(SmallRNAConsts.miRNA) && !m.Category.Equals(SmallRNAConsts.tRNA) && !m.Category.Equals(SmallRNAConsts.lincRNA)).ToList();
      if (otherRNAs.Count > 0)
      {
        Progress.SetMessage("Mapping reads to {0} other smallRNA entries.", otherRNAs.Count);

        MapReadsToSmallRNA(otherRNAs, chrStrandMatchedMap);

        var otherReads = GetMappedReads(otherRNAs);
        Progress.SetMessage("There are {0} SAM entries mapped to other smallRNA entries.", otherReads.Count);

        //remove reads mapped to tRNA
        RemoveReadsFromMap(chrStrandMatchedMap, otherReads);
      }

      //mapping to lincRNA
      var lincRNAs = mapped.Where(m => m.Category.Equals(SmallRNAConsts.lincRNA)).ToList();
      if (lincRNAs.Count > 0)
      {
        Progress.SetMessage("Mapping reads to {0} lincRNA entries.", lincRNAs.Count);

        MapReadsToLincRNA(lincRNAs, chrStrandMatchedMap);

        var lincReads = GetMappedReads(lincRNAs);
        Progress.SetMessage("There are {0} SAM entries mapped to lincRNA entries.", lincReads.Count);

        //remove reads mapped to tRNA
        RemoveReadsFromMap(chrStrandMatchedMap, lincReads);
      }
    }

    protected override void WriteOptions(StreamWriter sw)
    {
      base.WriteOptions(sw);
      sw.WriteLine("#maximumMismatchForShortRead\t{0}", options.MaximumMismatchForShortRead);
    }
  }
}