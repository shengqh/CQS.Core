using CQS.Genome.Pileup;
using RCPA;
using RCPA.Gui;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;

namespace CQS.Genome.Samtools
{
  public class MpileupProcessor : ProgressClass
  {
    private MpileupOptions _options;

    public MpileupProcessor(MpileupOptions options)
    {
      _options = options;
    }

    public Process ExecuteSamtools(IEnumerable<string> bamFiles, string chromosome, string positionFile = null)
    {
      var chr = string.IsNullOrEmpty(chromosome) ? "" : "-r " + chromosome;
      var pos = string.IsNullOrEmpty(positionFile) ? "" : "-l " + positionFile;
      var mapq = _options.MinimumReadQuality == 0 ? "" : "-q " + _options.MinimumReadQuality.ToString();
      var baseq = _options.MinimumBaseQuality == 0 ? "" : "-Q " + _options.MinimumBaseQuality.ToString();
      var disableBAQ = _options.DisableBAQ ? "-B" : string.Empty;
      var result = new Process
      {
        StartInfo = new ProcessStartInfo
        {
          //FileName = _options.GetSamtoolsCommand(),
          FileName = "samtools",
          Arguments = string.Format(" mpileup -A -O {0} {1} {2} {3} {4} -f {5} {6}", chr, mapq, baseq, disableBAQ, pos, _options.GenomeFastaFile, bamFiles.Merge(" ")),
          UseShellExecute = false,
          RedirectStandardOutput = true,
          CreateNoWindow = true
        }
      };

      Progress.SetMessage("running command : " + result.StartInfo.FileName + " " + result.StartInfo.Arguments);
      try
      {
        if (!result.Start())
        {
          throw new Exception(string.Format("ERROR: samtools mpileup cannot be started, check your parameters and ensure that samtools are available."));
        }
      }
      catch (Exception ex)
      {
        throw new Exception(string.Format("ERROR: samtools mpileup cannot be started, check your parameters and ensure that samtools are available : {0}", ex.Message));
      }

      return result;
    }
  }
}
