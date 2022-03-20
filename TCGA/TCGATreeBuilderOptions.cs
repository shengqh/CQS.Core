﻿using CommandLine;
using RCPA.Commandline;
using System.IO;

namespace CQS.TCGA
{
  public class TCGATreeBuilderOptions : AbstractOptions
  {
    [Option('o', "output", Required = true, MetaValue = "DIRECTORY", HelpText = "Output directory")]
    public string OutputDirectory { get; set; }

    public override bool PrepareOptions()
    {
      if (!Directory.Exists(this.OutputDirectory))
      {
        ParsingErrors.Add(string.Format("Directory not exists {0}.", this.OutputDirectory));
        return false;
      }

      return true;
    }
  }
}
