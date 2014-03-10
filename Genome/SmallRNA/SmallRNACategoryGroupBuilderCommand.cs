using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CQS.Commandline;
using CommandLine;
using System.IO;
using RCPA.Gui.Command;
using CQS.Genome.Sam;

namespace CQS.Genome.SmallRNA
{
  public class SmallRNACategoryGroupBuilderCommand : AbstractCommandLineCommand<SmallRNACategoryGroupBuilderOptions>
  {
    #region ICommandLineCommand Members

    public override string Name
    {
      get { return "smallrna_group"; }
    }

    public override string Description
    {
      get { return "Count the reads based prioritized small RNA category (miRNA will be the first!)"; }
    }

    public override RCPA.IProcessor GetProcessor(SmallRNACategoryGroupBuilderOptions options)
    {
      return new SmallRNACategoryGroupBuilder(options);
    }

    #endregion
  }
}
