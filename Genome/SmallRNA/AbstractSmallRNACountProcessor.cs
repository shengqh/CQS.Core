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

    public void DrawPositionImageCategory(List<SAMAlignedItem> notNTAreads, List<FeatureLocation> featureLocations, string category, bool positionByPercentage = false, int minimumReadCount = 5)
    {
      var positionFile = Path.ChangeExtension(options.OutputFile, "." + category + ".position");
      DrawPositionImage(notNTAreads, featureLocations.Where(m => m.Category.Equals(category)).ToList(), category, positionFile, positionByPercentage, minimumReadCount);
    }

    public void DrawPositionImage(List<SAMAlignedItem> notNTAreads, List<FeatureLocation> features, string category, string positionFile, bool positionByPercentage = false, int minimumReadCount = 5)
    {
      if (features.Count > 0)
      {
        Progress.SetMessage("mapping reads overlapped to {0} {1} entries.", features.Count, category);

        //map reads to miRNA without considering NTA and offset.
        var map = BuildStrandMap(notNTAreads);

        new SmallRNAMapper(category, options, m => true).MapReadToFeature(features, map);

        SmallRNAMappedPositionBuilder.Build(features.GroupByName().GroupBySequence(), Path.GetFileNameWithoutExtension(options.OutputFile), positionFile, m => m[0].Name.StringAfter(":"), positionByPercentage, minimumReadCount);

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