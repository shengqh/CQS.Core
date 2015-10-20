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
  public class PileupProcessorCommand : AbstractCommandLineCommand<PileupProcessorOptions>
  {
    #region ICommandLineTool
    public override string Name
    {
      get { return "pileup"; }
    }

    public override string Description
    {
      get { return "Initialize candidates from samtools mpileup result."; }
    }

    public override RCPA.IProcessor GetProcessor(PileupProcessorOptions options)
    {
      return options.GetProcessor();
    }

    #endregion ICommandLineTool
  }
}
