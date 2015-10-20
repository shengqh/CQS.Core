using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommandLine;
using System.IO;
using RCPA.Commandline;

namespace CQS.Genome.Bacteria
{
  public class GffToBedConverterCommand : AbstractCommandLineCommand<GffToBedConverterOptions>
  {
    public override string Name
    {
      get { return "bacteria_gff2bed"; }
    }

    public override string Description
    {
      get { return "Convert bacteria gff to bed"; }
    }

    public override RCPA.IProcessor GetProcessor(GffToBedConverterOptions options)
    {
      return new GffToBedConverter(options);
    }
  }
}
