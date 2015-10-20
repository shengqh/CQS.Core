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
  public class TrnaNonTemplatedNucleotideAdditionsQueryBuilderCommand : AbstractCommandLineCommand<TrnaNonTemplatedNucleotideAdditionsQueryBuilderOptions>
  {
    public override string Name
    {
      get { return "tgirt_nta"; }
    }

    public override string Description
    {
      get { return "TGIRT - clip fastq file for TRNA CCA/CCAA NTA"; }
    }

    public override RCPA.IProcessor GetProcessor(TrnaNonTemplatedNucleotideAdditionsQueryBuilderOptions options)
    {
      return new TrnaNonTemplatedNucleotideAdditionsQueryBuilder(options);
    }
  }
}
