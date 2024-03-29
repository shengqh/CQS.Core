﻿using CommandLine;
using System.IO;

namespace CQS.Genome.Parclip
{
  public class ParclipSmallRNATargetBuilderOptions : AbstractTargetBuilderOptions
  {
    private const int DEFAULT_SeedOffset = 1;

    public ParclipSmallRNATargetBuilderOptions()
    { }

    /// <summary>
    /// T2C xml file generated by SmallRNA T2C Builder
    /// </summary>
    [Option('i', "inputFile", Required = true, MetaValue = "FILE", HelpText = "T2C xml file generated by SmallRNA T2C Builder")]
    public string InputFile { get; set; }

    [Option("seedOffset", DefaultValue = DEFAULT_SeedOffset, MetaValue = "INTEGER", HelpText = "The seed offset of smallRNA")]
    public override int SeedOffset { get; set; }

    public override bool PrepareOptions()
    {
      base.PrepareOptions();

      if (!File.Exists(this.InputFile))
      {
        ParsingErrors.Add(string.Format("Input T2C count xml file not exists {0}.", this.InputFile));
      }

      return ParsingErrors.Count == 0;
    }
  }
}
