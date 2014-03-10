using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using CQS.Genome.Pileup;
using CQS.Genome.Statistics;
using RCPA.Seq;

namespace CQS.Genome.SomaticMutation
{
  public class PileupSingleProcessor : AbstractPileupProcessor
  {
    public PileupSingleProcessor(PileupOptions options)
      : base(options)
    {
    }

    protected override List<MpileupFisherResult> GetFisherFilterResults()
    {
      var result = new List<MpileupFisherResult>();
      Console.WriteLine("Single thread mode ...");
      var parser = _options.GetPileupItemParser();
      var pfile = new PileupFile(parser);
      switch (_options.From)
      {
        case PileupOptions.DataSourceType.Mpileup:
          pfile.Open(_options.MpileupFile);
          break;
        case PileupOptions.DataSourceType.BAM:
          var proc = ExecuteSamtools(_options.GetSamtoolsCommand(), _options.GetMpileupChromosomes());
          if (proc == null)
          {
            return null;
          }

          pfile.Open(proc.StandardOutput);
          pfile.Samtools = proc;
          break;
        case PileupOptions.DataSourceType.Console:
          pfile.Open(Console.In);
          break;
      }

      using (pfile)
      {
        try
        {
          var proc = new MpileupParseProcessor(_options);

          string line;
          while ((line = pfile.ReadLine()) != null)
          //while ((item = pfile.Next("1", 48901870)) != null)
          {
            _totalCount++;

            try
            {
              var item = proc.Parse(line);
              if (item == null)
              {
                continue;
              }

              result.Add(item);
            }
            catch (Exception ex)
            {
              Console.WriteLine("parsing error {0}\n{1}", ex.Message, line);
              return null;
            }
          }

          CopyCountInfo(proc);
        }
        finally
        {
          if (pfile.Samtools != null)
          {
            try
            {
              pfile.Samtools.Kill();
            }
            // ReSharper disable once EmptyGeneralCatchClause
            catch (Exception)
            {
            }
          }
        }
      }
      return result;
    }
  }
}