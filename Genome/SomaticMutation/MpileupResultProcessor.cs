using CQS.Genome.Pileup;
using CQS.Genome.Samtools;
using RCPA;
using System;
using System.Threading;

namespace CQS.Genome.SomaticMutation
{
  public class MpileupResultProcessor : MpileupProcessor
  {
    private PileupProcessorOptions _options;

    public MpileupResultProcessor(PileupProcessorOptions options)
      : base(options)
    {
      _options = options;
    }

    public void RunTask(string chr, CancellationTokenSource cts)
    {
      if (cts.IsCancellationRequested)
      {
        throw new UserTerminatedException();
      }

      var result = new MpileupResult(chr, _options.CandidatesDirectory);

      Progress.SetMessage("Processing chromosome {0} in thread {1}", chr, Thread.CurrentThread.ManagedThreadId);
      var process = ExecuteSamtools(new[] { _options.NormalBam, _options.TumorBam }, chr);
      if (process == null)
      {
        throw new Exception(string.Format("Fail to execute samtools for chromosome {0}.", chr));
      }

      var parser = _options.GetPileupItemParser();
      var pfile = new PileupFile(parser);
      pfile.Open(process.StandardOutput);

      var proc = new MpileupParser(_options, result)
      {
        Progress = this.Progress
      };
      try
      {
        using (pfile)
        {
          string line;
          while ((line = pfile.ReadLine()) != null)
          {
            result.TotalCount++;

            if (cts.IsCancellationRequested)
            {
              return;
            }

            try
            {
              proc.Parse(line);
            }
            catch (Exception ex)
            {
              throw new Exception(string.Format("Parsing mpileup entry failed : {0}\n{1}", ex.Message, line), ex);
            }
          }

          new MpileupResultCountFormat(_options, true).WriteToFile(result.CandidateSummary, result);
          Progress.SetMessage("Processing chromosome {0} in thread {1} finished.", chr, Thread.CurrentThread.ManagedThreadId);
        }
      }
      catch (Exception ex)
      {
        cts.Cancel();
        throw new Exception(string.Format("Processing chromosome {0} failed : {1}", result.Name, ex.Message), ex);
      }
      finally
      {
        try
        {
          if (process != null) process.Kill();
        }
        catch
        {
        }
      }
    }
  }
}
