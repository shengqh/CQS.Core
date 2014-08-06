using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CQS.Commandline;
using CommandLine;
using System.IO;
using RCPA.Gui.Command;
using CQS.Genome.Sam;

namespace CQS.Genome.Pileup
{
  public class AlleleCountBuilderCommand : AbstractCommandLineCommand<AlleleCountBuilderOptions>
  {
    public override string Name
    {
      get { return "cqs_allelecount"; }
    }

    public override string Description
    {
      get { return "Get count of major/minor allele from bam/sam file "; }
    }

    public override RCPA.IProcessor GetProcessor(AlleleCountBuilderOptions options)
    {
      return new AlleleCountBuilder(options);
    }
  }
}
