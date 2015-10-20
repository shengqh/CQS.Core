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
  public class ValidationProcessorCommand : AbstractCommandLineCommand<ValidationProcessorOptions>
  {
    #region ICommandLineTool
    public override string Name
    {
      get { return "validate"; }
    }

    public override string Description
    {
      get { return "Validate somatic mutation in vcf/bed file."; }
    }

    public override RCPA.IProcessor GetProcessor(ValidationProcessorOptions options)
    {
      return options.GetProcessor();
    }
    #endregion ICommandLineTool
  }
}
