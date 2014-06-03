using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommandLine;
using System.IO;
using CQS.Commandline;

namespace CQS.Genome.Bacteria.Rockhopper
{
  public class RockhopperSummaryBuilderCommand : AbstractCommandLineCommand<RockhopperSummaryBuilderOptions>
  {
    public override string Name
    {
      get { return "rockhopper_summary"; }
    }

    public override string Description
    {
      get { return "Build summary from multiple rockhopper results"; }
    }

    public override RCPA.IProcessor GetProcessor(RockhopperSummaryBuilderOptions options)
    {
      return new RockhopperSummaryBuilder(options);
    }
  }
}
