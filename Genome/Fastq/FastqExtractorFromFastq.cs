using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using CQS.Genome.Sam;
using RCPA.Gui;
using Bio.IO.SAM;
using RCPA;

namespace CQS.Genome.Fastq
{
  public class FastqExtractorFromFastq : ProgressClass, IFastqExtractor
  {
    public IFilter<FastqSequence> Filter { get; set; }

    public int Extract(string sourceFile, string targetFile, IEnumerable<string> exceptQueryNames)
    {
      int result = 0;

      var except = new HashSet<string>(exceptQueryNames);

      using (var sw = new StreamWriter(targetFile))
      {
        using (var sr = StreamUtils.GetReader(sourceFile))
        {
          FastqReader reader = new FastqReader();
          FastqWriter writer = new FastqWriter();

          FastqSequence ss;
          var count = 0;
          while ((ss = reader.ParseOne(sr)) != null)
          {
            count++;

            if (count % 100000 == 0)
            {
              Progress.SetMessage("{0} reads", count);
              if (Progress.IsCancellationPending())
              {
                throw new UserTerminatedException();
              }
            }

            if (except.Contains(ss.Name))
            {
              continue;
            }

            if (Filter != null && !Filter.Accept(ss))
            {
              continue;
            }

            writer.Write(sw, ss);
            result++;
          }
        }
      }
      return result;
    }
  }
}
