using RCPA;
using System;
using System.Collections.Generic;
using System.IO;

namespace CQS.Genome.Fastq
{
  public class Bam2PairedFastqProcessor : AbstractThreadProcessor
  {
    private readonly Bam2FastqProcessorOptions _options;

    public Bam2PairedFastqProcessor(Bam2FastqProcessorOptions option)
    {
      _options = option;
    }

    public override IEnumerable<string> Process()
    {
      Progress.SetMessage("This pair end bam file is not sorted by name, it will cost more time/memory to generate fastq files ...");
      var map = new Dictionary<string, FastqItem>();
      var output1 = _options.OutputPrefix + ".1.fastq";
      var output2 = _options.OutputPrefix + ".2.fastq";
      if (!_options.UnGzipped)
      {
        output1 = output1 + ".gz";
        output2 = output2 + ".gz";
      }
      var tmp1 = output1 + ".tmp";
      var tmp2 = output2 + ".tmp";

      using (var sw1 = StreamUtils.GetWriter(tmp1, !_options.UnGzipped))
      {
        using (var sw2 = StreamUtils.GetWriter(tmp2, !_options.UnGzipped))
        {
          var sw = new[] { null, sw1, sw2 };

          using (var sr = new FastqItemBAMParser(_options.InputFile))
          {
            FastqItem ss;
            var count = 0;
            var outputCount = 0;
            while ((ss = sr.ParseNext()) != null)
            {
              ss.CheckPairedName();

              count++;

              if (count % 100000 == 0)
              {
                Progress.SetMessage("{0} reads processed, {1} unpaired.", count, map.Count);
                if (Progress.IsCancellationPending())
                {
                  throw new UserTerminatedException();
                }
              }

              FastqItem paired;
              if (map.TryGetValue(ss.PairName, out paired))
              {
                if (paired.PairIndex == ss.PairIndex)
                {
                  continue;
                }
                ss.WriteFastq(sw[ss.PairIndex]);
                paired.WriteFastq(sw[paired.PairIndex]);
                sr.IgnoreQuery.Add(ss.Qname);
                sr.IgnoreQuery.Add(paired.Qname);
                map.Remove(ss.PairName);
                outputCount++;
                if (outputCount % 100000 == 0)
                {
                  var temp = new Dictionary<string, FastqItem>(map);
                  map.Clear();
                  map = temp;
                  GC.Collect();
                  GC.WaitForPendingFinalizers();
                  Progress.SetMessage("Cost memory: " + (GC.GetTotalMemory(true) / 1048576) + " MB");
                }
              }
              else
              {
                map[ss.PairName] = ss;
              }
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

      if (map.Count > 0)
      {
        var output3 = Path.ChangeExtension(_options.OutputPrefix, ".orphan.fastq");
        using (var sw3 = new StreamWriter(output3))
        {
          foreach (var v in map.Values)
          {
            v.WriteFastq(sw3);
          }
        }
      }

      return new[] { output1, output2 };
    }
  }
}