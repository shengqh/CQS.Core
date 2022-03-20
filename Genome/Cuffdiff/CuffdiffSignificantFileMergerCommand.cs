using CommandLine;
using RCPA.Commandline;
using System;
using System.Collections.Generic;
using System.IO;

namespace CQS.Genome.Cuffdiff
{
  public class CuffdiffSignificantFileMergerOptions : AbstractOptions
  {
    [OptionList('i', "inputFiles", Required = true, MetaValue = "FILES", Separator = ',', HelpText = "Cuffdiff significant files")]
    public IList<string> InputFiles { get; set; }

    [Option('a', "affyAnnoationFile", Required = false, MetaValue = "FILE", HelpText = "Affymetrix annotation file used to mapping gene symbol with gene description")]
    public string AffyAnnotationFile { get; set; }

    [Option('o', "outputFile", Required = false, MetaValue = "FILE", HelpText = "Combined cuffdiff significant file")]
    public string OutputFile { get; set; }

    public override bool PrepareOptions()
    {
      foreach (var file in this.InputFiles)
      {
        if (!File.Exists(file))
        {
          ParsingErrors.Add(string.Format("Input file not exists {0}.", file));
          return false;
        }
      }

      if (!string.IsNullOrEmpty(this.AffyAnnotationFile) && !File.Exists(this.AffyAnnotationFile))
      {
        ParsingErrors.Add(string.Format("Affymetrix annotation file not exists {0}.", this.AffyAnnotationFile));
        return false;
      }

      return true;
    }
  }

  public class CuffdiffSignificantFileMergerCommand : ICommandLineCommand
  {
    public string Name
    {
      get { return "cuffdiff_merge"; }
    }

    public string Description
    {
      get { return "Merging multiple cuffdiff result as one whole report"; }
    }

    public string SoftwareName { get; set; }

    public string SoftwareVersion { get; set; }

    public bool LinuxSupported { get { return true; } }

    public bool Process(string[] args)
    {
      var options = new CuffdiffSignificantFileMergerOptions();
      var result = Parser.Default.ParseArguments(args, options);
      if (result)
      {
        if (!options.PrepareOptions())
        {
          Console.Out.WriteLine(options.GetUsage());
          result = false;
        }
        else
        {
          var files = new CuffdiffSignificantFileMerger(options.AffyAnnotationFile, options.InputFiles).Process(options.OutputFile);
          Console.WriteLine("File saved to :\n" + files.Merge("\n"));
        }
      }

      return result;
    }
  }
}
