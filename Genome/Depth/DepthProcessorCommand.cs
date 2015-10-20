using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RCPA.Commandline;
using CommandLine;
using System.IO;
using RCPA.Gui.Command;
using CQS.Genome.Sam;

namespace CQS.Genome.Depth
{
  public class DepthProcessorCommand : AbstractCommandLineCommand<DepthProcessorOptions>
  {
    public override string Name
    {
      get { return "depth_filter"; }
    }

    public override string Description
    {
      get { return "Filter samtools depth result by minimum depth of all samples "; }
    }

    public override RCPA.IProcessor GetProcessor(DepthProcessorOptions options)
    {
      return new DepthProcessor(options);
    }
  }
}
