using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RCPA;
using System.IO;
using CQS.Genome.Fastq;

namespace CQS.Genome.Fastq
{
  public class FastqTrimmer : AbstractThreadProcessor
  {
    private FastqTrimmerOptions options;

    public FastqTrimmer(FastqTrimmerOptions options)
    {
      this.options = options;
    }

    public override IEnumerable<string> Process()
    {
      var result = new List<string>();

      var fastqFile = options.OutputFile;
      result.Add(fastqFile);

      Progress.SetMessage("Processing " + options.InputFile + " and writing to " + fastqFile + "...");
      var parser = new FastqReader();
      var writer = new FastqWriter();

      int readcount = 0;
      using (var sr = StreamUtils.GetReader(options.InputFile))
      {
        using (var sw = StreamUtils.GetWriter(fastqFile, options.Gzipped))
        {
          foreach (FastqSequence seq in parser.Parse(sr))
          {
            readcount++;
            if (readcount % 100000 == 0)
            {
              Progress.SetMessage("{0} reads processed", readcount);
            }

            if (options.Last > 0)
            {
              seq.SeqString = seq.SeqString.Substring(0, options.Last);
              seq.Score = seq.Score.Substring(0, options.Last);
            }

            if (options.Start > 1)
            {
              seq.SeqString = seq.SeqString.Substring(options.Start - 1);
              seq.Score = seq.Score.Substring(options.Start - 1);
            }

            if (options.TrimN)
            {
              while (seq.SeqString.StartsWith("N"))
              {
                seq.SeqString = seq.SeqString.Substring(1);
                seq.Score = seq.Score.Substring(1);
              }
              while (seq.SeqString.EndsWith("N"))
              {
                seq.SeqString = seq.SeqString.Substring(0, seq.SeqString.Length - 1);
                seq.Score = seq.Score.Substring(0, seq.Score.Length - 1);
              }
            }

            if (options.MinimumLength > 0 && seq.SeqString.Length < options.MinimumLength)
            {
              continue;
            }

            writer.Write(sw, seq);
          }
        }
      }

      Progress.End();

      return result;
    }
  }
}
