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
  public abstract class AbstractPileupProcessor
  {
    protected readonly PileupOptions _options;

    protected int _minReadDepthFailed = 0;
    protected int _oneEventFailed = 0;
    protected int _minorAlleleDecreasedFailed = 0;
    protected int _minimumPercentageFailed = 0;
    protected int _groupFisherFailed = 0;
    protected int _candidateCount = 0;

    protected int _totalCount = 0;

    protected bool? _samtoolsOk;

    public AbstractPileupProcessor(PileupOptions options)
    {
      _options = options;
    }

    protected Process ExecuteSamtools(string samtoolsCommand, string chromosome)
    {
      var chr = string.IsNullOrEmpty(chromosome) ? "" : " -r " + chromosome;
      var mapq = _options.MpileupMinimumReadQuality == 0 ? "" : " -q " + _options.MpileupMinimumReadQuality;
      var result = new Process
      {
        StartInfo = new ProcessStartInfo
        {
          FileName = samtoolsCommand,
          Arguments =
            string.Format(" mpileup{0}{1} -f {2} {3} {4} ", mapq, chr, _options.GenomeFastaFile, _options.NormalBam,
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
          Console.Out.WriteLine(
            "samtools mpileup cannot be started, check your parameters and ensure that samtools are available.");
          _samtoolsOk = false;
          return null;
        }
      }
      catch (Exception ex)
      {
        Console.Out.WriteLine(
          "samtools mpileup cannot be started, check your parameters and ensure that samtools are available : {0}",
          ex.Message);
        _samtoolsOk = false;
        return null;
      }

      _samtoolsOk = true;
      return result;
    }

    protected void CopyCountInfo(MpileupParseProcessor proc)
    {
      Interlocked.Add(ref _minReadDepthFailed, proc.MinReadDepthFailed);
      Interlocked.Add(ref _oneEventFailed, proc.OneEventFailed);
      Interlocked.Add(ref _minorAlleleDecreasedFailed, proc.MinorAlleleDecreasedFailed);
      Interlocked.Add(ref _minimumPercentageFailed, proc.PercentageFailed);
      Interlocked.Add(ref _groupFisherFailed, proc.GroupFisherFailed);
      Interlocked.Add(ref _candidateCount, proc.CandidateCount);
    }

    public bool Process()
    {
      Console.Out.WriteLine("initialize process started at {0}", DateTime.Now);

      Console.Out.WriteLine("#output directory: " + _options.OutputSuffix);
      Console.Out.WriteLine("#minimum count: " + _options.MinimumReadDepth);
      Console.Out.WriteLine("#minimum read quality: " + _options.MpileupMinimumReadQuality);
      Console.Out.WriteLine("#minimum base quality: " + _options.MinimumBaseQuality);
      Console.Out.WriteLine("#maximum percentage of minor allele in normal: " + _options.MaximumPercentageOfMinorAllele);
      Console.Out.WriteLine("#minimum percentage of minor allele in tumor: " + _options.MinimumPercentageOfMinorAllele);
      Console.Out.WriteLine("#pvalue: " + _options.PValue);
      //Console.Out.WriteLine("#filter by position bias: " + (!_options.NotFilterPosition));
      //Console.Out.WriteLine("#filter by strand bias: " + (!_options.NotFilterStrand));
      Console.Out.WriteLine("#thread count: " + _options.ThreadCount);

      var watch = new Stopwatch();
      watch.Start();

      var saved = GetFisherFilterResults();
      if (saved == null)
      {
        return false;
      }

      watch.Stop();
      Console.Out.WriteLine("initialize process ended at {0}, cost {1}", DateTime.Now, watch.Elapsed);

      GenomeUtils.SortChromosome(saved, m => m.Item.SequenceIdentifier, m => (int)m.Item.Position);

      using (var sw = new StreamWriter(_options.CandidatesFilename))
      {
        sw.WriteLine(
          "chr\tloc\tref\tmajor_allele\tminor_allele\tnormal_major_count\tnormal_minor_count\ttumor_major_count\ttumor_minor_count\tfisher_group"
          //+ "\tfisher_position\tfisher_strand"
          );

        foreach (var res in saved)
        {
          sw.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}\t{7}\t{8}\t{9:0.0E00}"
            //+"\t{10:0.0E00}\t{11:0.0E00}"
          ,
            res.Item.SequenceIdentifier, res.Item.Position, res.Item.Nucleotide, res.Group.SucceedName,
            res.Group.FailedName,
            res.Group.Sample1.Succeed, res.Group.Sample1.Failed, res.Group.Sample2.Succeed, res.Group.Sample2.Failed,
            res.Group.PValue
            //, res.Position.PValue, res.Strand.PValue
            );
        }
      }

      using (var sw = new StreamWriter(_options.BaseFilename))
      {
        if (saved.Count > 0)
        {
          string line;
          using (var sr = new StreamReader(saved[0].CandidateFile))
          {
            line = sr.ReadLine();
            sw.WriteLine("Identity\t{0}", line);
          }

          foreach (var res in saved)
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

                sw.WriteLine("{0}\t{1}",
                  Path.GetFileNameWithoutExtension(res.CandidateFile),
                  line);
              }
            }
          }
        }
      }

      Thread.Sleep(2000);
      foreach (var file in saved)
      {
        File.Delete(file.CandidateFile);
      }

      using (var sw = new StreamWriter(_options.SummaryFilename))
      {
        sw.WriteLine("Reason\tRemoved candidate\tRetained candidate");
        sw.WriteLine("total site\t\t{0}", _totalCount);
        _totalCount -= _minReadDepthFailed;
        sw.WriteLine("minimum read depth failed\t{0}\t{1}", _minReadDepthFailed, _totalCount);
        _totalCount -= _oneEventFailed;
        sw.WriteLine("all same allele\t{0}\t{1}", _oneEventFailed, _totalCount);
        _totalCount -= _minorAlleleDecreasedFailed;
        sw.WriteLine("minor allele decreased\t{0}\t{1}", _minorAlleleDecreasedFailed, _totalCount);
        _totalCount -= _minimumPercentageFailed;
        sw.WriteLine("minimum percentage of minor allele failed\t{0}\t{1}", _minimumPercentageFailed, _totalCount);
        _totalCount -= _groupFisherFailed;
        sw.WriteLine("fisher exact test is not significant\t{0}\t{1}", _groupFisherFailed, _totalCount);
        //if (_options.NotFilterPosition)
        //{
        //  _totalCount -= _positionFisherFailed;
        //  sw.WriteLine("fisher exact test of position is significant\t{0}\t{1}", _positionFisherFailed, _totalCount);
        //}
        //if (_options.NotFilterStrand)
        //{
        //  _totalCount -= _strandFisherFailed;
        //  sw.WriteLine("fisher exact test of strand is significant\t{0}\t{1}", _strandFisherFailed, _totalCount);
        //}
      }

      return true;
    }

    protected abstract List<MpileupFisherResult> GetFisherFilterResults();
  }
}