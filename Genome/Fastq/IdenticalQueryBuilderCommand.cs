using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RCPA.Commandline;
using CommandLine;
using System.IO;
using RCPA.Gui.Command;

namespace CQS.Genome.Fastq
{
  public class IdenticalQueryBuilderCommand : AbstractCommandLineCommand<IdenticalQueryBuilderOptions>
  {
    public override string Name
    {
      get { return "fastq_identical"; }
    }

    public override string Description
    {
      get { return "Refine fastq file to keep identical query only"; }
    }

    public override RCPA.IProcessor GetProcessor(IdenticalQueryBuilderOptions options)
    {
      return new IdenticalQueryBuilder(options);
    }
  }
}
