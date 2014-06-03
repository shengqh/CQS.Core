using CQS.Genome.Pileup;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;

namespace CQS.Genome.SomaticMutation
{
  public class MpileupParseProcessor
  {
    private PileupOptions _options;

    public MpileupParseProcessor(PileupOptions options)
    {
      _options = options;
    }

    public Process ExecuteSamtools(string chromosome)
    {
      var chr = string.IsNullOrEmpty(chromosome) ? "" : "-r " + chromosome;
      var mapq = _options.MpileupMinimumReadQuality == 0 ? "" : "-q " + _options.MpileupMinimumReadQuality.ToString();
      var baseq = _options.MinimumBaseQuality == 0 ? "" : "-Q " + _options.MinimumBaseQuality.ToString();
      var result = new Process
      {
        StartInfo = new ProcessStartInfo
        {
          FileName = _options.GetSamtoolsCommand(),
          Arguments =
            string.Format(" mpileup -A -O {0} {1} {2} -f {3} {4} {5} ", chr, mapq, baseq, _options.GenomeFastaFile, _options.NormalBam,
              _options.TumorBam),
          UseShellExecute = false,
          RedirectStandardOutput = true,
          CreateNoWindow = true
        }
      };

      Console.Out.WriteLine("running command : " + result.StartInfo.FileName + " " + result.StartInfo.Arguments);
      try
      {
        if (!result.Start())
        {
          Console.Error.WriteLine(
            "samtools mpileup cannot be started, check your parameters and ensure that samtools are available.");
          return null;
        }
      }
      catch (Exception ex)
      {
        Console.Error.WriteLine(
          "samtools mpileup cannot be started, check your parameters and ensure that samtools are available : {0}",
          ex.Message);
        return null;
      }

      return result;
    }

    public void RunTask(string chr, CancellationTokenSource cts)
    {
      if (cts.IsCancellationRequested)
      {
        return;
      }

      var result = new MpileupResult(chr, _options.CandidatesDirectory);

      Console.WriteLine("Processing chromosome {0} in thread {1}", chr, Thread.CurrentThread.ManagedThreadId);
      var process = ExecuteSamtools(chr);
      if (process == null)
      {
        throw new Exception(string.Format("Fail to execute samtools for chromosome {0}.", chr));
      }

      var parser = _options.GetPileupItemParser();
      var pfile = new PileupFile(parser);
      pfile.Open(process.StandardOutput);

      var proc = new MpileupParser(_options, result);
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

          new MpileupResultCountFormat().WriteToFile(result.CandidateSummary, result);
          Console.WriteLine("Processing chromosome {0} in thread {1} finished.", chr, Thread.CurrentThread.ManagedThreadId);
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
