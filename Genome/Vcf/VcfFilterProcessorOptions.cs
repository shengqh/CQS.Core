using System.Collections.Generic;
using Bio.Util;
using CommandLine;
using RCPA.Commandline;
using System;
using System.IO;
using System.Linq;

namespace CQS.Genome.Vcf
{
  public class VcfFilterProcessorOptions : AbstractOptions
  {
    private const int DEFAULT_MinimumMedianDepth = 3;

    public VcfFilterProcessorOptions()
    {
      this.MinimumMedianDepth = DEFAULT_MinimumMedianDepth;
    }

    [Option('i', "input", Required = true, MetaValue = "FILE", HelpText = "Input vcf file")]
    public string InputFile { get; set; }

    [Option('o', "output", Required = true, MetaValue = "FILE", HelpText = "Output vcf file")]
    public string OutputFile { get; set; }

    [Option('d', "depth", DefaultValue = DEFAULT_MinimumMedianDepth, MetaValue = "INT", HelpText = "Minimum median depth")]
    public int MinimumMedianDepth { get; set; }

    public override bool PrepareOptions()
    {
      if (!File.Exists(this.InputFile))
      {
        ParsingErrors.Add(string.Format("Input file not exists {0}.", this.InputFile));
      }

      return ParsingErrors.Count == 0;
    }
  }
}
