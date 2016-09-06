using RCPA;
using RCPA.Gui;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Linq;

namespace CQS.Genome.SomaticMutation
{
  public abstract class AbstractPileupProcessor : ProgressClass, IProcessor
  {
    protected readonly PileupProcessorOptions _options;

    protected bool? _samtoolsOk;

    protected virtual bool outputNotCoveredSite { get { return false; } }

    public string SoftwareName { get; set; }

    public string SoftwareVersion { get; set; }

    public AbstractPileupProcessor(PileupProcessorOptions options)
    {
      _options = options;
    }

    protected abstract MpileupResult GetMpileupResult();

    public virtual IEnumerable<string> Process()
    {
      Progress.SetMessage("initialize process started at {0}", DateTime.Now);

      _options.PrintParameter(Console.Out);

      var watch = new Stopwatch();
      watch.Start();

      //remove all candidate files
      Directory.GetFiles(_options.CandidatesDirectory, "*.wsm").ForEach(m => File.Delete(m));

      var summary = GetMpileupResult();
      watch.Stop();
      Progress.SetMessage("initialize process ended at {0}, cost {1}", DateTime.Now, watch.Elapsed);

      WriteSummaryFile(summary);

      //GenomeUtils.SortChromosome(summary.Results, m => m.Item.SequenceIdentifier, m => (int)m.Item.Position);
      new MpileupFisherResultFileFormat().WriteToFile(_options.CandidatesFilename, summary.Results);

      using (var sw = new StreamWriter(_options.BaseFilename))
      {
        if (summary.Results.Count > 0)
        {
          var candFiles = summary.Results.Where(m => !string.IsNullOrEmpty(m.CandidateFile)).ToArray();
          if (candFiles.Length == 0)
          {
            Progress.SetMessage("No candidate file found!");
          }
          else
          {
            string line;
            using (var sr = new StreamReader(candFiles[0].CandidateFile))
            {
              line = sr.ReadLine();
              sw.WriteLine("Identity\t{0}", line);
            }

            foreach (var res in candFiles)
            {
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

                  sw.WriteLine("{0}\t{1}", Path.GetFileNameWithoutExtension(res.CandidateFile), line);
                }
              }
            }
          }
        }
      }

      Thread.Sleep(2000);
      foreach (var file in summary.Results)
      {
        if (File.Exists(file.CandidateFile))
        {
          File.Delete(file.CandidateFile);
        }
      }

      return new string[] { _options.BaseFilename };
    }

    protected virtual void WriteSummaryFile(MpileupResult summary)
    {
      new MpileupResultCountFormat(_options, outputNotCoveredSite).WriteToFile(_options.SummaryFilename, summary);
    }
  }
}