using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RCPA.Commandline;
using CommandLine;
using System.IO;
using RCPA.Gui.Command;

namespace CQS.Genome.SomaticMutation
{
  public class AnnotationProcessorCommand : AbstractCommandLineCommand<AnnotationProcessorOptions>
  {
    #region ICommandLineTool
    public override string Name
    {
      get { return "annotation"; }
    }

    public override string Description
    {
      get { return "Annotate mutation using varies tools."; }
    }

    public override RCPA.IProcessor GetProcessor(AnnotationProcessorOptions options)
    {
      return new AnnotationProcessor(options);
    }
    #endregion ICommandLineTool
  }
}
