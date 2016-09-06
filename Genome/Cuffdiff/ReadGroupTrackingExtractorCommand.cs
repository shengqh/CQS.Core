using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RCPA.Commandline;
using CommandLine;
using System.IO;
using RCPA.Gui.Command;
using RCPA;

namespace CQS.Genome.Cuffdiff
{
  public class ReadGroupTrackingExtractorOptions : AbstractOptions
  {
    [OptionList('i', "inputFiles", Required = true, MetaValue = "FILES", Separator = ',', HelpText = "Cuffdiff read_group_tracking files")]
    public IList<string> InputFiles { get; set; }

    [OptionList('s', "significantFiles", Required = true, MetaValue = "FILES", HelpText = "Cuffdiff significant files")]
    public IList<string> SignificantFiles { get; set; }

    [Option('m', "mapFile", Required = false, MetaValue = "FILE", HelpText = "Cuffdiff group/sample map file")]
    public string MapFile { get; set; }

    [Option('o', "outputFilePrefix", Required = true, MetaValue = "FILE", HelpText = "Result file prefix")]
    public string OutputFilePrefix { get; set; }

    public override bool PrepareOptions()
    {
      foreach (var file in this.InputFiles)
      {
        if (!File.Exists(file))
        {
          ParsingErrors.Add(string.Format("Cuffdiff group tracking file not exists {0}.", file));
          return false;
        }
      }

      foreach (var file in this.SignificantFiles)
      {
        if (!File.Exists(file))
        {
          ParsingErrors.Add(string.Format("Cuffdiff significant file not exists {0}.", file));
          return false;
        }
      }

      if (!string.IsNullOrEmpty(this.MapFile) && !File.Exists(this.MapFile))
      {
        ParsingErrors.Add(string.Format("Cuffdiff group/sample map file not exists {0}.", this.MapFile));
        return false;
      }

      return true;
    }
  }

  public class ReadGroupTrackingExtractorCommand : AbstractCommandLineCommand<ReadGroupTrackingExtractorOptions>, IToolCommand
  {
    public override string Name
    {
      get { return "cuffdiff_count"; }
    }

    public override string Description
    {
      get { return "Extract count/rpkm from cuffdiff read_group_tracking file"; }
    }

    public override IProcessor GetProcessor(ReadGroupTrackingExtractorOptions options)
    {
      return new ReadGroupTrackingExtractor(options);
    }

    public string GetClassification()
    {
      return "Quantification";
    }

    public string GetCaption()
    {
      return ReadGroupTrackingExtractorUI.title;
    }

    public string GetVersion()
    {
      return ReadGroupTrackingExtractorUI.version;
    }

    public void Run()
    {
      new ReadGroupTrackingExtractorUI().MyShow();
    }
  }
}
