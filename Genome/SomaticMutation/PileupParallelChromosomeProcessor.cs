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
  public class PileupParallelChromosomeProcessor : AbstractPileupProcessor
  {
    private int _startedThreadCount;
    private int _threadFailed;
    private int _threadcount;

    public PileupParallelChromosomeProcessor(PileupOptions options)
      : base(options)
    { }

    private class ChromosomeResult
    {
      public ChromosomeResult(string chromosome)
      {
        this.Name = chromosome;
        this.Result = new List<MpileupFisherResult>();
      }

      public string Name { get; private set; }
      public List<MpileupFisherResult> Result { get; private set; }
    }

    private void ParallelChromosome(Object stateInfo)
    {
      var chromosomes = stateInfo as ConcurrentQueue<ChromosomeResult>;


      Interlocked.Increment(ref _startedThreadCount);

      int mythreadindex = Interlocked.Increment(ref _threadcount);
      Console.WriteLine("Start sub thread {0}", mythreadindex);
      try
      {
        var proc = new MpileupParseProcessor(_options);
        var parser = _options.GetPileupItemParser();

        var localTotalCount = 0;

        while (!chromosomes.IsEmpty)
        {
          ChromosomeResult chromosome;
          if (!chromosomes.TryDequeue(out chromosome))
          {
            Thread.Sleep(10);
            continue;
          }

          Console.WriteLine("Processing chromosome {0} in thread {1}", chromosome.Name, mythreadindex);
          var process = ExecuteSamtools(_options.GetSamtoolsCommand(), chromosome.Name);
          if (process == null)
          {
            return;
          }

          try
          {
            using (var pfile = new PileupFile(parser))
            {
              pfile.Open(process.StandardOutput);
              string line;
              while ((line = pfile.ReadLine()) != null)
              {
                localTotalCount++;

                if (_threadFailed > 0)
                {
                  process.Kill();
                  return;
                }

                try
                {
                  var item = proc.Parse(line);
                  if (item == null)
                  {
                    continue;
                  }

                  chromosome.Result.Add(item);
                }
                catch (Exception ex)
                {
                  Console.WriteLine("Parsing mpileup result error : {0}\n{1}", ex.Message, line);
                  Interlocked.Increment(ref _threadFailed);
                  return;
                }
              }
            }
          }
          finally
          {
            try
            {
              process.Kill();
            }
            // ReSharper disable once EmptyGeneralCatchClause
            catch
            {
            }
          }
        }

        Interlocked.Add(ref _totalCount, localTotalCount);
        CopyCountInfo(proc);
      }
      finally
      {
        Interlocked.Decrement(ref _threadcount);
        Console.WriteLine("Finished sub thread {0}", mythreadindex);
      }
    }

    protected override List<MpileupFisherResult> GetFisherFilterResults()
    {
      Console.WriteLine("Multiple thread mode, parallel by chromosome ...");
      _startedThreadCount = 0;
      _threadFailed = 0;
      _threadcount = 0;

      var totalThread = _options.ThreadCount - 1;
      var chromosomes = new ConcurrentQueue<ChromosomeResult>();
      List<ChromosomeResult> results = new List<ChromosomeResult>();
      foreach (var chr in _options.ChromosomeNames)
      {
        var res = new ChromosomeResult(chr);
        results.Add(res);
        chromosomes.Enqueue(res);
      }

      _samtoolsOk = null;
      ThreadPool.QueueUserWorkItem(ParallelChromosome, chromosomes);
      while (_samtoolsOk == null)
      {
        Thread.Sleep(100);
      }

      if (_samtoolsOk == false)
      {
        return null;
      }

      for (var i = 0; i < totalThread - 1; i++)
      {
        ThreadPool.QueueUserWorkItem(ParallelChromosome, chromosomes);
      }

      while (_startedThreadCount == 0)
      {
        Thread.Sleep(100);
      }

      while (_threadcount > 0)
      {
        Thread.Sleep(100);
      }

      if (_threadFailed > 0)
      {
        return null;
      }

      var result = new List<MpileupFisherResult>();
      foreach (var res in results)
      {
        result.AddRange(res.Result);
      }

      return result;
    }
  }
}