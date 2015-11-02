using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RCPA.Commandline;
using CommandLine;
using System.IO;
using RCPA.Gui.Command;
using CQS.Genome.Sam;

namespace CQS.Genome.Database
{
  public class DatabaseReorderProcessorCommand : AbstractCommandLineCommand<DatabaseReorderProcessorOptions>
  {
    public override string Name
    {
      get { return "database_reorder"; }
    }

    public override string Description
    {
      get { return "Reorder chromosomes of database"; }
    }

    public override RCPA.IProcessor GetProcessor(DatabaseReorderProcessorOptions options)
    {
      return new DatabaseReorderProcessor(options);
    }
  }
}
