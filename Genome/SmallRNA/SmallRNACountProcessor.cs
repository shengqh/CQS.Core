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
  public class SmallRNACountProcessor : AbstractSmallRNACountProcessor<SmallRNACountProcessorOptions>
  {
    protected Dictionary<char, char> noPaneltyMutations = new Dictionary<char, char>();

    public SmallRNACountProcessor(SmallRNACountProcessorOptions options)
      : base(options)
    {
      noPaneltyMutations['C'] = 'T';
      noPaneltyMutations['G'] = 'A';
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

      var resultFilename = options.OutputFile;
      result.Add(resultFilename);

      //parsing reads
      HashSet<string> totalQueries;
      var reads = ParseCandidates(options.InputFiles, resultFilename, out totalQueries);
      int totalQueryCount;
      if (reads.Count == totalQueries.Count && File.Exists(options.CountFile))
      {
        totalQueryCount = Counts.GetTotalCount();
      }
      else
      {
        totalQueryCount = (from q in totalQueries select q.StringBefore(SmallRNAConsts.NTA_TAG)).Distinct().Sum(m => Counts.GetCount(m));
      }

      //First of all, draw mapping position graph
      Progress.SetMessage("Drawing position pictures...");

      var miRNAPositionFile = Path.ChangeExtension(options.OutputFile, SmallRNAConsts.miRNA + ".position");
      var tRNAPositionFile = Path.ChangeExtension(options.OutputFile, SmallRNAConsts.tRNA + ".position");
      if (!options.NotOverwrite || (!File.Exists(miRNAPositionFile) && !File.Exists(tRNAPositionFile)))
      {
        var notNTAreads = reads.Where(m => m.Qname.Contains(SmallRNAConsts.NTA_TAG) && m.Qname.StringAfter(SmallRNAConsts.NTA_TAG).Length > 0).ToList();
        DrawPositionImage(notNTAreads, featureLocations.Where(m => m.Category.Equals(SmallRNAConsts.miRNA)).ToList(), "miRNA", miRNAPositionFile);
        DrawPositionImage(notNTAreads, featureLocations.Where(m => m.Category.Equals(SmallRNAConsts.tRNA)).ToList(), "tRNA", tRNAPositionFile);
      }

      var allmapped = new List<FeatureItemGroup>();
      var mappedfile = resultFilename + ".mapped.xml";
      if (File.Exists(mappedfile) && options.NotOverwrite)
      {
        Progress.SetMessage("Reading mapped feature items...");
        allmapped = new FeatureItemGroupXmlFormat().ReadFromFile(mappedfile);
      }
      else
      {
        Progress.SetMessage("Mapping feature items...");
        //mapping reads to features based on miRNA, tRNA and other smallRNA priority
        MapReadToSequenceRegion(featureLocations, reads);

        var featureMapped = featureLocations.GroupByName();
        featureMapped.RemoveAll(m => m.EstimateCount == 0);
        featureMapped.ForEach(m => m.CombineLocations());

        var mirnaGroups = featureMapped.Where(m => m.Name.StartsWith(SmallRNAConsts.miRNA)).GroupBySequence();
        if (mirnaGroups.Count > 0)
        {
          OrderFeatureItemGroup(mirnaGroups);

          Progress.SetMessage("writing miRNA count ...");

          var mirnaCountFile = Path.ChangeExtension(resultFilename, "." + SmallRNAConsts.miRNA + ".count");
          new SmallRNACountMicroRNAWriter(options.Offsets).WriteToFile(mirnaCountFile, mirnaGroups);
          result.Add(mirnaCountFile);
          allmapped.AddRange(mirnaGroups);
        }

        var trnaGroups = featureMapped.Where(m => m.Name.StartsWith(SmallRNAConsts.tRNA)).GroupByIdenticalQuery();
        if (trnaGroups.Count > 0)
        {
          OrderFeatureItemGroup(trnaGroups);

          Progress.SetMessage("writing tRNA count ...");
          var trnaCountFile = Path.ChangeExtension(resultFilename, "." + SmallRNAConsts.tRNA + ".count");

          new FeatureItemGroupCountWriter().WriteToFile(trnaCountFile, trnaGroups);
          result.Add(trnaCountFile);

          allmapped.AddRange(trnaGroups);
        }

        var otherGroups = featureMapped.Where(m => !m.Name.StartsWith(SmallRNAConsts.miRNA) && !m.Name.StartsWith(SmallRNAConsts.tRNA)).GroupByIdenticalQuery();
        if (otherGroups.Count > 0)
        {
          OrderFeatureItemGroup(otherGroups);

          if (mirnaGroups.Count > 0 || trnaGroups.Count > 0)
          {
            Progress.SetMessage("writing other smallRNA count ...");
            var otherCountFile = Path.ChangeExtension(resultFilename, ".other.count");
            new FeatureItemGroupCountWriter().WriteToFile(otherCountFile, otherGroups);
            result.Add(otherCountFile);
          }

          allmapped.AddRange(otherGroups);
        }

        Progress.SetMessage("writing all smallRNA count ...");
        new FeatureItemGroupCountWriter().WriteToFile(resultFilename, allmapped);
        result.Add(resultFilename);

        Progress.SetMessage("writing mapping details...");
        new FeatureItemGroupXmlFormat().WriteToFile(mappedfile, allmapped);
        result.Add(mappedfile);
      }

      var totalMappedCount = (from q in reads select q.Qname.StringBefore(SmallRNAConsts.NTA_TAG)).Distinct().Sum(m => Counts.GetCount(m));

      var infoFile = Path.ChangeExtension(resultFilename, ".info");
      WriteSummaryFile(allmapped, totalQueryCount, totalMappedCount, infoFile);
      result.Add(infoFile);

      Progress.End();

      return result;
    }

    protected void MapReadToSequenceRegion(List<FeatureLocation> mapped, List<SAMAlignedItem> reads)
    {
      var chrStrandMatchedMap = BuildStrandMap(reads);

      //First of all, mapping to miRNA
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

      //remove reads with NTA
      foreach (var strandMap in chrStrandMatchedMap.Values)
      {
        foreach (var locList in strandMap.Values)
        {
          locList.RemoveAll(k => k.Parent.Qname.Contains(SmallRNAConsts.NTA_TAG) && k.Parent.Qname.StringAfter(SmallRNAConsts.NTA_TAG).Length > 0);
        }
      }

      //Secondly, mapping to tRNA 
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
  }
}