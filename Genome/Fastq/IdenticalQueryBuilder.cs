using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RCPA;
using System.IO;
using CQS.Genome.Fastq;

namespace CQS.Genome.Fastq
{
  public class IdenticalQueryBuilder : AbstractThreadProcessor
  {
    private IdenticalQueryBuilderOptions options;

    public IdenticalQueryBuilder(IdenticalQueryBuilderOptions options)
    {
      this.options = options;
    }

    public override IEnumerable<string> Process()
    {
      var result = new List<string>();

      var fastqFile = options.OutputFile;
      if (!options.Gunzipped && !fastqFile.ToLower().EndsWith(".gz"))
      {
        fastqFile = fastqFile + ".gz";
      }
      result.Add(fastqFile);

      Dictionary<string, FastqSequence> queries = new Dictionary<string, FastqSequence>();
      Progress.SetMessage("Processing " + options.InputFile + " and writing to " + fastqFile + "...");
      var parser = new FastqReader();
      var writer = new FastqWriter();

      int readcount = 0;
      using (var sr = StreamUtils.GetReader(options.InputFile))
      {
        using (var sw = StreamUtils.GetWriter(fastqFile, !options.Gunzipped))
        {
          foreach (FastqSequence seq in parser.Parse(sr))
          {
            readcount++;
            if (readcount % 100000 == 0)
            {
              Progress.SetMessage("{0} reads processed", readcount);
            }

            if (seq.SeqString.Length < options.MinimumReadLength)
            {
              continue;
            }

            FastqSequence count;
            if (queries.TryGetValue(seq.SeqString, out count))
            {
              count.RepeatCount++;
              if (options.OutputScores)
              {
                count.RepeatScores.Add(seq.Score);
              }
              continue;
            }

            queries[seq.SeqString] = seq;
            if (options.OutputScores)
            {
              seq.RepeatScores.Add(seq.Score);
            }

            writer.Write(sw, seq);
          }
        }
      }

      Progress.End();

      var countFile = Path.ChangeExtension(fastqFile, ".dupcount");
      result.Add(countFile);
      Progress.SetMessage("writing duplicate count ...");
      using (StreamWriter sw = new StreamWriter(countFile))
      {
        sw.WriteLine("Query\tCount\tSequence");
        var seqs = (from s in queries
                    orderby s.Value.RepeatCount descending, s.Key
                    select s).ToList();
        foreach (var seq in seqs)
        {
          sw.WriteLine("{0}\t{1}\t{2}", seq.Value.Name, seq.Value.RepeatCount, seq.Value.SeqString);
        }
      }

      if (options.OutputScores)
      {
        Progress.SetMessage("writing score ...");
        var scoreFile = Path.ChangeExtension(fastqFile, ".scores");
        result.Add(scoreFile);
        using (StreamWriter sw = new StreamWriter(scoreFile))
        {
          sw.WriteLine("Query\tSequence\tPosition\tScores");
          var seqs = (from s in queries
                      orderby s.Value.RepeatCount descending, s.Key
                      select s.Value).ToList();
          foreach (var seq in seqs)
          {
            sw.WriteLine("{0}\t{1}", seq.Name, seq.SeqString);
            for (int i = 0; i < seq.SeqString.Length; i++)
            {
              Dictionary<char, int> count = new Dictionary<char, int>();
              foreach (var score in seq.RepeatScores)
              {
                int oldcount;
                if (count.TryGetValue(score[i], out oldcount))
                {
                  count[score[i]] = oldcount + 1;
                }
                else
                {
                  count[score[i]] = 1;
                }
              }

              sw.Write("\t\t{0}\t", i + 1);
              var keys = (from c in count.Keys orderby c select c).ToList();
              foreach (var key in keys)
              {
                sw.Write("{0}({1})", key, count[key]);
              }
              sw.WriteLine();
            }
          }
        }
      }

      return result;
    }
  }
}
