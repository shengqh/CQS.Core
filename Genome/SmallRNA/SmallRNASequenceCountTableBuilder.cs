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
using RCPA;

namespace CQS.Genome.SmallRNA
{
  public class SmallRNASequenceCountTableBuilder : AbstractThreadProcessor
  {
    public Func<FastqSequence, bool> Accept { get; set; }

    private SmallRNASequenceCountTableBuilderOptions options;

    public SmallRNASequenceCountTableBuilder(SmallRNASequenceCountTableBuilderOptions options)
    {
      this.options = options;
    }

    public override IEnumerable<string> Process()
    {
      var result = new List<string>();

      var countfiles = options.GetCountFiles();
      var counts = new Dictionary<string, Dictionary<string, double>>();
      foreach (var file in countfiles)
      {
        Progress.SetMessage("Reading " + file.File + "...");
        counts[file.Name] = new MapItemReader("Sequence", "Count").ReadFromFile(file.File).ToDictionary(m => m.Key, m => double.Parse(m.Value.Value));
      }

      var samples = counts.Keys.OrderBy(m => m).ToArray();

      var sequences = (from map in counts.Values
                       let smap = map.OrderByDescending(m => m.Value)
                       let count = Math.Min(map.Count, options.TopNumber)
                       from seq in smap.Take(count)
                       select seq.Key).Distinct().ToArray();

      Progress.SetMessage("Total {0} distinct sequences selected", sequences.Length);

      var seqCounts = (from seq in sequences
                       let curCounts = (from c in counts.Values select c.ContainsKey(seq) ? c[seq] : 0).ToArray()
                       let median = MathNet.Numerics.Statistics.Statistics.Median(curCounts)
                       let mean = MathNet.Numerics.Statistics.Statistics.Mean(curCounts)
                       select new { Sequence = seq, Median = median, Mean = mean }).OrderByDescending(m => m.Median).ThenByDescending(m => m.Mean).ToArray();

      Progress.SetMessage("Writing " + options.OutputFile + "...");
      using (var sw = new StreamWriter(options.OutputFile))
      {
        sw.WriteLine("Sequence\t{0}", samples.Merge("\t"));
        foreach (var sc in seqCounts)
        {
          sw.WriteLine("{0}\t{1}", sc.Sequence, (from sample in samples
                                                 let map = counts[sample]
                                                 select map.ContainsKey(sc.Sequence) ? map[sc.Sequence].ToString() : "0").Merge("\t"));
        }
      }

      Progress.End();

      return result;
    }
  }
}