using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RCPA.Commandline;
using CommandLine;
using System.IO;
using RCPA.Gui.Command;

namespace CQS.Genome.Cuffdiff
{
  public class CuffdiffSignificantFileMergerOptions : AbstractOptions
  {
    [OptionList('i', "inputFiles", Required = true, MetaValue = "FILES",  Separator = ',', HelpText = "Cuffdiff significant files")]
    public IList<string> InputFiles { get; set; }

    [Option('a', "affyAnnoationFile", Required=false, MetaValue="FILE", HelpText = "Affymetrix annotation file used to mapping gene symbol with gene description")]
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

      if (! string.IsNullOrEmpty(this.AffyAnnotationFile) && !File.Exists(this.AffyAnnotationFile))
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

    public bool Process(string[] args)
    {
      CuffdiffSignificantFileMergerOptions options;
      bool result = true;
      try
      {
        options = CommandLine.Parser.Default.ParseArguments<CuffdiffSignificantFileMergerOptions>(args,
          () =>
          {
            result = false;
          }
        );

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
      }
      catch (Exception ex)
      {
        Console.Error.WriteLine(ex.Message);
        result = false;
      }

      return result;
    }
  }
}
