using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CQS.Genome.Fastq;
using CQS.Genome.Sam;

namespace CQS.Genome.Mapping
{
  public class MappedCountProcessor : AbstractCountProcessor<MappedCountProcessorOptions>
  {
    public MappedCountProcessor(MappedCountProcessorOptions options)
      : base(options)
    {
    }

    public override IEnumerable<string> Process(string fileName)
    {
      var result = new List<string>();

      //read regions      
      var srItems = GetSequenceRegions(options.GTFKey);
      Progress.SetMessage("There are {0} coordinate entries", srItems.Count);
      if (srItems.Count == 0)
      {
        throw new Exception(string.Format("No coordinate found for {0} in file {1}", options.GTFKey,
          options.CoordinateFile));
      }

      var resultFilename = GetResultFilename(fileName);
      result.Add(resultFilename);

      //parsing reads
      int totalReadCount;
      int mappedReadCount;
      var reads = ParseCandidates(fileName, resultFilename, out totalReadCount, out mappedReadCount);

      var regions = srItems.ConvertAll(m => new SequenceRegionMapped { Region = m });

      Progress.SetMessage("mapping reads to sequence regions...");
      MapReadToSequenceRegion(regions, reads);

      var featureReadCount = reads.Where(m => m.Locations.Any(n => n.Features.Count > 0)).Sum(m => m.QueryCount);
      Console.WriteLine("feature reads = {0}", featureReadCount);

      var mappedItems = MappedItemUtils.GroupByName(regions);
      mappedItems.RemoveAll(m => m.EstimateCount == 0);

      var mappedGroups = mappedItems.GroupByIdenticalQuery();

      //group by miRNA name
      if (!options.NoMappedFile)
      {
        Progress.SetMessage("output mapping details...");
        var mappedfile = resultFilename + ".mapped.xml";
        new MappedItemGroupXmlFileFormat().WriteToFile(mappedfile, mappedGroups);
        result.Add(mappedfile);
      }

      Progress.SetMessage("write result ...");
      new MappedItemGroupCountWriter().WriteToFile(resultFilename, mappedGroups);

      if (options.ExportLengthDistribution)
      {
        var disfile = resultFilename + ".length";
        new MappedItemGroupReadLengthWriter().WriteToFile(disfile, mappedGroups);
        result.Add(disfile);
      }

      if (options.ExportSequenceCount)
      {
        var seqfile = resultFilename + ".seqcount";
        new MappedItemGroupSequenceWriter().WriteToFile(seqfile, mappedGroups);
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
          new FastqExtractorFromFastq { Progress = Progress }.Extract(options.FastqFile, unmappedFile, except);
        }
        else
        {
          new FastqExtractorFromBam(options.Samtools) { Progress = Progress }.Extract(fileName, unmappedFile, except);
        }
        result.Add(unmappedFile);
      }

      Progress.SetMessage("summarizing ...");
      var infoFile = Path.ChangeExtension(resultFilename, ".info");
      using (var sw = new StreamWriter(infoFile))
      {
        sw.WriteLine("#file\t{0}", fileName);
        sw.WriteLine("#coordinate\t{0}", options.CoordinateFile);
        sw.WriteLine("#minLength\t{0}", options.MinimumReadLength);
        sw.WriteLine("#maxMismatchCount\t{0}", options.MaximumMismatchCount);
        if (File.Exists(options.CountFile))
        {
          sw.WriteLine("#countFile\t{0}", options.CountFile);
        }
        sw.WriteLine("TotalReads\t{0}", totalReadCount);
        sw.WriteLine("MappedReads\t{0}", mappedReadCount);
        sw.WriteLine("MultipleMappedReads\t{0}", reads.Where(m => m.Locations.Count > 1).Sum(m => m.QueryCount));
        sw.WriteLine("FeatureReads\t{0}", featureReadCount);
      }
      result.Add(infoFile);

      Progress.End();

      return result;
    }

    private void MapReadToSequenceRegion(List<SequenceRegionMapped> mapped, List<SAMAlignedItem> reads)
    {
      //build chr/strand/samlist map
      Progress.SetMessage("building chr/strand/samlist map ...");

      var chrStrandMatchedMap = new Dictionary<string, Dictionary<char, List<SAMAlignedLocation>>>();
      foreach (var read in reads)
      {
        foreach (var loc in read.Locations)
        {
          Dictionary<char, List<SAMAlignedLocation>> map;
          if (!chrStrandMatchedMap.TryGetValue(loc.Seqname, out map))
          {
            map = new Dictionary<char, List<SAMAlignedLocation>>();
            map['+'] = new List<SAMAlignedLocation>();
            map['-'] = new List<SAMAlignedLocation>();
            chrStrandMatchedMap[loc.Seqname] = map;
          }
          map[loc.Strand].Add(loc);
        }
      }

      Progress.SetRange(0, mapped.Count);
      var gmapped = new Dictionary<string, SAMAlignedItem>();
      foreach (var curmapped in mapped)
      {
        Progress.Increment(1);
        Dictionary<char, List<SAMAlignedLocation>> curMatchedMap;

        if (!chrStrandMatchedMap.TryGetValue(curmapped.Region.Seqname, out curMatchedMap))
        {
          continue;
        }

        //mapped query must have same oritation with miRNA defined at gff or bed file.
        var matches = curMatchedMap[curmapped.Region.Strand];
        foreach (var m in matches)
        {
          if (!curmapped.Region.Overlap(m, 0))
            continue;

          m.Features.Add(curmapped.Region);
          curmapped.AlignedLocations.Add(m);
        }
      }
    }
  }
}