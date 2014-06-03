using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace CQS.Genome.SomaticMutation
{
  public abstract class AbstractPileupProcessor
  {
    protected readonly PileupOptions _options;
    
    protected bool? _samtoolsOk;

    public AbstractPileupProcessor(PileupOptions options)
    {
      _options = options;
    }

    public bool Process()
    {
      Console.Out.WriteLine("initialize process started at {0}", DateTime.Now);

      _options.PrintParameter();

      var watch = new Stopwatch();
      watch.Start();

      //remove all candidate files
      Directory.GetFiles(_options.CandidatesDirectory, "*.wsm").ForEach(m => File.Delete(m));

      var summary = GetMpileupResult();
      if (summary == null)
      {
        return false;
      }

      watch.Stop();
      Console.Out.WriteLine("initialize process ended at {0}, cost {1}", DateTime.Now, watch.Elapsed);

      new MpileupResultCountFormat().WriteToFile(_options.SummaryFilename, summary);

      GenomeUtils.SortChromosome(summary.Results, m => m.Item.SequenceIdentifier, m => (int)m.Item.Position);
      new MpileupFisherResultFileFormat().WriteToFile(_options.CandidatesFilename, summary.Results);

      using (var sw = new StreamWriter(_options.BaseFilename))
      {
        if (summary.Results.Count > 0)
        {
          string line;
          if (string.IsNullOrWhiteSpace(summary.Results[0].CandidateFile))
          {
            throw new Exception("Miss candidate file!");
          }

          using (var sr = new StreamReader(summary.Results[0].CandidateFile))
          {
            line = sr.ReadLine();
            sw.WriteLine("Identity\t{0}", line);
          }

          foreach (var res in summary.Results)
          {
            if (string.IsNullOrWhiteSpace(res.CandidateFile))
            {
              throw new Exception("Miss candidate file!");
            }

            using (var sr = new StreamReader(res.CandidateFile))
            {
              //pass the header line
              sr.ReadLine();
              while ((line = sr.ReadLine()) != null)
              {
                if (string.IsNullOrWhiteSpace(line))
                {
                  continue;
                }

                sw.WriteLine("{0}\t{1}",
                  Path.GetFileNameWithoutExtension(res.CandidateFile),
                  line);
              }
            }
          }
        }
      }

      Thread.Sleep(2000);
      foreach (var file in summary.Results)
      {
        File.Delete(file.CandidateFile);
      }

      return true;
    }

    protected abstract MpileupResult GetMpileupResult();
  }
}