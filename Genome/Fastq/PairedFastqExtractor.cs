using RCPA;
using System.Collections.Generic;
using System.IO;

namespace CQS.Genome.Fastq
{
  public class PairedFastqExtractor : AbstractThreadFileProcessor
  {
    private PairedFastqExtractorOptions options;

    public PairedFastqExtractor(PairedFastqExtractorOptions options)
    {
      this.options = options;
    }

    public override IEnumerable<string> Process(string fileName)
    {
      IFilter<FastqSequence> filter = options.GetFilter();
      using (GzipTextReader gz1 = new GzipTextReader(options.Gzip, options.FastqFiles[0]))
      using (GzipTextReader gz2 = new GzipTextReader(options.Gzip, options.FastqFiles[1]))
      using (StreamWriter sw1 = new StreamWriter(options.OutputFiles[0]))
      using (StreamWriter sw2 = new StreamWriter(options.OutputFiles[1]))
      {
        FastqReader reader = new FastqReader();
        FastqWriter writer = new FastqWriter();
        var count = 0;
        while (true)
        {
          var q1 = reader.Parse(gz1.Reader);
          var q2 = reader.Parse(gz2.Reader);
          if (q1 == null || q2 == null)
          {
            break;
          }

          count++;

          if (count % 100000 == 0)
          {
            Progress.SetMessage("{0} reads", count);
            if (Progress.IsCancellationPending())
            {
              throw new UserTerminatedException();
            }
          }

          if (filter.Accept(q1) && filter.Accept(q2))
          {
            writer.Write(sw1, q1);
            writer.Write(sw2, q2);
          }
        }
      }

      return options.OutputFiles;
    }
  }
}
