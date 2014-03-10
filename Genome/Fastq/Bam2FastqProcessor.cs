using System;
using System.Collections.Generic;
using System.IO;
using CQS.Genome.Sam;
using RCPA;

namespace CQS.Genome.Fastq
{
  public class Bam2FastqProcessor : AbstractThreadProcessor
  {
    private readonly Bam2FastqProcessorOptions options;

    public Bam2FastqProcessor(Bam2FastqProcessorOptions option)
    {
      options = option;
    }

    public override IEnumerable<string> Process()
    {
      if (options.IsPaired)
      {
        return DoPairEndProcess();
      }
      return DoSingleEndProcess();
    }

    private IEnumerable<string> DoSingleEndProcess()
    {
      using (var sw = new StreamWriter(options.OutputFile))
      {
        using (var sr = SAMFactory.GetReader(options.InputFile, options.Samtools, true))
        {
          string line;
          var count = 0;
          var ignored = new HashSet<string>();
          while ((line = sr.ReadLine()) != null)
          {
            count++;

            if (count%100000 == 0)
            {
              Progress.SetMessage("{0} reads", count);
              if (Progress.IsCancellationPending())
              {
                throw new UserTerminatedException();
              }
            }

            try
            {
              var ss = LineToSamItem(line);
              Console.WriteLine(ss.Qname);
              if (ignored.Contains(ss.Qname))
              {
                continue;
              }

              ss.WriteFastq(sw);
              ignored.Add(ss.Qname);
            }
            catch (Exception ex)
            {
              Console.Error.WriteLine("Error of line {0} : {1}", line, ex.StackTrace);
              throw;
            }
          }
        }
      }
      return new[] {options.OutputFile};
    }

    private IEnumerable<string> DoPairEndProcess()
    {
      var map = new Dictionary<string, SAMItemSlim>();

      var output1 = Path.ChangeExtension(options.OutputFile, ".1" + Path.GetExtension(options.OutputFile));
      var output2 = Path.ChangeExtension(options.OutputFile, ".2" + Path.GetExtension(options.OutputFile));

      var ignored = new HashSet<string>();
      using (var sw1 = new StreamWriter(output1))
      {
        using (var sw2 = new StreamWriter(output2))
        {
          var sw = new[] {null, sw1, sw2};
          using (var sr = SAMFactory.GetReader(options.InputFile, options.Samtools, true))
          {
            string line;
            var count = 0;
            while ((line = sr.ReadLine()) != null)
            {
              count++;

              if (count%100000 == 0)
              {
                Progress.SetMessage("{0} reads", count);
                if (Progress.IsCancellationPending())
                {
                  throw new UserTerminatedException();
                }
              }

              var ss = LineToPairedSamItem(line);

              if (ignored.Contains(ss.Qname))
              {
                continue;
              }

              SAMItemSlim paired;
              if (map.TryGetValue(ss.Qname, out paired))
              {
                if (paired.Pos == ss.Pos)
                {
                  continue;
                }
                ss.WriteFastq(sw[ss.Pos], true);

                paired.WriteFastq(sw[paired.Pos], true);
                ignored.Add(ss.Qname);
                map.Remove(ss.Qname);
              }
              else
              {
                map[ss.Qname] = ss;
              }
            }
          }

          if (map.Count > 0)
          {
            var output3 = Path.ChangeExtension(options.OutputFile, ".orphan" + Path.GetExtension(options.OutputFile));
            using (var sw3 = new StreamWriter(output3))
            {
              foreach (var v in map.Values)
              {
                v.WriteFastq(sw3, true);
              }
            }
          }
        }
      }
      return new[] {output1, output2};
    }

    private static SAMItemSlim LineToPairedSamItem(string line)
    {
      var ss = LineToSamItem(line);

      if (ss.Qname.EndsWith("/1") || ss.Qname.EndsWith("#0"))
      {
        ss.Pos = 1;
      }
      else if (ss.Qname.EndsWith("/2"))
      {
        ss.Pos = 2;
      }
      else
      {
        throw new ArgumentException(string.Format("Reads are not paired: {0}", ss.Qname));
      }

      ss.Qname = ss.Qname.Substring(0, ss.Qname.Length - 2);
      return ss;
    }

    private static SAMItemSlim LineToSamItem(string line)
    {
      return SAMUtils.Parse<SAMItemSlim>(line);
    }
  }
}