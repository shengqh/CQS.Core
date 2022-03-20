using RCPA;
using System;
using System.Collections.Generic;
using System.IO;

namespace CQS.Genome.Fastq
{
  public class Bam2SingleFastqNameSortedProcessor : AbstractThreadProcessor
  {
    private readonly Bam2FastqProcessorOptions _options;

    public Bam2SingleFastqNameSortedProcessor(Bam2FastqProcessorOptions option)
    {
      _options = option;
    }

    public override IEnumerable<string> Process()
    {
      Progress.SetMessage("This single end bam file has been sorted by name, it will cost less time/memory to generate fastq files ...");

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
          string lastname = null;
          FastqItem ss;
          var count = 0;
          var outputCount = 0;
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

            if (!ss.Qname.Equals(lastname))
            {
              ss.WriteFastq(sw);
              lastname = ss.Qname;
              outputCount++;
              if (outputCount % 1000000 == 0)
              {
                GC.Collect();
                GC.WaitForPendingFinalizers();
                Progress.SetMessage(string.Format("{0} single reads processed, cost memory: {1} MB", outputCount, (GC.GetTotalMemory(true) / 1048576)));
              }
            }
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