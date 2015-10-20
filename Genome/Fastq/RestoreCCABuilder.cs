using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RCPA;
using System.IO;
using CQS.Genome.Fastq;

namespace CQS.Genome.Fastq
{
  public class RestoreCCABuilder : AbstractThreadProcessor
  {
    private RestoreCCABuilderOptions options;

    public RestoreCCABuilder(RestoreCCABuilderOptions options)
    {
      this.options = options;
    }

    public override IEnumerable<string> Process()
    {
      var parser = new FastqReader();

      Progress.SetMessage("Reading " + options.InputFile + "...");
      var ccs = new Dictionary<string, string>();
      using (var sr = StreamUtils.GetReader(options.InputFile))
      {
        FastqSequence seq;
        int readcount = 0;
        while ((seq = parser.Parse(sr)) != null)
        {
          readcount++;
          if (readcount % 100000 == 0)
          {
            Progress.SetMessage("{0} / {1} reads end with CC found", ccs.Count, readcount);
          }

          if (seq.SeqString.EndsWith("CC"))
          {
            ccs[seq.Name] = seq.SeqString;
          }
        }
      }

      Dictionary<string, FastqSequence> queries = new Dictionary<string, FastqSequence>();
      Progress.SetMessage("Processing " + options.UntrimmedFile + " and writing to " + options.OutputFile + "...");
      var writer = new FastqWriter();
      using (var sr = StreamUtils.GetReader(options.UntrimmedFile))
      {
        using (var sw = new StreamWriter(options.OutputFile))
        {
          sw.WriteLine("Name\tIsCCA");
          FastqSequence seq;
          int readcount = 0;

          parser.AcceptName = m => ccs.ContainsKey(m);

          while ((seq = parser.Parse(sr)) != null)
          {
            readcount++;
            if (readcount % 10000 == 0)
            {
              Progress.SetMessage("{0} reads end with CC processed", readcount);
            }

            string sequence = ccs[seq.Name];
            var pos = seq.SeqString.IndexOf(sequence);
            if (pos == -1)
            {
              throw new Exception(string.Format("Cannot find trimmed sequence {0} in untrimmed sequence {1} of read {2}", sequence, seq.SeqString, seq.Name));
            }

            var nextseq = seq.SeqString.Substring(pos + sequence.Length);
            sw.WriteLine("{0}\t{1}", seq.Name, nextseq.StartsWith("A"));
            ccs.Remove(seq.Name);
          }
        }
      }

      if (ccs.Count != 0)
      {
        var unfoundFile = options.OutputFile + ".unfound";
        using (var sw = new StreamWriter(unfoundFile))
        {
          ccs.ForEach(m => sw.WriteLine(m.Key));
        }
        throw new Exception(string.Format("Cannot find {0} reads in untrimmed file, saved to {1}", ccs.Count, unfoundFile));
      }

      Progress.End();

      return new[] { options.OutputFile };
    }
  }
}
