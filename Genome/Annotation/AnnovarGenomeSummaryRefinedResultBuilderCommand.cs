using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RCPA.Commandline;
using CommandLine;
using System.IO;
using RCPA.Gui.Command;

namespace CQS.Genome.Annotation
{
  public class AnnovarGenomeSummaryRefinedResultBuilderCommand : AbstractCommandLineCommand<AnnovarGenomeSummaryRefinedResultBuilderOptions>, IToolCommand
  {
    #region ICommandLineTool
    public override string Name
    {
      get { return "annovar_refine"; }
    }

    public override string Description
    {
      get { return "Refine annovar gene summmary file"; }
    }

    public override RCPA.IProcessor GetProcessor(AnnovarGenomeSummaryRefinedResultBuilderOptions options)
    {
      return new AnnovarGenomeSummaryRefinedResultTsvBuilder(options);
    }
    #endregion ICommandLineTool

    #region IToolCommand
    public string GetClassification()
    {
      return "Annotation";
    }

    public string GetCaption()
    {
      return AnnovarGenomeSummaryRefinedResultBuilderUI.title;
    }

    public string GetVersion()
    {
      return AnnovarGenomeSummaryRefinedResultBuilderUI.version;
    }

    public void Run()
    {
      new AnnovarGenomeSummaryRefinedResultBuilderUI().MyShow();
    }
    #endregion IToolCommand
  }
}
