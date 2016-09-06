using CommandLine;
using RCPA.Commandline;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CQS.Genome.Parclip
{
  public class ParclipSmallRNAT2CBuilderOptions : AbstractOptions
  {
    public const double DEFAULT_ExpectRate = 0.013;
    public const double DEFAULT_Pvalue = 0.05;
    public const int DEFAULT_MinimumCount = 2;

    public ParclipSmallRNAT2CBuilderOptions()
    {
      this.ExpectRate = DEFAULT_ExpectRate;
      this.Pvalue = DEFAULT_Pvalue;
      this.MinimumCount = DEFAULT_MinimumCount;
    }

    [Option('i', "inputFile", Required = true, MetaValue = "FILE", HelpText = "Annovar gene summary report file")]
    public string InputFile { get; set; }

    [Option('o', "outputFile", Required = false, MetaValue = "FILE", HelpText = "Refined result file")]
    public string OutputFile { get; set; }

    [Option('e', "expectRate", Required = false, DefaultValue = DEFAULT_ExpectRate, MetaValue = "DOUBLE", HelpText = "Expect T2C rate in next generation sequencing data")]
    public double ExpectRate { get; set; }

    [Option('p', "pvalue", Required = false, DefaultValue = DEFAULT_Pvalue, MetaValue = "DOUBLE", HelpText = "Maximum pvalue for T2C mutation")]
    public double Pvalue { get; set; }

    [Option('c', "minimumCount", Required = false, DefaultValue = DEFAULT_MinimumCount, MetaValue = "INT", HelpText = "Minumim T2C count and not T2C count in the sample")]
    public int MinimumCount { get; set; }

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
