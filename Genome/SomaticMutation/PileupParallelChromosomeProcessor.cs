﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CQS.Genome.Pileup;
using CQS.Genome.Statistics;
using RCPA.Seq;

namespace CQS.Genome.SomaticMutation
{
  public class PileupParallelChromosomeProcessor : AbstractPileupProcessor
  {
    public PileupParallelChromosomeProcessor(PileupOptions options)
      : base(options)
    { }

    private int _threadCount;

    protected override MpileupResult GetMpileupResult()
    {
      Console.WriteLine("Multiple thread mode, parallel by chromosome ...");

      _threadCount = 0;

      var chromosomes = new ConcurrentQueue<string>();
      foreach (var chr in _options.ChromosomeNames)
      {
        chromosomes.Enqueue(chr);
      }

      var cts = new CancellationTokenSource();

      var maxThreadCount = Math.Min(_options.ThreadCount, _options.ChromosomeNames.Count);
      for (int i = 0; i < maxThreadCount; i++)
      {
        ThreadPool.QueueUserWorkItem(ParallelChromosome, new Tuple<CancellationTokenSource, ConcurrentQueue<string>>(cts, chromosomes));
      }

      Thread.Sleep(5000);

      while (_threadCount > 0)
      {
        Thread.Sleep(100);
      }

      Console.WriteLine("After thread finished ...");
      var result = new MpileupResult(string.Empty, _options.CandidatesDirectory);

      Console.WriteLine("Merging summary information ...");
      foreach (var chr in _options.ChromosomeNames)
      {
        var summaryFile = new MpileupResult(chr, _options.CandidatesDirectory).CandidateSummary;
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

    private void ParallelChromosome(Object stateInfo)
    {
      var param = stateInfo as Tuple<CancellationTokenSource, ConcurrentQueue<string>>;

      var cts = param.Item1;
      var chromosomes = param.Item2;

      Interlocked.Increment(ref _threadCount);
      Console.WriteLine("Sub thread {0} started.", Thread.CurrentThread.ManagedThreadId);
      try
      {
        while (!chromosomes.IsEmpty)
        {
          string chromosomeName;
          if (!chromosomes.TryDequeue(out chromosomeName))
          {
            Thread.Sleep(10);
            continue;
          }

          new MpileupParseProcessor(_options).RunTask(chromosomeName, cts);
        }
      }
      finally
      {
        Interlocked.Decrement(ref _threadCount);
        Console.WriteLine("Sub thread {0} finished.", Thread.CurrentThread.ManagedThreadId);
      }
    }
  }
}