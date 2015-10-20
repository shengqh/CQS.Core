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
using RCPA.Commandline;
using CommandLine;
using System.Text.RegularExpressions;
using CQS.Genome.Mapping;

namespace CQS.Genome.Mirna
{
  public class MirnaCountTableBuilder : AbstractThreadProcessor
  {
    private MirnaCountTableBuilderOptions options;

    public MirnaCountTableBuilder(MirnaCountTableBuilderOptions options)
    {
      this.options = options;
    }

    private class CountItem
    {
      public string Dir { get; set; }
      public Dictionary<string, string[]> Data { get; set; }
    }

    public override IEnumerable<string> Process()
    {
      var countfiles = options.GetCountFiles();

      List<CountItem> counts = new List<CountItem>();
      foreach (var file in countfiles)
      {
        Progress.SetMessage("reading " + file.File + " ...");
        var count = new CountItem()
        {
          Dir = file.Name,
          Data =
            (from line in File.ReadAllLines(file.File).Skip(1)
             let parts = line.Split('\t')
             where parts.Length >= 6
             select parts).ToDictionary(m => m[0])
        };
        counts.Add(count);
      }

      var features = (from c in counts
                      from k in c.Data.Keys
                      select k).Distinct().OrderBy(m => m).ToList();

      var checkSequence = counts.First().Data.First().Value[2];
      double value;
      var hasSequence = !double.TryParse(checkSequence, out value);
      var seqheader = hasSequence ? "\tSequence" : "";
      var startIndex = hasSequence ? 3 : 2;

      using (StreamWriter sw = new StreamWriter(options.OutputFile))
      using (StreamWriter swIso = new StreamWriter(options.IsomirFile))
      {

        sw.WriteLine("Feature\tLocation{0}\t{1}", seqheader, (from c in counts select c.Dir).Merge("\t"));
        swIso.WriteLine("Feature\tLocation{0}\t{1}", seqheader, (from c in counts select c.Dir).Merge("\t"));

        foreach (var feature in features)
        {
          OutputCount(counts, sw, feature, startIndex, "", hasSequence);
          OutputCount(counts, swIso, feature, startIndex + 1, "_+_0", hasSequence);
          OutputCount(counts, swIso, feature, startIndex + 2, "_+_1", hasSequence);
          OutputCount(counts, swIso, feature, startIndex + 3, "_+_2", hasSequence);
        }
      }

      var result = new[] { options.OutputFile, options.IsomirFile }.ToList();

      var infofile = Path.ChangeExtension(options.OutputFile, ".info");
      if (CountUtils.WriteInfoSummaryFile(infofile, options.GetCountFiles().ToDictionary(m => m.Name, m => m.File)))
      {
        result.Add(infofile);
      }

      return result;
    }

    private static void OutputCount(List<CountItem> counts, StreamWriter sw, string feature, int index, string indexSuffix, bool hasSequence)
    {
      if (!(from count in counts
            where count.Data.ContainsKey(feature)
            select count.Data[feature]).All(m => string.IsNullOrEmpty(m[index]) || m[index].Equals("0")))
      {
        var fea = counts.First(m => m.Data.ContainsKey(feature)).Data[feature];
        var seqvalue = hasSequence ? "\t" + fea[2] : "";
        sw.Write(feature + indexSuffix + "\t" + fea[1] + seqvalue);

        foreach (var count in counts)
        {
          if (count.Data.ContainsKey(feature))
          {
            sw.Write("\t" + count.Data[feature][index]);
          }
          else
          {
            sw.Write("\t0");
          }
        }
        sw.WriteLine();
      }
    }
  }
}
