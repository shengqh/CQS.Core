using System.Collections.Generic;
using System.IO;
using RCPA;

namespace CQS.Genome.Fastq
{
  public class Bam2SingleFastqProcessor : AbstractThreadProcessor
  {
    private readonly Bam2FastqProcessorOptions _options;

    public Bam2SingleFastqProcessor(Bam2FastqProcessorOptions option)
    {
      _options = option;
    }

    public override IEnumerable<string> Process()
    {
      Progress.SetMessage("This single end bam file is not sorted by name, it will cost more time/memory to generate fastq files ...");

      var output = _options.OutputPrefix + ".fastq";
      if (!_options.UnGzipped)
      {
        output = output + ".gz";
      }

      var tmp = output + ".tmp";

      using (var sw = StreamUtils.GetWriter(tmp, !_options.UnGzipped))
      {
        using (var sr = new FastqItemBAMParser(_options.InputFile))
        {
          FastqItem ss;
          var count = 0;
          while ((ss = sr.ParseNext()) != null)
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

            ss.WriteFastq(sw);
            sr.IgnoreQuery.Add(ss.Qname);
          }
        }
      }

      if (File.Exists(output))
      {
        File.Delete(output);
      }
      File.Move(tmp, output);

      return new[] { output };
    }
  }
}