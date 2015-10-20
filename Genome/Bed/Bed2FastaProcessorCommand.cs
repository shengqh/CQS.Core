using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommandLine;
using RCPA.Commandline;
using System.IO;

namespace CQS.Genome.Bed
{
  public class Bed2FastaProcessorCommand : AbstractCommandLineCommand<Bed2FastaProcessorOptions>
  {
    public override string Name
    {
      get { return "bed2fasta"; }
    }

    public override string Description
    {
      get { return "bed to fasta"; }
    }

    public override RCPA.IProcessor GetProcessor(Bed2FastaProcessorOptions options)
    {
      return new Bed2FastaProcessor(options);
    }
  }
}
