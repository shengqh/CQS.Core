using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using CQS.Genome.Annotation;
using RCPA.Gui;
using RCPA;

namespace CQS.Genome.SomaticMutation
{
  public class AnnotationProcessor : AbstractThreadProcessor
  {
    private readonly AnnotationProcessorOptions _options;

    public AnnotationProcessor(AnnotationProcessorOptions options)
    {
      _options = options;
    }

    public override IEnumerable<string> Process()
    {
      Progress.SetMessage("annotation process started at {0}", DateTime.Now);
      var watch = new Stopwatch();
      watch.Start();

      var exporters = new List<IAnnotationTsvExporter>();

      if (_options.Annovar)
      {
        if (!Directory.Exists(_options.AnnotationDirectory))
        {
          try
          {
            Directory.CreateDirectory(_options.AnnotationDirectory);
          }
          catch (Exception ex)
          {
            throw new Exception(string.Format("ERROR: Cannot create directory {0} : {1}", _options.AnnotationDirectory, ex.Message));
          }
        }

        using (var sw = new StreamWriter(_options.AnnovarInputFile))
        {
          using (var sr = new StreamReader(_options.InputFile))
          {
            string line = sr.ReadLine();
            if (line.Contains("fileformat=VCF"))
            {
              while ((line.StartsWith("##")))
              {
                line = sr.ReadLine();
              }

              while ((line = sr.ReadLine()) != null)
              {
                var parts = line.Split('\t');
                if (parts.Length > 4)
                {
                  sw.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}", parts[0], parts[1], parts[1], parts[3], parts[4]);
                }
              }
            }
            else
            {
              while ((line = sr.ReadLine()) != null)
              {
                var parts = line.Split('\t');
                if (parts.Length > 4)
                {
                  sw.WriteLine(parts.Take(5).Merge('\t'));
                }
              }
            }
          }
        }

        var perlproc = new Process
        {
          StartInfo = new ProcessStartInfo
          {
            FileName = _options.AnnovarCommand,
            Arguments = _options.AnnovarParameter,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            CreateNoWindow = true
          }
        };

        Progress.SetMessage("running command : " + perlproc.StartInfo.FileName + " " + perlproc.StartInfo.Arguments);
        try
        {
          if (!perlproc.Start())
          {
            throw new Exception("ERROR: Annovar command cannot be started, check your parameters and ensure that annovar and perl are available.");
          }
        }
        catch (Exception ex)
        {
          throw new Exception(string.Format("ERROR: Annovar command cannot be started : {0}", ex.Message));
        }

        try
        {
          string line;
          while ((line = perlproc.StandardOutput.ReadLine()) != null)
          {
            Progress.SetMessage(line);
          }
        }
        catch (Exception ex)
        {
          throw new Exception(string.Format("ERROR: Annovar command error : {0}", ex.Message));
        }

        var annovarResultFile = _options.AnnovarOutputFile + "." + _options.AnnovarBuildVersion + "_multianno.txt";
        if (!File.Exists(annovarResultFile))
        {
          throw new Exception(string.Format("ERROR: Annovar might be failed: cannot find annovar result {0}", annovarResultFile));
        }

        exporters.Add(new AnnovarExporter(annovarResultFile, GetKey));
      }

      if (_options.Rnaediting)
      {
        exporters.Add(new RnaeditingExporter(_options.RnaeditingDatabase, GetKey));
      }

      if (_options.Distance)
      {
        if (!string.IsNullOrEmpty(_options.DistanceJunctionBed))
        {
          exporters.Add(new JunctionDistanceExporter(_options.DistanceJunctionBed));
        }

        if (!string.IsNullOrEmpty(_options.DistanceInsertionBed))
        {
          exporters.Add(new InsertionDistanceExporter(_options.DistanceInsertionBed));
        }

        if (!string.IsNullOrEmpty(_options.DistanceDeletionBed))
        {
          exporters.Add(new DeletionDistanceExporter(_options.DistanceDeletionBed));
        }

        if (!string.IsNullOrEmpty(_options.GtfFile))
        {
          exporters.Add(new GtfDistanceExporter(_options.GtfFile));
        }
      }

      exporters.Add(new SomaticMutationDistanceExporter(_options.InputFile));

      Progress.SetMessage("writing merged result " + _options.OutputFile + " ...");
      using (var sw = new StreamWriter(_options.OutputFile))
      {
        using (var sr = new StreamReader(_options.InputFile))
        {
          var line = sr.ReadLine();

          if (line.Contains("fileformat=VCF"))
          {
            while ((line.StartsWith("##")))
            {
              line = sr.ReadLine();
            }
            line = line.Substring(1);
          }

          sw.WriteLine("{0}\t{1}", line, (from exporter in exporters
                                          select exporter.GetHeader()).Merge('\t'));

          while ((line = sr.ReadLine()) != null)
          {
            var parts = line.Split('\t');
            var chr = parts[0].StringAfter("chr");
            var position = long.Parse(parts[1]);
            //Console.WriteLine("{0}\t{1}", chr, position);
            sw.WriteLine("{0}\t{1}", parts.Merge('\t'), (from exporter in exporters
                                                         select exporter.GetValue(chr, position, position)).Merge('\t'));
          }
        }
      }

      watch.Stop();
      Progress.SetMessage("annotation process ended at {0}, cost {1}", DateTime.Now, watch.Elapsed);

      return new[] { _options.OutputFile };
    }

    private static string GetKey(string chr, long position)
    {
      return string.Format("{0}_{1}", chr, position);
    }
  }
}