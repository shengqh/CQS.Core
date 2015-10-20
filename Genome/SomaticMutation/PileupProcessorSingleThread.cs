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
using CQS.Genome.Samtools;

namespace CQS.Genome.SomaticMutation
{
  public class PileupProcessorSingleThread : AbstractPileupProcessor
  {
    public PileupProcessorSingleThread(PileupProcessorOptions options)
      : base(options)
    { }

    protected override MpileupResult GetMpileupResult()
    {
      var result = new MpileupResult(string.Empty, _options.CandidatesDirectory);

      Progress.SetMessage("Single thread mode ...");
      var parser = _options.GetPileupItemParser();
      var pfile = new PileupFile(parser);
      switch (_options.From)
      {
        case DataSourceType.Mpileup:
          pfile.Open(_options.MpileupFile);
          break;
        case DataSourceType.BAM:
          var proc = new MpileupProcessor(_options).ExecuteSamtools(new[] { _options.NormalBam, _options.TumorBam }.ToList(), _options.GetMpileupChromosomes());
          if (proc == null)
          {
            return null;
          }

          pfile.Open(proc.StandardOutput);
          pfile.Samtools = proc;
          break;
        case DataSourceType.Console:
          pfile.Open(Console.In);
          break;
      }

      using (pfile)
      {
        try
        {
          IMpileupParser proc = new MpileupParser(_options, result);

          string line;
          while ((line = pfile.ReadLine()) != null)
          //while ((item = pfile.Next("1", 48901870)) != null)
          {
            result.TotalCount++;

            try
            {
              var item = proc.Parse(line, true);
              if (item == null)
              {
                continue;
              }

              result.Results.Add(item);
            }
            catch (Exception ex)
            {
              throw new Exception(string.Format("parsing error {0}\n{1}", ex.Message, line));
            }
          }
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