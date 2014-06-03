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
using System.Threading.Tasks;
using RCPA;

namespace CQS.Genome.SomaticMutation
{
  /// <summary>
  /// Try to read in one thread and processing in multiple thread. But the bottleneck of analysis is the IO.
  /// This processor is not used.
  /// </summary>
  public class PileupParallelProcessor : AbstractPileupProcessor
  {
    public PileupParallelProcessor(PileupOptions options)
      : base(options)
    { }

    private int _threadCount;

    private void ParallelMpileupItem(Object stateInfo)
    {
      var param = stateInfo as Tuple<string, BlockingCollection<string>>;
      var mpileuplines = param.Item2;

      var mythreadindex = Interlocked.Increment(ref _threadCount);
      Console.WriteLine("Start sub thread {0} at {1}", mythreadindex, DateTime.Now);
      var result = new MpileupResult(param.Item1, _options.CandidatesDirectory);
      try
      {
        var proc = new MpileupParser(_options, result);

        while (!mpileuplines.IsCompleted)
        {
          Console.WriteLine("Waiting for data {0} ...", mythreadindex);
          Thread.Sleep(100);
          string line;
          while (mpileuplines.TryTake(out line))
          {
            var item = proc.Parse(line);

            if (item == null)
            {
              continue;
            }
          }
        }

        new MpileupResultCountFormat().WriteToFile(result.CandidateSummary, result);
      }
      catch(Exception ex)
      {
        Console.WriteLine("Thread {0} failed : {1}", mythreadindex, ex.Message);
        throw ex;
      }
      finally
      {
        Interlocked.Decrement(ref _threadCount);
        Console.WriteLine("Finished sub thread {0} at {1}", mythreadindex, DateTime.Now);
      }
    }

    protected override MpileupResult GetMpileupResult()
    {
      Console.WriteLine("Multiple thread mode ...");
      var parser = _options.GetPileupItemParser();
      var pfile = new PileupFile(parser);
      switch (_options.From)
      {
        case PileupOptions.DataSourceType.Mpileup:
          pfile.Open(_options.MpileupFile);
          break;
        case PileupOptions.DataSourceType.BAM:
          var proc = new MpileupParseProcessor(_options).ExecuteSamtools(null);
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

      _threadCount = 0;

      var lines = new BlockingCollection<string>();

      Console.WriteLine("Multiple thread mode, parallel by mpileup item ...");
      var mpileuplines = new BlockingCollection<string>();
      for (var i = 0; i < _options.ThreadCount - 1; i++)
      {
        ThreadPool.QueueUserWorkItem(ParallelMpileupItem, new Tuple<string, BlockingCollection<string>>(i.ToString(), mpileuplines));
      }

      long totalCount = 0;
      using (pfile)
      {
        try
        {
          string line;
          while ((line = pfile.ReadLine()) != null)
          {
            totalCount++;
            lines.Add(line);
          }

          lines.CompleteAdding();
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

      while (_threadCount > 0)
      {
        Thread.Sleep(100);
      }

      Console.WriteLine("After thread finished ...");
      var result = new MpileupResult(string.Empty, _options.CandidatesDirectory);

      Console.WriteLine("Merging summary information ...");

      for (int i = 0; i < _options.ThreadCount - 1; i++)
      {
        var summaryFile = new MpileupResult(i.ToString(), _options.CandidatesDirectory).CandidateSummary;
        var summary = new MpileupResultCountFormat().ReadFromFile(summaryFile);
        result.MergeWith(summary);
      }

      Console.WriteLine("Loading candidates ...");
      foreach (var file in Directory.GetFiles(_options.CandidatesDirectory, "*.wsm"))
      {
        var res = new MpileupFisherResult();
        res.ParseString(Path.GetFileNameWithoutExtension(file));
        res.CandidateFile = file;
        result.Results.Add(res);
      }

      return result;
    }
  }
}