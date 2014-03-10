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
using CQS.Commandline;
using CommandLine;
using System.Text.RegularExpressions;
using CQS.Genome.Mirna;

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

      using (StreamWriter sw = new StreamWriter(options.OutputFile))
      {
        sw.Write("Feature\tLocation");

        var hassequence = !string.IsNullOrEmpty(counts.First().DisplayNameGroupMap.First().Value.First().Sequence);

        if (hassequence)
        {
          sw.Write("\tSequence");
        }

        sw.WriteLine("\t" + (from c in counts select c.ItemName).Merge("\t"));

        foreach (var feature in features)
        {
          var first = counts.FirstOrDefault(m => m.DisplayNameGroupMap.ContainsKey(feature));
          var item = first.DisplayNameGroupMap[feature];
          sw.Write(feature + "\t" + item.DisplayLocation);
          if (hassequence)
          {
            sw.Write("\t" + item.DisplaySequence);
          }
          for (int i = 0; i < counts.Count; i++)
          {
            if (counts[i].DisplayNameGroupMap.ContainsKey(feature))
            {
              sw.Write("\t{0:0.##}", counts[i].DisplayNameGroupMap[feature].EstimateCount);
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
