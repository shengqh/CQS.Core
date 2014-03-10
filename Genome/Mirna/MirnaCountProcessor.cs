using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RCPA;
using System.IO;
using CQS.Genome.Sam;
using CQS.Genome.Gtf;
using Bio.IO.SAM;
using CQS.Genome.Bed;
using CQS.Genome.Fastq;
using System.Collections.Concurrent;
using System.Threading;
using RCPA.Seq;
using CQS.Genome.Mapping;

namespace CQS.Genome.Mirna
{
  public class MirnaCountProcessor : AbstractCountProcessor<MirnaCountProcessorOptions>
  {
    public MirnaCountProcessor(MirnaCountProcessorOptions options):base(options){}

    public override IEnumerable<string> Process(string fileName)
    {
      var result = GetResultFilename(fileName);

      //parsing reads
      int totalReadCount;
      int mappedReadCount;
      List<SAMAlignedItem> reads = ParseCandidates(fileName, result, out totalReadCount, out mappedReadCount);

      //Initialize gff map
      Progress.SetMessage("initializing sequence region map...");
      var srItems = GetSequenceRegions("miRNA");
      var mappedregions = srItems.ConvertAll(m => new SequenceRegionMapped() { Region = m });
      var srMapped = srItems.ConvertAll(m =>
          (from p in options.Offsets
           select new SequenceRegionMapped()
           {
             Region = m,
             Offset = p
           }).ToDictionary(n => n.Offset)).ToDictionary(m => m[0].Region);

      Progress.SetMessage("mapping reads to sequence regions...");
      MapReadToSequenceRegion(srItems, reads, srMapped);

      //remove position 1,2 based on position 0; remove position 2 based on position 1 ...
      Progress.SetMessage("filter reads by positions...");
      filterPositions(srMapped);

      var featureReadCount = reads.Where(m => m.Locations.Any(n => n.Features.Count > 0)).Sum(m => m.QueryCount);
      Console.WriteLine("feature reads = {0}", featureReadCount);

      //group by miRNA name
      Progress.SetMessage("grouping by miRNA name...");
      var mirnas = GetMirnaResult(srMapped);

      if (!options.NoMappedFile)
      {
        Progress.SetMessage("output mapping details...");
        //new MappedMiRNAFileFormat(this.positions).WriteToFile(result + ".mapped", mirnas);
        new MappedMirnaGroupXmlFileFormat().WriteToFile(result + ".mapped.xml", mirnas);
      }

      Progress.SetMessage("write result ...");
      new MirnaCountFileWriter(options.Offsets).WriteToFile(result, mirnas);

      Progress.SetMessage("output unmapped query...");
      var unmappedFile = Path.ChangeExtension(result, ".unmapped.fastq");
      HashSet<string> except = new HashSet<string>(from g in mirnas
                                                   from loc in g.GetAlignedLocations()
                                                   select loc.Parent.Qname);
      int failedCount;
      if (File.Exists(options.FastqFile))
      {
        failedCount = new FastqExtractorFromFastq() { Progress = this.Progress }.Extract(options.FastqFile, unmappedFile, except);
      }
      else
      {
        failedCount = new FastqExtractorFromBam(options.Samtools) { Progress = this.Progress }.Extract(fileName, unmappedFile, except);
      }

      Progress.SetMessage("summarizing ...");
      var infoFile = Path.ChangeExtension(result, ".info");
      using (StreamWriter sw = new StreamWriter(infoFile))
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
        sw.WriteLine("MappedReads\t{0}", mappedReadCount );
        sw.WriteLine("MultipleMappedReads\t{0}", reads.Where(m => m.Locations.Count > 1).Sum(m => m.QueryCount));
        sw.WriteLine("FeatureReads\t{0}", featureReadCount);
      }

      Progress.End();

      return new[] { result, infoFile, unmappedFile };
    }

