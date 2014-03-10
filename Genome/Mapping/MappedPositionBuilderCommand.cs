using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CQS.Commandline;
using CommandLine;
using System.IO;

namespace CQS.Genome.Mapping
{
  public class MappedPositionBuilderCommand : AbstractCommandLineCommand<SimpleDataTableBuilderOptions>
  {
    public override string Name
    {
      get { return "mapped_position"; }
    }

    public override string Description
    {
      get { return "Build mapped position/percentage table from files"; }
    }

    public override RCPA.IProcessor GetProcessor(SimpleDataTableBuilderOptions options)
    {
      return new MappedPositionBuilder(options);
    }
  }
}
