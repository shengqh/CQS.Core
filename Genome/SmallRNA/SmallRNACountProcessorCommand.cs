using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RCPA.Commandline;
using CommandLine;
using System.IO;

namespace CQS.Genome.SmallRNA
{
  public class SmallRNACountProcessorCommand : AbstractCommandLineCommand<SmallRNACountProcessorOptions>
  {
    public override string Name
    {
      get { return "smallrna_count"; }
    }

    public override string Description
    {
      get { return "Parsing smallRNA count from bam/sam file"; }
    }

    public override RCPA.IProcessor GetProcessor(SmallRNACountProcessorOptions options)
    {
      return new SmallRNACountProcessor(options);
    }
  }
}
