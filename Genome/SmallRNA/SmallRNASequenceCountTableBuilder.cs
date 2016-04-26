using CQS.Genome.Fastq;
using RCPA;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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
        var keptNames = new HashSet<string>();
        Func<KeyValuePair<string, MapItem>, bool> accept;
        if (File.Exists(file.AdditionalFile)) // keep the read in fastq file only
        {
          Progress.SetMessage("Reading " + file.AdditionalFile + "...");
          var fastqReader = new FastqReader();
          using (var sr = StreamUtils.GetReader(file.AdditionalFile))
          {
            FastqSequence fs;
            while ((fs = fastqReader.Parse(sr)) != null)
            {
              var curname = fs.Name.StringBefore(SmallRNAConsts.NTA_TAG);
              //Console.Error.WriteLine(curname);
              keptNames.Add(curname);
            }
          }

          accept = m => keptNames.Contains(m.Value.Information);
        }
        else
        {
          accept = m => true;
        }

        Progress.SetMessage("Reading " + file.File + "...");
        counts[file.Name] = new MapItemReader("Sequence", "Count", '\t', true, "Query").ReadFromFile(file.File).Where(m => accept(m)).ToDictionary(m => m.Key, m => double.Parse(m.Value.Value));
      }

      var samples = counts.Keys.OrderBy(m => m).ToArray();

      var sequences = (from map in counts.Values
                       let smap = map.OrderByDescending(m => m.Value)
                       let count = Math.Min(map.Count, options.TopNumber)
                       from seq in smap.Take(count)
                       select seq.Key).Distinct().ToArray();

      Progress.SetMessage("Total {0} distinct sequences selected", sequences.Length);

      var seqCounts = (from seq in sequences
                       let totalCount = (from c in counts.Values where c.ContainsKey(seq) select c[seq]).Sum()
                       select new { Sequence = seq, TotalCount = totalCount }).OrderByDescending(m => m.TotalCount).ToArray();

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

      result.Add(options.OutputFile);

      if (options.ExportFasta)
      {
        var fastaFile = options.OutputFile + ".fasta";
        Progress.SetMessage("Writing " + fastaFile + " ...");
        using (var sw = new StreamWriter(fastaFile))
        {
          int number = 0;
          foreach (var sc in seqCounts)
          {
            sw.WriteLine(">{0}_{1}", sc.Sequence, sc.TotalCount);
            sw.WriteLine("{0}", sc.Sequence);
            number++;
            if (number == options.ExportFastaNumber)
            {
              break;
            }
          }
        }
        result.Add(fastaFile);
      }

      Progress.End();

      return result;
    }
  }
}