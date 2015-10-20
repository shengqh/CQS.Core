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
  public class RestoreCCABuilderCommand : AbstractCommandLineCommand<RestoreCCABuilderOptions>
  {
    public override string Name
    {
      get { return "tgirt_checkcca"; }
    }

    public override string Description
    {
      get { return "TGIRT - Check 3' CCA after adaptor trimming"; }
    }

    public override RCPA.IProcessor GetProcessor(RestoreCCABuilderOptions options)
    {
      return new RestoreCCABuilder(options);
    }
  }
}
