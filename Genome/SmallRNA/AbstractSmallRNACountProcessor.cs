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
  public abstract class AbstractSmallRNACountProcessor<T> : AbstractCountProcessor<T> where T : AbstractSmallRNACountProcessorOptions
  {
    public AbstractSmallRNACountProcessor(T options)
      : base(options)
    { }

    public void DrawPositionImage(List<SAMAlignedItem> notNTAreads, List<FeatureLocation> features, string category, string positionFile, bool positionByPercentage = false, int minimumReadCount = 5)
    {
      if (features.Count > 0)
      {
        Progress.SetMessage("mapping reads overlapped to {0} {1} entries.", features.Count, category);

        //map reads to miRNA without considering NTA and offset.
        var map = BuildStrandMap(notNTAreads);
        MapReadsToSmallRNA(features, map);

        SmallRNAMappedPositionBuilder.DrawImage(features.GroupByName().GroupBySequence(), Path.GetFileName(positionFile), positionFile, positionByPercentage, minimumReadCount);

        //clear all mapping information
        foreach (var read in notNTAreads)
        {
          foreach (var loc in read.Locations)
          {
            loc.Features.Clear();
          }
        }

        foreach (var feature in features)
        {
          feature.SamLocations.Clear();
        }
      }
    }

    protected void OrderFeatureItemGroup(List<FeatureItemGroup> mirnaGroups)
    {
      mirnaGroups.Sort((m1, m2) =>
      {
        var res = m1[0].Locations[0].Category.CompareTo(m2[0].Locations[0].Category);
        if (res == 0)
        {
          res = m2.GetEstimatedCount().CompareTo(m1.GetEstimatedCount());
        }
        if (res == 0)
        {
          res = m2.GetEstimatedCount(l => l.Offset == options.Offsets[0]).CompareTo(m1.GetEstimatedCount(l => l.Offset == options.Offsets[0]));
        }
        if (res == 0)
        {
          res = m1.Name.CompareTo(m2.Name);
        }
        return res;
      });
    }

    protected HashSet<SAMAlignedItem> GetMappedReads(List<FeatureLocation> features)
    {
      return new HashSet<SAMAlignedItem>(from m in features
                                         from l in m.SamLocations
                                         select l.SamLocation.Parent);
    }

    protected void RemoveReadsFromMap(Dictionary<string, Dictionary<char, List<SAMAlignedLocation>>> chrStrandMatchedMap, HashSet<SAMAlignedItem> mappedReads)
    {
      var qnames = new HashSet<string>(from r in mappedReads
                                       select r.Qname.StringBefore(SmallRNAConsts.NTA_TAG));

      foreach (var chr in chrStrandMatchedMap.Keys)
      {
        var strandMap = chrStrandMatchedMap[chr];
        foreach (var strand in strandMap.Keys)
        {
          var locs = strandMap[strand];
          locs.RemoveAll(m => qnames.Contains(m.Parent.Qname.StringBefore(SmallRNAConsts.NTA_TAG)));
        }
      }
    }

    public class AcceptResult
    {
      public bool Accepted { get; set; }
      public int NumberOfMismatch { get; set; }
      public int NumberOfNoPenaltyMutation { get; set; }
      public double OverlapPercentage { get; set; }
    }

    protected void DoMapReadsToFeatures(List<FeatureLocation> smallRNAs, Dictionary<string, Dictionary<char, List<SAMAlignedLocation>>> chrStrandMatchedMap, int maxMismatch, Func<FeatureLocation, SAMAlignedLocation, int, AcceptResult> accept)
    {
      //mapping to genome, considering offset limitation
      Progress.SetRange(0, smallRNAs.Count);
      foreach (var smallRNA in smallRNAs)
      {
        Progress.Increment(1);
        Dictionary<char, List<SAMAlignedLocation>> curMatchedMap;

        if (!chrStrandMatchedMap.TryGetValue(smallRNA.Seqname, out curMatchedMap))
        {
          continue;
        }

        //mapped query must have same oritation with miRNA defined at gff or bed file.
        var matches = curMatchedMap[smallRNA.Strand];
        foreach (var m in matches)
        {
          var r = accept(smallRNA, m, maxMismatch);
          if (r.Accepted)
          {
            var fsl = new FeatureSamLocation(smallRNA);
            fsl.SamLocation = m;
            fsl.NumberOfMismatch = r.NumberOfMismatch;
            fsl.NumberOfNoPenaltyMutation = r.NumberOfNoPenaltyMutation;
            fsl.OverlapPercentage = r.OverlapPercentage;
          }
        }
      }
    }

    public AcceptResult AcceptLocationPairMiRNA(FeatureLocation floc, SAMAlignedLocation sloc, int maxMismatch)
    {
      var result = AcceptLocationPairSmallRNA(floc, sloc, maxMismatch);

      if (!result.Accepted)
      {
        return result;
      }

      var offset = sloc.Offset(floc);
      result.Accepted = options.Offsets.Contains(offset);

      return result;
    }

    public AcceptResult AcceptLocationPairSmallRNA(FeatureLocation floc, SAMAlignedLocation sloc, int maxMismatch)
    {
      var result = CheckNoPenaltyMutation(floc, sloc, maxMismatch);
      if (!result.Accepted)
      {
        return result;
      }

      result.OverlapPercentage = floc.OverlapPercentage(sloc);
      result.Accepted = result.OverlapPercentage > 0 && result.OverlapPercentage >= options.MinimumOverlapPercentage;

      return result;
    }

    public AcceptResult CheckNoPenaltyMutation(FeatureLocation floc, SAMAlignedLocation sloc, int maxMismatch)
    {
      if (sloc.NumberOfNoPenaltyMutation > 0)
      {
        var polys = sloc.GetGsnapMismatches();
        var mismatch = 0;
        if (floc.Strand == sloc.Strand) //the non-penalty mutation has to be T2C
        {
          mismatch = polys.Count(m => m.RefAllele != 'T' || m.SampleAllele != 'C');
        }
        else
        {
          mismatch = polys.Count(m => m.RefAllele != 'A' || m.SampleAllele != 'G');
        }

        var nnpm = sloc.NumberOfMismatch + sloc.NumberOfNoPenaltyMutation - mismatch;
        if (mismatch > maxMismatch || nnpm > options.MaximumNoPenaltyMutationCount)
        {
          return new AcceptResult()
          {
            Accepted = false
          };
        }

        return new AcceptResult()
        {
          Accepted = true,
          NumberOfMismatch = mismatch,
          NumberOfNoPenaltyMutation = sloc.NumberOfMismatch + sloc.NumberOfNoPenaltyMutation - mismatch
        };
      }
      else if (sloc.NumberOfMismatch > maxMismatch)
      {
        return new AcceptResult()
        {
          Accepted = false
        };
      }
      else
      {
        return new AcceptResult()
        {
          Accepted = true,
          NumberOfMismatch = sloc.NumberOfMismatch,
          NumberOfNoPenaltyMutation = 0
        };
      }
    }

    public AcceptResult AcceptLocationPairLincRNA(FeatureLocation floc, SAMAlignedLocation sloc, int maxMismatch)
    {
      if (sloc.Parent.Sequence.Length < options.MinimumReadLengthForLincRNA)
      {
        return new AcceptResult()
          {
            Accepted = false
          };
      }

      return AcceptLocationPairSmallRNA(floc, sloc, maxMismatch);
    }

    protected void MapReadsToSmallRNA(List<FeatureLocation> smallRNAs, Dictionary<string, Dictionary<char, List<SAMAlignedLocation>>> chrStrandMatchedMap)
    {
      DoMapReadsToFeatures(smallRNAs, chrStrandMatchedMap, options.MaximumMismatch, AcceptLocationPairSmallRNA);
    }

    protected void MapReadsToLincRNA(List<FeatureLocation> lincRNAs, Dictionary<string, Dictionary<char, List<SAMAlignedLocation>>> chrStrandMatchedMap)
    {
      Progress.SetMessage("Filtering lincRNA with maximum mismatch {0} and minimum read length {1} ...", options.MaximumMismatchForLincRNA, options.MinimumReadLengthForLincRNA);

      DoMapReadsToFeatures(lincRNAs, chrStrandMatchedMap, options.MaximumMismatchForLincRNA, AcceptLocationPairLincRNA);
    }

    protected void MapReadsToMiRNA(List<FeatureLocation> miRNAs, Dictionary<string, Dictionary<char, List<SAMAlignedLocation>>> readMap)
    {
      if (readMap.Count == 0 || miRNAs.Count == 0)
      {
        return;
      }

      DoMapReadsToFeatures(miRNAs, readMap, options.MaximumMismatch, (m1, m2, maxMismatch) => AcceptLocationPairMiRNA(m1, m2, maxMismatch));

      //For each query, keep the one with the best offset
      var fsls = (from m in miRNAs
                  from l in m.SamLocations
                  select l).GroupBy(m => m.SamLocation.Parent).ToList().ConvertAll(m => m.ToArray());

      Progress.SetMessage("After mapping, there are {0} queries mapped to miRNA.", fsls.Count);

      foreach (var fsl in fsls)
      {
        if (fsl.Count() == 1)
        {
          continue;
        }

        var bestOffset = fsl.Min(m => options.Offsets.IndexOf(m.Offset));
        foreach (var f in fsl)
        {
          if (options.Offsets.IndexOf(f.Offset) != bestOffset)
          {
            f.FeatureLocation.SamLocations.Remove(f);
            f.SamLocation.Features.Remove(f.FeatureLocation);
          }
        }
      }

      //filter NTA
      if (readMap.First().Value.First().Value.First().Parent.Qname.Contains(SmallRNAConsts.NTA_TAG))
      {
        Progress.SetMessage("filter reads by clip information...");
        var mappedReads = (from m in miRNAs
                           from l in m.SamLocations
                           let loc = l.SamLocation
                           select new { Loc = loc, Qname = loc.Parent.Qname.StringBefore(SmallRNAConsts.NTA_TAG), NTA_Length = loc.Parent.Qname.StringAfter(SmallRNAConsts.NTA_TAG).Length }).ToList();

        var map = mappedReads.GroupBy(m => m.Qname);

        var sals = new HashSet<SAMAlignedLocation>();
        foreach (var locs in map)
        {
          //get smallest number of mismatch, then get shortest NTA
          var ntas = locs.GroupBy(m => m.Loc.NumberOfMismatch).OrderBy(m => m.Key).First().GroupBy(m => m.NTA_Length).OrderBy(m => m.Key).First();
          foreach (var nta in ntas)
          {
            sals.Add(nta.Loc);
          }
        }

        foreach (var m in miRNAs)
        {
          foreach (var l in new List<FeatureSamLocation>(m.SamLocations))
          {
            if (!sals.Contains(l.SamLocation))
            {
              m.SamLocations.Remove(l);
              l.SamLocation.Features.Remove(l.FeatureLocation);
            }
          }
        }
      }
    }

    public Dictionary<string, Dictionary<char, List<SAMAlignedLocation>>> BuildStrandMap(List<SAMAlignedItem> reads)
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
      return chrStrandMatchedMap;
    }

    protected void WriteSummaryFile(List<FeatureItemGroup> allmapped, int totalQueryCount, int totalMappedCount, string infoFile)
    {
      if (!File.Exists(infoFile) || !options.NotOverwrite)
      {
        Progress.SetMessage("summarizing ...");
        using (var sw = new StreamWriter(infoFile))
        {
          WriteOptions(sw);

          if (File.Exists(options.CountFile))
          {
            sw.WriteLine("#countFile\t{0}", options.CountFile);
          }

          sw.WriteLine("TotalReads\t{0}", totalQueryCount);

          //The mapped reads is the union of tRNA and otherRNA alignment result
          sw.WriteLine("MappedReads\t{0}", totalMappedCount);

          var featureReadCount = (from feature in allmapped
                                  from item in feature.GetAlignedLocations()
                                  select item.Parent.OriginalQname).Distinct().Sum(l => Counts.GetCount(l));
          sw.WriteLine("FeatureReads\t{0}", featureReadCount);

          //Output individual category
          foreach (var cat in SmallRNAConsts.Biotypes)
          {
            var count = (from c in allmapped
                         from l in c
                         where l.Name.StartsWith(cat)
                         select l.GetEstimatedCount()).Sum();
            if (count > 0)
            {
              sw.WriteLine("{0}\t{1:0.#}", cat, count);
            }
          }
        }
      }
    }

    protected virtual void WriteOptions(StreamWriter sw)
    {
      sw.WriteLine("#file\t{0}", options.InputFiles.Merge(","));
      sw.WriteLine("#coordinate\t{0}", options.CoordinateFile);
      sw.WriteLine("#minLength\t{0}", options.MinimumReadLength);
      sw.WriteLine("#maxMismatchCount\t{0}", options.MaximumMismatch);
    }
  }
}