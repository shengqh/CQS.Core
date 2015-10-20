using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RCPA.Commandline;
using CommandLine;
using System.IO;
using RCPA.Gui.Command;
using CQS.Genome.Sam;

namespace CQS.Genome.SmallRNA
{
  public class SmallRNADatabaseBuilderCommand : AbstractCommandLineCommand<SmallRNADatabaseBuilderOptions>
  {
    #region ICommandLineCommand Members

    public override string Name
    {
      get { return "smallrna_database"; }
    }

    public override string Description
    {
      get { return "Build smallRNA database"; }
    }

    public override RCPA.IProcessor GetProcessor(SmallRNADatabaseBuilderOptions options)
    {
      return new SmallRNADatabaseBuilder(options);
    }

    #endregion
  }
}
