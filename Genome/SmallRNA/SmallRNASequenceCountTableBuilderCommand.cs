using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RCPA.Commandline;
using CommandLine;
using System.IO;

namespace CQS.Genome.SmallRNA
{
  public class SmallRNASequenceCountTableBuilderCommand : AbstractCommandLineCommand<SmallRNASequenceCountTableBuilderOptions>
  {
    public override string Name
    {
      get { return "smallrna_sequence_count_table"; }
    }

    public override string Description
    {
      get { return "Build smallRNA identical sequence count table"; }
    }

    public override RCPA.IProcessor GetProcessor(SmallRNASequenceCountTableBuilderOptions options)
    {
      return new SmallRNASequenceCountTableBuilder(options);
    }
  }
}
