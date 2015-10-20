using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RCPA.Commandline;
using CommandLine;
using System.IO;
using RCPA.Gui.Command;

namespace CQS.Genome.SmallRNA
{
  public class TGIRTCountProcessorCommand : AbstractCommandLineCommand<TGIRTCountProcessorOptions>
  {
    public override string Name
    {
      get { return "tgirt_count"; }
    }

    public override string Description
    {
      get { return "TGIRT - smallRNA count"; }
    }

    public override RCPA.IProcessor GetProcessor(TGIRTCountProcessorOptions options)
    {
      return new TGIRTCountProcessor(options);
    }
  }
}
