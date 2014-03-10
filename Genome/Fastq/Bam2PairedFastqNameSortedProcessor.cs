using System;
using System.Collections.Generic;
using System.IO;
using RCPA;

namespace CQS.Genome.Fastq
{
  public class Bam2PairedFastqNameSortedProcessor : AbstractThreadProcessor
  {
    private readonly Bam2FastqProcessorOptions _options;

    public Bam2PairedFastqNameSortedProcessor(Bam2FastqProcessorOptions option)
    {
      _options = option;
    }

    public override IEnumerable<string> Process()
    {
      Progress.SetMessage("This pair end bam file has been sorted by name, it will cost less time to generate fastq files ...");

      var output1 = _options.OutputPrefix + ".1.fastq";
      var output2 = _options.OutputPrefix + ".2.fastq";
      if (!_options.UnGzipped)
      {
        output1 = output1 + ".gz";
        output2 = output2 + ".gz";
      }
      var tmp1 = output1 + ".tmp";
      var tmp2 = output2 + ".tmp";
      var output3 = Path.ChangeExtension(_options.OutputPrefix, ".orphan.fastq");

      var paired = new FastqItem[3];
      string lastname = null;
      using (var sw1 = StreamUtils.GetWriter(tmp1, !_options.UnGzipped))
      {
        using (var sw2 = StreamUtils.GetWriter(tmp2, !_options.UnGzipped))
        {
          using (var sw3 = new StreamWriter(output3))
          {
            var sw = new[] { null, sw1, sw2, sw3 };

            using (var sr = new FastqItemBAMParser(_options.InputFile))
            {
              FastqItem ss;
              var count = 0;
              var outputCount = 0;
              while ((ss = sr.ParseNext()) != null)
              {
                if (string.IsNullOrEmpty(ss.Qname))
                {
                  throw new Exception(string.Format("Entry after {0} has empty name", lastname));
                }

                ss.CheckPairedName();

                if (string.IsNullOrEmpty(ss.Qname))
                {
                  throw new Exception(string.Format("After check paired name, entry after {0} has empty name", lastname));
                }

                count++;

                if (count % 100000 == 0)
                {
                  Progress.SetMessage("{0} reads processed.", count);
                  if (Progress.IsCancellationPending())
                  {
                    throw new UserTerminatedException();
                  }
                }

                if (lastname == null)
                {
                  paired[ss.PairIndex] = ss;
                  lastname = ss.PairName;
                  continue;
                }

                if (ss.PairName.Equals(lastname))
                {
                  paired[ss.PairIndex] = ss;
                  continue;
                }

                outputCount = WriteFastq(paired, sw, outputCount);

                paired[1] = null;
                paired[2] = null;
                paired[ss.PairIndex] = ss;
                lastname = ss.PairName;
              }

              WriteFastq(paired, sw, outputCount);
            }
          }
        }
      }

      if (File.Exists(output1))
      {
        File.Delete(output1);
      }
      File.Move(tmp1, output1);

      if (File.Exists(output2))
      {
        File.Delete(output2);
      }
      File.Move(tmp2, output2);

      if (new FileInfo(output3).Length == 0)
      {
        File.Delete(output3);
      }

      return new[] { output1, output2 };
    }

    private int WriteFastq(FastqItem[] paired, StreamWriter[] sw, int outputCount)
    {
      if (paired[1] != null && paired[2] != null)
      {
        paired[1].WriteFastq(sw[1]);
        paired[2].WriteFastq(sw[2]);

        outputCount++;
        if (outputCount % 1000000 == 0)
        {
          GC.Collect();
          GC.WaitForPendingFinalizers();
          Progress.SetMessage(string.Format("{0} paired reads processed, cost memory: {1} MB", outputCount, (GC.GetTotalMemory(true) / 1048576)));
        }
      }
      else if (paired[1] != null)
      {
        paired[1].WriteFastq(sw[3]);
      }
      else
      {
        paired[2].WriteFastq(sw[3]);
      }
      return outputCount;
    }
  }
}