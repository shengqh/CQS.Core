using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using CQS.Genome.Annotation;

namespace CQS.Genome.SomaticMutation
{
  public class AnnotationProcessor
  {
    private readonly AnnotationOptions _options;

    public AnnotationProcessor(AnnotationOptions options)
    {
      _options = options;
    }

    public bool Process()
    {
      Console.Out.WriteLine("annotation process started at {0}", DateTime.Now);
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
            Console.Out.WriteLine("Cannot create directory {0} : {1}", _options.AnnotationDirectory, ex.Message);
            return false;
          }
        }

        using (var sw = new StreamWriter(_options.AnnovarInputFile))
        {
          using (var sr = new StreamReader(_options.InputFile))
          {
            sr.ReadLine();

            string line;
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

        Console.Out.WriteLine("running command : " + perlproc.StartInfo.FileName + " " + perlproc.StartInfo.Arguments);
        try
        {
          if (!perlproc.Start())
          {
            Console.Out.WriteLine(
              "Annovar command cannot be started, check your parameters and ensure that annovar and perl are available.");
          }
        }
        catch (Exception ex)
        {
          Console.Out.WriteLine("Annovar command cannot be started : {0}", ex.Message);
          return false;
        }

        try
        {
          string line;
          while ((line = perlproc.StandardOutput.ReadLine()) != null)
          {
            Console.Out.WriteLine(line);
          }
        }
        catch (Exception ex)
        {
          Console.Out.WriteLine("Annovar command error : {0}", ex.Message);
          return false;
        }

        var annovarResultFile = _options.AnnovarOutputFile + "." + _options.AnnovarBuildVersion + "_multianno.txt";
        if (!File.Exists(annovarResultFile))
        {
          Console.Out.WriteLine("Annovar might be failed: cannot find annovar result {0}", annovarResultFile);
          return false;
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

      var resultfile = Path.ChangeExtension(_options.InputFile, ".annotation.tsv");
      Console.WriteLine("writing merged result " + resultfile + " ...");
      using (var sw = new StreamWriter(resultfile))
      {
        using (var sr = new StreamReader(_options.InputFile))
        {
          var line = sr.ReadLine();
          var delimiter = _options.InputFile.EndsWith(".tsv") ? '\t' : ',';

          var parts = line.Split(delimiter);

          sw.WriteLine("{0}\t{1}", parts.Merge('\t'), (from exporter in exporters
            select exporter.GetHeader()).Merge('\t'));

          while ((line = sr.ReadLine()) != null)
          {
            parts = line.Split(delimiter);
            var chr = parts[0];
            var position = long.Parse(parts[1]);
            //Console.WriteLine("{0}\t{1}", chr, position);
            sw.WriteLine("{0}\t{1}", parts.Merge('\t'), (from exporter in exporters
              select exporter.GetValue(chr, position, position)).Merge('\t'));
          }
        }
      }

      watch.Stop();
      Console.Out.WriteLine("annotation process ended at {0}, cost {1}", DateTime.Now, watch.Elapsed);

      return true;
    }

    private static string GetKey(string chr, long position)
    {
      return string.Format("{0}_{1}", chr, position);
    }
  }
}