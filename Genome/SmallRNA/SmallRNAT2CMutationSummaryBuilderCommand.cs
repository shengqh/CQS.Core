using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RCPA.Commandline;
using CommandLine;
using System.IO;

namespace CQS.Genome.SmallRNA
{
  public class SmallRNAT2CMutationSummaryBuilderCommand : AbstractCommandLineCommand<SmallRNAT2CMutationSummaryBuilderOptions>
  {
    public override string Name
    {
      get { return "smallrna_t2c_summary"; }
    }

    public override string Description
    {
      get { return "Build smallRNA T2C mutaion summary"; }
    }

    public override RCPA.IProcessor GetProcessor(SmallRNAT2CMutationSummaryBuilderOptions options)
    {
      return new SmallRNAT2CMutationSummaryBuilder(options);
    }
  }
}
