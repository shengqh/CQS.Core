using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CQS.Genome.Fastq;
using CQS.Genome.Sam;
using CQS.Genome.Mirna;
using CQS.Genome.Feature;
using CQS.Genome.SmallRNA;

namespace CQS.Genome.Mapping
{
  public class MappedCountProcessor : AbstractCountProcessor<MappedCountProcessorOptions>
  {
    private MappedCountProcessorOptions MappedOptions;

    public MappedCountProcessor(MappedCountProcessorOptions options)
      : base(options)
    {
      this.MappedOptions = options;
    }

    public override IEnumerable<string> Process()
    {
      var result = new List<string>();

      //read regions      
      var featureLocations = this.MappedOptions.GetSequenceRegions();

      Progress.SetMessage("There are {0} coordinate entries", featureLocations.Count);
      if (featureLocations.Count == 0)
      {
        throw new Exception(string.Format("No coordinate found for {0} in file {1}", options.GtfFeatureName,
          options.CoordinateFile));
      }

      var resultFilename = options.OutputFile;
      result.Add(resultFilename);

      //parsing reads
      var totalQueries = new HashSet<string>();
      var reads = ParseCandidates(options.InputFile, resultFilename, out totalQueries);

      int totalQueryCount;
      if (reads.Count == totalQueries.Count && File.Exists(options.CountFile))
      {
        totalQueryCount = Counts.GetTotalCount();
      }
      else
      {
        totalQueryCount = (from q in totalQueries select q.StringBefore(SmallRNAConsts.NTA_TAG)).Distinct().Sum(m => Counts.GetCount(m));
      }

      if (reads.Count > 0 && reads[0].Qname.Contains(SmallRNAConsts.NTA_TAG))
      {
        if (!options.NTA)
        {
          reads.RemoveAll(m => !m.Qname.EndsWith(SmallRNAConsts.NTA_TAG));
        }
      }
      var totalMappedCount = (from q in reads select q.Qname.StringBefore(SmallRNAConsts.NTA_TAG)).Distinct().Sum(m => Counts.GetCount(m));

      Progress.SetMessage("mapping reads to sequence regions...");
      MapReadToSequenceRegion(featureLocations, reads);

      var featureReadCount = reads.Where(m => m.Locations.Any(n => n.Features.Count > 0)).Sum(m => m.QueryCount);
      Console.WriteLine("feature reads = {0}", featureReadCount);

      var mappedItems = featureLocations.GroupByName();
      mappedItems.RemoveAll(m => m.EstimateCount == 0);

      mappedItems.ForEach(m => m.CombineLocations());

      var mappedGroups = mappedItems.GroupByIdenticalQuery();

      //group by miRNA name
      if (!options.NoMappedFile)
      {
        Progress.SetMessage("output mapping details...");
        var mappedfile = resultFilename + ".mapped.xml";
        new FeatureItemGroupXmlFormat().WriteToFile(mappedfile, mappedGroups);
        result.Add(mappedfile);
      }

      Progress.SetMessage("write result ...");
      mappedGroups.Sort((m1, m2) => m2.EstimateCount.CompareTo(m1.EstimateCount));
      new FeatureItemGroupCountWriter().WriteToFile(resultFilename, mappedGroups);

      if (options.ExportLengthDistribution)
      {
        var disfile = resultFilename + ".length";
        new FeatureItemGroupReadLengthWriter().WriteToFile(disfile, mappedGroups);
        result.Add(disfile);
      }

      if (options.ExportSequenceCount)
      {
        var seqfile = resultFilename + ".seqcount";
        new FeatureItemGroupSequenceWriter().WriteToFile(seqfile, mappedGroups);
        result.Add(seqfile);
      }

      if (options.UnmappedFastq)
      {
        Progress.SetMessage("output unmapped query...");
        var unmappedFile = Path.ChangeExtension(resultFilename, ".unmapped.fastq.gz");
        var except = new HashSet<string>(from r in reads
                                         where r.Locations.Count > 0
                                         select r.Qname);

        if (File.Exists(options.FastqFile))
        {
          new FastqExtractorFromFastq { Progress = Progress }.Extract(options.FastqFile, unmappedFile, except, options.CountFile);
        }
        else
        {
          new FastqExtractorFromBam() { Progress = Progress }.Extract(options.InputFile, unmappedFile, except, options.CountFile);
        }
        result.Add(unmappedFile);
      }

      Progress.SetMessage("summarizing ...");
      var infoFile = Path.ChangeExtension(resultFilename, ".info");
      using (var sw = new StreamWriter(infoFile))
      {
        sw.WriteLine("#file\t{0}", options.InputFile);
        sw.WriteLine("#coordinate\t{0}", options.CoordinateFile);
        sw.WriteLine("#minLength\t{0}", options.MinimumReadLength);
        sw.WriteLine("#maxMismatchCount\t{0}", options.MaximumMismatch);
        if (File.Exists(options.CountFile))
        {
          sw.WriteLine("#countFile\t{0}", options.CountFile);
        }

        sw.WriteLine("TotalReads\t{0}", totalQueryCount);
        sw.WriteLine("MappedReads\t{0}", totalMappedCount);
        sw.WriteLine("MultipleMappedReads\t{0}", reads.Where(m => m.Locations.Count > 1).Sum(m => m.QueryCount));
        sw.WriteLine("FeatureReads\t{0}", featureReadCount);
      }
      result.Add(infoFile);

      Progress.End();

      return result;
    }

