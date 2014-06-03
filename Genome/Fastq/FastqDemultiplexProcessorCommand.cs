using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CQS.Commandline;
using CommandLine;
using System.IO;
using RCPA.Gui.Command;

namespace CQS.Genome.Fastq
{
  public class FastqDemultiplexProcessorCommand : AbstractCommandLineCommand<FastqDemultiplexProcessorOptions>
  {
    public override string Name
    {
      get { return "fastq_demultiplex"; }
    }

    public override string Description
    {
      get { return "Demultiplex fastq file"; }
    }

    public override RCPA.IProcessor GetProcessor(FastqDemultiplexProcessorOptions options)
    {
      return new FastqDemultiplexProcessor(options);
    }
  }
}
