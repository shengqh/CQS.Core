using System;
using System.IO;
using CommandLine;
using RCPA;

namespace CQS.Genome.SomaticMutation
{
  public class FilterOptions : AbstractProgramOptions
  {
    public const double DEFAULT_ErrorRate = 0.01;

    public FilterOptions()
    {
      SourceRFile = FileUtils.GetTemplateDir() + "/rsmc.r";
      ErrorRate = DEFAULT_ErrorRate;
      PValue = PileupOptions.DefaultPValue;
      MinimumBaseQuality = PileupOptions.DefaultMinimumBaseQuality;
    }

    public string SourceRFile { get; set; }

    [Option("error_rate", MetaValue = "DOUBLE", DefaultValue = DEFAULT_ErrorRate, HelpText = "Sequencing error rate for normal sample test")]
    public double ErrorRate { get; set; }

    [Option('e', "pvalue", MetaValue = "DOUBLE", DefaultValue = PileupOptions.DefaultPValue, HelpText = "pvalue used for significance test")]
    public double PValue { get; set; }

    [Option('q', "base_quality", MetaValue = "INT", DefaultValue = PileupOptions.DefaultMinimumBaseQuality,
      HelpText = "Minimum base quality for mpileup result filter")]
    public int MinimumBaseQuality { get; set; }

    [Option('o', "output", MetaValue = "FILE", Required = true, HelpText = "Output file")]
    public string OutputFile { get; set; }

    [Option('i', "input", MetaValue = "FILE", Required = true,
      HelpText = "Input file")]
    public string InputFile { get; set; }

    public string TargetRFile
    {
      get { return new FileInfo(OutputFile + ".r").FullName.Replace("\\", "/"); }
    }

    public string GetRCommand()
    {
      return Config.FindOrCreate("R", "R").Command;
    }

    public override bool PrepareOptions()
    {
      if (!File.Exists(SourceRFile))
      {
        ParsingErrors.Add(string.Format("file not exists : {0}", SourceRFile));
        return false;
      }

      if (!IsPileup)
      {
        if (!File.Exists(InputFile))
        {
          ParsingErrors.Add(string.Format("File not exists : {0}", InputFile));
          return false;
        }
      }

      try
      {
        var lines = File.ReadAllLines(SourceRFile);
        using (var sw = new StreamWriter(TargetRFile))
        {
          sw.WriteLine("setwd(\"{0}\")", Path.GetDirectoryName(Path.GetFullPath(InputFile)).Replace("\\", "/"));
          sw.WriteLine("inputfile<-\"{0}\"", Path.GetFileName(InputFile));
          if (Path.GetDirectoryName(Path.GetFullPath(InputFile)).Equals(Path.GetDirectoryName(Path.GetFullPath(OutputFile))))
          {
            sw.WriteLine("outputfile<-\"{0}\"", Path.GetFileName(OutputFile));
          }
          else
          {
            sw.WriteLine("outputfile<-\"{0}\"", Path.GetFullPath(OutputFile).Replace("\\", "/"));
          }
          sw.WriteLine("pvalue<-{0}", PValue);
          sw.WriteLine("errorrate<-{0}", ErrorRate);

          var inpredefined = true;
          foreach (var line in lines)
          {
            if (line.StartsWith("##predefine_end"))
            {
              inpredefined = false;
              continue;
            }

            if (!inpredefined && !line.Trim().StartsWith("#"))
            {
              sw.WriteLine(line);
            }
          }
        }

        return true;
      }
      catch (Exception ex)
      {
        ParsingErrors.Add(string.Format("create R file {0} failed: {1}", TargetRFile, ex.Message));
        return false;
      }
    }
  }
}