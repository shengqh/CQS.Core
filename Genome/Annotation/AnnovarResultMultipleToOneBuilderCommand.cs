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
  public class AnnovarResultMultipleToOneBuilderCommand : AbstractCommandLineCommand<AnnovarResultMultipleToOneBuilderOptions>
  {
    #region ICommandLineTool
    public override string Name
    {
      get { return "annovar_merge"; }
    }

    public override string Description
    {
      get { return "Merge annovar gene summmary file"; }
    }

    public override RCPA.IProcessor GetProcessor(AnnovarResultMultipleToOneBuilderOptions options)
    {
      return new AnnovarResultMultipleToOneBuilder(options);
    }
    #endregion ICommandLineTool
  }
}
