using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RCPA.Commandline;
using CommandLine;
using System.IO;
using RCPA.Gui.Command;

namespace CQS.Genome.Mirna
{
  public class MirnaNonTemplatedNucleotideAdditionsQueryBuilderCommand : AbstractCommandLineCommand<MirnaNonTemplatedNucleotideAdditionsQueryBuilderOptions>
  {
    public override string Name
    {
      get { return "fastq_mirna"; }
    }

    public override string Description
    {
      get { return "Clip fastq file for miRNA NTA detection"; }
    }

    public override RCPA.IProcessor GetProcessor(MirnaNonTemplatedNucleotideAdditionsQueryBuilderOptions options)
    {
      return new MirnaNonTemplatedNucleotideAdditionsQueryBuilder(options);
    }
  }
}
