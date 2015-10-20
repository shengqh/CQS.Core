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
  public class FastqTrimmerCommand : AbstractCommandLineCommand<FastqTrimmerOptions>
  {
    public override string Name
    {
      get { return "fastq_trimmer"; }
    }

    public override string Description
    {
      get { return "Trim fastq file"; }
    }

    public override RCPA.IProcessor GetProcessor(FastqTrimmerOptions options)
    {
      return new FastqTrimmer(options);
    }
  }
}
