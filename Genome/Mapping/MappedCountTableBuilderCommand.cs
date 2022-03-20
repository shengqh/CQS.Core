﻿using RCPA.Commandline;

namespace CQS.Genome.Mapping
{
  public class MappedCountTableBuilderCommand : AbstractCommandLineCommand<DataTableBuilderOptions>
  {
    public override string Name
    {
      get { return "mapped_table"; }
    }

    public override string Description
    {
      get { return "Build mapped data table from files located in subdirectories of root directory"; }
    }

    public override RCPA.IProcessor GetProcessor(DataTableBuilderOptions options)
    {
      return new MappedCountTableBuilder(options);
    }
  }
}