    private List<MappedMirnaGroup> GetMirnaResult(Dictionary<ISequenceRegion, Dictionary<int, SequenceRegionMapped>> srMapped)
    {
      //merge sequence region by feature name (same miRNA)
      var grp = srMapped.ToGroupDictionary(m => m.Key.Name);

      var result = new List<MappedMirnaGroup>();
      foreach (var g in grp)
      {
        var group = new MappedMirnaGroup();
        result.Add(group);

        var mirna = new MappedMirna();
        group.Add(mirna);

        mirna.Name = g.Key;
        mirna.Sequence = g.Value.First().Key.Sequence;
        foreach (var sr in g.Value)
        {
          var item = new MappedMirnaRegion();
          mirna.MappedRegions.Add(item);

          item.Region = sr.Key;
          item.Mapped = sr.Value;
        }
      }

      //merge miRNA with same sequence
      if (result.Count > 0 && !string.IsNullOrEmpty(result[0][0].Sequence))
      {
        var sequenceGrp = result.GroupBy(m => m[0].Sequence).ToList().ConvertAll(m => (from n in m
                                                                                    orderby n.DisplayName
                                                                                    select n).ToList()).ToList();
        Console.WriteLine("There are {0}/{1} distinct miRNAs quantified!", sequenceGrp.Count, result.Count);

        result = new List<MappedMirnaGroup>();
        foreach (var mirnas in sequenceGrp)
        {
          if (mirnas.Count > 1)
          {
            for (int i = 1; i < mirnas.Count; i++)
            {
              mirnas[0].AddRange(mirnas[i]);
            }
          }
          result.Add(mirnas[0]);
        }
      }

      result.RemoveAll(m => m.EstimateCount == 0);
      result.Sort((m1, m2) => m1.DisplayName.CompareTo(m2.DisplayName));

      Progress.SetMessage("sorting miRNA locations...");
      foreach (var g in result)
      {
        foreach (var m in g)
        {
          if (m.MappedRegions.Count > 1)
          {
            m.MappedRegions.Sort((m1, m2) => m2.GetTotalReadCount().CompareTo(m1.GetTotalReadCount()));
          }
        }
      }

      return result;
    }

    /// <summary>
    /// Once one query has been mapped to high prilority locus, the low prilority locuses will be removed.
    /// </summary>
    /// <param name="srMapped"></param>
    private void filterPositions(Dictionary<ISequenceRegion, Dictionary<int, SequenceRegionMapped>> srMapped)
    {
      var removequeries = new HashSet<string>();
      for (int pfrom = 0; pfrom < options.Offsets.Count; pfrom++)
      {
        var pf = options.Offsets[pfrom];
        removequeries.UnionWith(from srms in srMapped.Values
                                from mq in srms[pf].AlignedLocations
                                select mq.Parent.Qname);

        for (int pto = pfrom + 1; pto < options.Offsets.Count; pto++)
        {
          var pt = options.Offsets[pto];
          var srms = (from v in srMapped.Values
                      where v[pt].AlignedLocations.Count > 0
                      select v[pt]).ToList();
          foreach (var srm in srms)
          {
            for (int i = srm.AlignedLocations.Count - 1; i >= 0; i--)
            {
              var loc = srm.AlignedLocations[i];
              if (removequeries.Contains(loc.Parent.Qname))
              {
                loc.Features.Remove(srm.Region);
                srm.AlignedLocations.RemoveAt(i);
              }
            }
          }
        }
      }
    }

    private void MapReadToSequenceRegion(List<GtfItem> srItems, List<SAMAlignedItem> reads, Dictionary<ISequenceRegion, Dictionary<int, SequenceRegionMapped>> srMapped)
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

      Progress.SetRange(0, (long)srItems.Count);
      var gmapped = new Dictionary<string, SAMAlignedItem>();
      foreach (var srItem in srItems)
      {
        Progress.Increment(1);
        Dictionary<char, List<SAMAlignedLocation>> curMatchedMap;

        if (!chrStrandMatchedMap.TryGetValue(srItem.Seqname, out curMatchedMap))
        {
          continue;
        }

        var curSrPositions = srMapped[srItem];

        //mapped query must have same oritation with miRNA defined at gff or bed file.
        var matches = curMatchedMap[srItem.Strand];
        foreach (var m in matches)
        {
          SequenceRegionMapped curmapped = FindMatch(srItem, curSrPositions, m);

          if (curmapped != null)
          {
            m.Features.Add(curmapped.Region);
            curmapped.AlignedLocations.Add(m);
          }
        }
      }
    }

    private SequenceRegionMapped FindMatch(ISequenceRegion srItem, Dictionary<int, SequenceRegionMapped> curSrPositions, SAMAlignedLocation loc)
    {
      if (srItem.Strand == '+')
      {
        foreach (var pffset in options.Offsets)
        {
          if (loc.Start == srItem.Start + pffset)
          {
            return curSrPositions[pffset];
          }
        }
      }
      else
      {
        foreach (var offset in options.Offsets)
        {
          if (loc.End == srItem.End - offset)
          {
            return curSrPositions[offset];
          }
        }
      }
      return null;
    }
  }
}
