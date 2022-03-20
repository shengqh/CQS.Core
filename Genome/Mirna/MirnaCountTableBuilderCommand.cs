﻿using RCPA.Commandline;

namespace CQS.Genome.Mirna
{
  public class MirnaDataTableBuilderCommand : AbstractCommandLineCommand<MirnaCountTableBuilderOptions>
  {
    public override string Name
    {
      get { return "mirna_table"; }
    }

    public override string Description
    {
      get { return "Build miRNA data table from files located in subdirectories of root directory"; }
    }

    public override RCPA.IProcessor GetProcessor(MirnaCountTableBuilderOptions options)
    {
      return new MirnaCountTableBuilder(options);
    }
  }
}
