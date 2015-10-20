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
  public class PipelineProcessorCommand : AbstractCommandLineCommand<PipelineProcessorOptions>
  {
    #region ICommandLineTool
    public override string Name
    {
      get { return "call"; }
    }

    public override string Description
    {
      get { return "Call somatic mutation, filter candidates by brglm model and finally annotate the result"; }
    }

    public override RCPA.IProcessor GetProcessor(PipelineProcessorOptions options)
    {
      return new PipelineProcessor(options);
    }
    #endregion ICommandLineTool
  }
}