    private void MapReadToSequenceRegion(List<FeatureLocation> mapped, List<SAMAlignedItem> reads)
    {
      if (options.OrientationFree)
      {
        Progress.SetMessage("Orientation free ...");
        DoMapReadToSequenceRegionOrientationFree(mapped, reads);
      }
      else
      {
        DoMapReadToSequenceRegionWithOrientation(mapped, reads);
      }
    }

    private void DoMapReadToSequenceRegionWithOrientation(List<FeatureLocation> mapped, List<SAMAlignedItem> reads)
    {
      //build chr/strand/samlist map
      Progress.SetMessage("building chr/strand/samlist map ...");

      var chrStrandMatchedMap = new Dictionary<string, Dictionary<char, List<SamAlignedLocation>>>();
      foreach (var read in reads)
      {
        foreach (var loc in read.Locations)
        {
          Dictionary<char, List<SamAlignedLocation>> map;
          if (!chrStrandMatchedMap.TryGetValue(loc.Seqname, out map))
          {
            map = new Dictionary<char, List<SamAlignedLocation>>();
            map['+'] = new List<SamAlignedLocation>();
            map['-'] = new List<SamAlignedLocation>();
            chrStrandMatchedMap[loc.Seqname] = map;
          }
          map[loc.Strand].Add(loc);
        }
      }

      var gmapped = new Dictionary<string, SAMAlignedItem>();
      foreach (var curmapped in mapped)
      {
        Dictionary<char, List<SamAlignedLocation>> curMatchedMap;

        if (!chrStrandMatchedMap.TryGetValue(curmapped.Seqname, out curMatchedMap))
        {
          continue;
        }

        //mapped query must have same oritation with miRNA defined at gff or bed file.
        var matches = curMatchedMap[curmapped.Strand];
        foreach (var m in matches)
        {
          if (!curmapped.Overlap(m, 0))
            continue;

          var fsl = new FeatureSamLocation(curmapped);
          fsl.SamLocation = m;
        }
      }
    }
    private void DoMapReadToSequenceRegionOrientationFree(List<FeatureLocation> mapped, List<SAMAlignedItem> reads)
    {
      //build chr/samlist map
      Progress.SetMessage("building chr/samlist map ...");

      var chrStrandMatchedMap = new Dictionary<string, List<SamAlignedLocation>>();
      foreach (var read in reads)
      {
        foreach (var loc in read.Locations)
        {
          List<SamAlignedLocation> map;
          if (!chrStrandMatchedMap.TryGetValue(loc.Seqname, out map))
          {
            map = new List<SamAlignedLocation>();
            chrStrandMatchedMap[loc.Seqname] = map;
          }
          map.Add(loc);
        }
      }

      var gmapped = new Dictionary<string, SAMAlignedItem>();
      foreach (var curmapped in mapped)
      {
        List<SamAlignedLocation> matches;

        if (!chrStrandMatchedMap.TryGetValue(curmapped.Seqname, out matches))
        {
          continue;
        }

        foreach (var m in matches)
        {
          var op = curmapped.OverlapPercentage(m);
          if (op == 0.0 || op < options.MinimumOverlapPercentage)
          {
            continue;
          }

          var fsl = new FeatureSamLocation(curmapped);
          fsl.SamLocation = m;
          fsl.OverlapPercentage = op;
        }
      }
    }
  }
}