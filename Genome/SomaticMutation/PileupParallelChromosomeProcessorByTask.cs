using System;
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
  // Provides a task scheduler that ensures a maximum concurrency level while  
  // running on top of the thread pool. 
  public class LimitedConcurrencyLevelTaskScheduler : TaskScheduler
  {
    // Indicates whether the current thread is processing work items.
    [ThreadStatic]
    private static bool _currentThreadIsProcessingItems;

    // The list of tasks to be executed  
    private readonly LinkedList<Task> _tasks = new LinkedList<Task>(); // protected by lock(_tasks) 

    // The maximum concurrency level allowed by this scheduler.  
    private readonly int _maxDegreeOfParallelism;

    // Indicates whether the scheduler is currently processing work items.  
    private int _delegatesQueuedOrRunning = 0;

    // Creates a new instance with the specified degree of parallelism.  
    public LimitedConcurrencyLevelTaskScheduler(int maxDegreeOfParallelism)
    {
      if (maxDegreeOfParallelism < 1) throw new ArgumentOutOfRangeException("maxDegreeOfParallelism");
      _maxDegreeOfParallelism = maxDegreeOfParallelism;
    }

    // Queues a task to the scheduler.  
    protected sealed override void QueueTask(Task task)
    {
      // Add the task to the list of tasks to be processed.  If there aren't enough  
      // delegates currently queued or running to process tasks, schedule another.  
      lock (_tasks)
      {
        _tasks.AddLast(task);
        if (_delegatesQueuedOrRunning < _maxDegreeOfParallelism)
        {
          ++_delegatesQueuedOrRunning;
          NotifyThreadPoolOfPendingWork();
        }
      }
    }

    // Inform the ThreadPool that there's work to be executed for this scheduler.  
    private void NotifyThreadPoolOfPendingWork()
    {
      ThreadPool.UnsafeQueueUserWorkItem(_ =>
      {
        // Note that the current thread is now processing work items. 
        // This is necessary to enable inlining of tasks into this thread.
        _currentThreadIsProcessingItems = true;
        try
        {
          // Process all available items in the queue. 
          while (true)
          {
            Task item;
            lock (_tasks)
            {
              // When there are no more items to be processed, 
              // note that we're done processing, and get out. 
              if (_tasks.Count == 0)
              {
                --_delegatesQueuedOrRunning;
                break;
              }

              // Get the next item from the queue
              item = _tasks.First.Value;
              _tasks.RemoveFirst();
            }

            // Execute the task we pulled out of the queue 
            base.TryExecuteTask(item);
          }
        }
        // We're done processing items on the current thread 
        finally { _currentThreadIsProcessingItems = false; }
      }, null);
    }

    // Attempts to execute the specified task on the current thread.  
    protected sealed override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
    {
      // If this thread isn't already processing a task, we don't support inlining 
      if (!_currentThreadIsProcessingItems) return false;

      // If the task was previously queued, remove it from the queue 
      if (taskWasPreviouslyQueued)
        // Try to run the task.  
        if (TryDequeue(task))
          return base.TryExecuteTask(task);
        else
          return false;
      else
        return base.TryExecuteTask(task);
    }

    // Attempt to remove a previously scheduled task from the scheduler.  
    protected sealed override bool TryDequeue(Task task)
    {
      lock (_tasks) return _tasks.Remove(task);
    }

    // Gets the maximum concurrency level supported by this scheduler.  
    public sealed override int MaximumConcurrencyLevel { get { return _maxDegreeOfParallelism; } }

    // Gets an enumerable of the tasks currently scheduled on this scheduler.  
    protected sealed override IEnumerable<Task> GetScheduledTasks()
    {
      bool lockTaken = false;
      try
      {
        Monitor.TryEnter(_tasks, ref lockTaken);
        if (lockTaken) return _tasks;
        else throw new NotSupportedException();
      }
      finally
      {
        if (lockTaken) Monitor.Exit(_tasks);
      }
    }
  }

  public class PileupParallelChromosomeProcessorByTask : AbstractPileupProcessor
  {
    public PileupParallelChromosomeProcessorByTask(PileupOptions options)
      : base(options)
    { }

    protected override MpileupResult GetMpileupResult()
    {
      Console.WriteLine("Multiple thread mode, parallel by chromosome ...");

      LimitedConcurrencyLevelTaskScheduler lcts = new LimitedConcurrencyLevelTaskScheduler(_options.ThreadCount - 1);
      List<Task> tasks = new List<Task>();

      // Create a TaskFactory and pass it our custom scheduler. 
      TaskFactory factory = new TaskFactory(lcts);
      CancellationTokenSource cts = new CancellationTokenSource();

      foreach (var chr in _options.ChromosomeNames)
      {
        Task t = factory.StartNew(() =>
        {
          new MpileupParseProcessor(_options).RunTask(chr, cts);
        }, cts.Token);
        tasks.Add(t);
      }

      // Wait for the tasks to complete before displaying a completion message.
      try
      {
        Task.WaitAll(tasks.ToArray());
      }
      catch (Exception ex)
      {
        cts.Cancel();
        throw ex;
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
  }
}