using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RCPA.Commandline;
using CommandLine;
using System.IO;

namespace CQS.Genome.SmallRNA
{
  public class SmallRNAUnmappedReadBuilderCommand : AbstractCommandLineCommand<SmallRNAUnmappedReadBuilderOptions>
  {
    public override string Name
    {
      get { return "smallrna_unmapped"; }
    }

    public override string Description
    {
      get { return "Export unmapped reads to FASTQ files"; }
    }

    public override RCPA.IProcessor GetProcessor(SmallRNAUnmappedReadBuilderOptions options)
    {
      return new SmallRNAUnmappedReadBuilder(options);
    }
  }
}
