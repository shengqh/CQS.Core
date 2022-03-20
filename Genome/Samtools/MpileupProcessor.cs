﻿using RCPA.Gui;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace CQS.Genome.Samtools
{
  public class MpileupProcessor : ProgressClass
  {
    private MpileupOptions _options;

    public MpileupProcessor(MpileupOptions options)
    {
      _options = options;
    }

    private void CheckMpileup(string samtools, out bool runable, out bool supportMinDepth)
    {
      runable = false;
      supportMinDepth = false;

      //Progress.SetMessage("Checking {0} ...", samtools);
      var si = new ProcessStartInfo
      {
        FileName = samtools,
        Arguments = string.Format(" mpileup"),
        UseShellExecute = false,
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        CreateNoWindow = true
      };

      using (var result = new Process { StartInfo = si })
      {
        try
        {
          if (runable = result.Start())
          {
            string line;
            while ((line = result.StandardError.ReadLine()) != null)
            {
              if (line.Contains("--min-depth"))
              {
                supportMinDepth = true;
                //Progress.SetMessage("{0} mpileup supports min-depth.");
                break;
              }
            }
          }
        }
        catch (Exception)
        {
          runable = false;
        }
      }
    }

    public Process ExecuteSamtools(IEnumerable<string> bamFiles, string chromosome = null, string positionFile = null)
    {
      var chr = string.IsNullOrEmpty(chromosome) ? "" : "-r " + chromosome;
      var pos = string.IsNullOrEmpty(positionFile) ? "" : "-l " + positionFile;
      var mapq = _options.MinimumReadQuality == 0 ? "" : "-q " + _options.MinimumReadQuality.ToString();
      var baseq = _options.MinimumBaseQuality == 0 ? "" : "-Q " + _options.MinimumBaseQuality.ToString();
      var maxDepth = _options.MaximumReadDepth == 0 ? "" : "-d " + _options.MaximumReadDepth.ToString();
      var disableBAQ = _options.DisableBAQ ? "-B" : string.Empty;

      string minDepth = string.Empty;
      var samtools = _options.GetSamtoolsCommand();

      //Progress.SetMessage("MinimumReadDepth = {0}, IgnoreDepthLimitation = {1}", _options.MinimumReadDepth, _options.IgnoreDepthLimitation);
      if (_options.MinimumReadDepth > 0 && !_options.IgnoreDepthLimitation)
      {
        //Progress.SetMessage("Checking depth...");

        var fileRunable = false;
        var fileSupportMinDepth = false;
        if (File.Exists(samtools))
        {
          CheckMpileup(samtools, out fileRunable, out fileSupportMinDepth);
        }

        var nativeRunable = false;
        var nativeSupportMinDepth = false;
        CheckMpileup("samtools", out nativeRunable, out nativeSupportMinDepth);

        if (nativeSupportMinDepth)
        {
          samtools = "samtools";
          minDepth = "--md " + _options.MinimumReadDepth.ToString();
        }
        else if (fileSupportMinDepth)
        {
          minDepth = "--md " + _options.MinimumReadDepth.ToString();
        }
        else if (nativeRunable)
        {
          samtools = "samtools";
        }
      }

      var result = new Process
      {
        StartInfo = new ProcessStartInfo
        {
          FileName = samtools,
          Arguments = string.Format(" mpileup -A -O {0} {1} {2} {3} {4} {5} {6} -f {7} {8}", chr, mapq, baseq, disableBAQ, pos, minDepth, maxDepth, _options.GenomeFastaFile, bamFiles.Merge(" ")),
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
