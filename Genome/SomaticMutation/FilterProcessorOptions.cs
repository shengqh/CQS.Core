using System;
using System.IO;
using CommandLine;
using RCPA;

namespace CQS.Genome.SomaticMutation
{
  public class FilterProcessorOptions : AbstractProgramOptions
  {
    public const double DEFAULT_ErrorRate = 0.01;
    public const double DEFAULT_GlmPvalue = 0.05;

    public FilterProcessorOptions()
    {
      SourceRFile = FileUtils.GetTemplateDir() + "/glmvc.r";
      ErrorRate = DEFAULT_ErrorRate;
      GlmPvalue = DEFAULT_GlmPvalue;
      IsValidation = false;
    }

    public string SourceRFile { get; set; }

    [Option("error_rate", MetaValue = "DOUBLE", DefaultValue = DEFAULT_ErrorRate, HelpText = "Sequencing error rate for normal sample test")]
    public double ErrorRate { get; set; }

    [Option("glm_pvalue", MetaValue = "DOUBLE", DefaultValue = DEFAULT_GlmPvalue, HelpText = "Maximum adjusted pvalue used for GLM test")]
    public double GlmPvalue { get; set; }

    [Option('o', "output", MetaValue = "FILE", Required = true, HelpText = "Output file")]
    public string OutputFile { get; set; }

    [Option('i', "input", MetaValue = "FILE", Required = true, HelpText = "Input pileup result file (*.bases)")]
    public string InputFile { get; set; }

    public bool IsValidation { get; set; }

    public string TargetRFile
    {
      get { return new FileInfo(OutputFile + ".r").FullName.Replace("\\", "/"); }
    }

    public string GetRCommand()
    {
      return Config.FindOrCreate("R", "R").Command;
    }

    public string ROutputFile
    {
      get
      {
        if (IsValidation)
        {
          return OutputFile;
        }
        else
        {
          return Path.ChangeExtension(OutputFile, ".unfiltered.tsv");
        }
      }
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
          if (Path.GetDirectoryName(Path.GetFullPath(InputFile)).Equals(Path.GetDirectoryName(Path.GetFullPath(ROutputFile))))
          {
            sw.WriteLine("outputfile<-\"{0}\"", Path.GetFileName(ROutputFile));
          }
          else
          {
            sw.WriteLine("outputfile<-\"{0}\"", Path.GetFullPath(ROutputFile).Replace("\\", "/"));
          }
          sw.WriteLine("pvalue<-{0}", GlmPvalue);
          sw.WriteLine("errorrate<-{0}", ErrorRate);
          sw.WriteLine("isvalidation<-{0}", IsValidation ? "1" : "0");

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