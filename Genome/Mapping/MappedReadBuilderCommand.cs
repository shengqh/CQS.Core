using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RCPA.Commandline;
using CommandLine;
using System.IO;
using RCPA.Gui.Command;
using CQS.Genome.Sam;

namespace CQS.Genome.Mapping
{
  public class MappedReadBuilderCommand : AbstractCommandLineCommand<MappedReadBuilderOptions>
  {
    public override string Name
    {
      get { return "mapped_reads"; }
    }

    public override string Description
    {
      get { return "Extract perfect mapped reads with count and sequence"; }
    }

    public override RCPA.IProcessor GetProcessor(MappedReadBuilderOptions options)
    {
      return new MappedReadBuilder(options);
    }
  }
}
