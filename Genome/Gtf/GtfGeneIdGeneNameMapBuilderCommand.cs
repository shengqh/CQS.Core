using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommandLine;
using System.IO;
using CQS.Commandline;

namespace CQS.Genome.Gtf
{
  public class GtfGeneIdGeneNameMapBuilderCommand : AbstractCommandLineCommand<GtfGeneIdGeneNameMapBuilderOptions>
  {
    public override string Name
    {
      get { return "gtf_buildmap"; }
    }

    public override string Description
    {
      get { return "Build gene_id gene_name map from gtf file"; }
    }

    public override RCPA.IProcessor GetProcessor(GtfGeneIdGeneNameMapBuilderOptions options)
    {
      return new GtfGeneIdGeneNameMapBuilder(options);
    }
  }
}
