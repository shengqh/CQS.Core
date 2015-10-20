using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RCPA.Commandline;
using CommandLine;
using System.IO;
using RCPA.Gui.Command;

namespace CQS.Genome.Sam
{
  public class BamCleanerCommand : AbstractCommandLineCommand<BamCleanerOptions>
  {
    #region ICommandLineTool
    public override string Name
    {
      get { return "bam_clean"; }
    }

    public override string Description
    {
      get { return "Keep the bam file with longest file name and delete any other bam files (for failed refinement)"; }
    }

    public override RCPA.IProcessor GetProcessor(BamCleanerOptions options)
    {
      return new BamCleaner(options);
    }
    #endregion ICommandLineTool
  }
}
