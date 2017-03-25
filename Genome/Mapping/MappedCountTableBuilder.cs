using CQS.Genome.SmallRNA;
using RCPA;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CQS.Genome.Mapping
{
  public class MappedCountTableBuilder : AbstractThreadProcessor
  {
    private SimpleDataTableBuilderOptions options;

    public MappedCountTableBuilder(SimpleDataTableBuilderOptions options)
    {
      this.options = options;
    }

    public override IEnumerable<string> Process()
    {
      var counts = new MappedCountItemList();

      var features = counts.Parse(options, m => SmallRNAUtils.SortNames(m));

      var featureCounts = (from feature in features
                           let count = MathNet.Numerics.Statistics.Statistics.Median(from c in counts
                                                                                     select c.DisplayNameGroupMap.ContainsKey(feature) ? c.DisplayNameGroupMap[feature].GetEstimatedCount() : 0)
                           select new { Feature = feature, Count = count }).OrderByDescending(m => m.Count).ToList();

      using (StreamWriter sw = new StreamWriter(options.OutputFile))
      {
        sw.Write("Feature\tLocation");

        var hassequence = !string.IsNullOrEmpty(counts.First().DisplayNameGroupMap.First().Value.First().Sequence);

        if (hassequence)
        {
          sw.Write("\tSequence");
        }

        sw.WriteLine("\t" + (from c in counts select c.ItemName).Merge("\t"));

        foreach (var fc in featureCounts)
        {
          var feature = fc.Feature;
          var first = counts.FirstOrDefault(m => m.DisplayNameGroupMap.ContainsKey(feature));
          var item = first.DisplayNameGroupMap[feature];
          sw.Write(feature + "\t" + item.DisplayLocations);
          if (hassequence)
          {
            sw.Write("\t" + item.DisplaySequence);
          }
          for (int i = 0; i < counts.Count; i++)
          {
            if (counts[i].DisplayNameGroupMap.ContainsKey(feature))
            {
              sw.Write("\t{0:0.##}", counts[i].DisplayNameGroupMap[feature].GetEstimatedCount());
            }
            else
            {
              sw.Write("\t0");
            }

          }
          sw.WriteLine();
        }
      }

      var result = new[] { Path.GetFullPath(options.OutputFile) }.ToList();

      var infofile = Path.ChangeExtension(options.OutputFile, ".info");
      if (CountUtils.WriteInfoSummaryFile(infofile, options.GetCountFiles().ToDictionary(m => m.Name, m => m.File)))
      {
        result.Add(infofile);
      }

      return new string[] { Path.GetFullPath(options.OutputFile) };
    }
  }
}
