﻿using RCPA.Commandline;

namespace CQS.Genome.Tophat
{
  public class TophatSummaryBuilderCommand : AbstractCommandLineCommand<TophatSummaryBuilderOptions>
  {
    #region ICommandLineTool
    public override string Name
    {
      get { return "tophat_summary"; }
    }

    public override string Description
    {
      get { return "Summarize tophat mapping result"; }
    }

    public override RCPA.IProcessor GetProcessor(TophatSummaryBuilderOptions options)
    {
      return new TophatSummaryBuilder(options);
    }
    #endregion ICommandLineTool
  }
}
