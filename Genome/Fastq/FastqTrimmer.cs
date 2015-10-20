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

      result.AddRange(options.OutputFiles);

      Progress.SetMessage("Processing " + options.InputFiles.Merge(",") + " and writing to " + options.OutputFiles.Merge(",") + "...");
      var writer = new FastqWriter();

      int readcount = 0;
      var srs = new List<StreamReader>();
      var sws = new List<StreamWriter>();
      var parsers = new FastqReader();
      try
      {
        Progress.SetMessage("Opening input files ...");
        srs.AddRange(from input in options.InputFiles select StreamUtils.GetReader(input));

        Progress.SetMessage("Opening output files ...");
        sws.AddRange(from output in options.OutputFiles select StreamUtils.GetWriter(output, output.ToLower().EndsWith(".gz") || options.Gzipped));

        Progress.SetMessage("Reading sequences ...");
        while (true)
        {
          var seqs = (from sr in srs select parsers.Parse(sr)).ToArray();

          if (seqs.All(m => m == null))
          {
            break;
          }

          if (seqs.Any(m => m == null))
          {
            throw new Exception("The data is not properly paired :" + (from s in seqs where s != null select s.Name).Merge(" ! "));
          }

          if (seqs.Length > 1)
          {
            var names = (from seq in seqs
                         select seq.Name.StringBefore(" ").StringBefore("/1").StringBefore("/2")).ToArray();
            if (names.Any(m => !m.Equals(names[0])))
            {
              throw new Exception("The data is not properly paired: " + names.Merge(" ! "));
            }
          }

          readcount++;
          if (readcount % 100000 == 0)
          {
            Progress.SetMessage("{0} reads processed", readcount);
          }

          if (options.Last > 0)
          {
            seqs.ForEach(seq =>
            {
              seq.SeqString = seq.SeqString.Substring(0, options.Last);
              seq.Score = seq.Score.Substring(0, options.Last);
            });
          }

          if (options.Start > 1)
          {
            seqs.ForEach(seq =>
            {
              seq.SeqString = seq.SeqString.Substring(options.Start - 1);
              seq.Score = seq.Score.Substring(options.Start - 1);
            });
          }

          if (options.TrimN)
          {
            seqs.ForEach(seq =>
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
            });
          }

          if (options.MinimumLength > 0 && seqs.Any(m => m.SeqString.Length < options.MinimumLength))
          {
            continue;
          }

          for (int i = 0; i < seqs.Length; i++)
          {
            writer.Write(sws[i], seqs[i]);
          }
        }
      }
      finally
      {
        srs.ForEach(m => m.Close());
        sws.ForEach(m => m.Close());
      }

      Progress.End();

      return result;
    }
  }
}
