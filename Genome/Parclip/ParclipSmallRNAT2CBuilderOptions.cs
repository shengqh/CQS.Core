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
    public ParclipSmallRNAT2CBuilderOptions()
    {
    }

    [Option('i', "inputFile", Required = true, MetaValue = "FILE", HelpText = "Annovar gene summary report file")]
    public string InputFile { get; set; }

    [Option('o', "outputFile", Required = false, MetaValue = "FILE", HelpText = "Refined result file")]
    public string OutputFile { get; set; }

    [Option('e', "expectRate", Required = false, DefaultValue=0.013, MetaValue = "DOUBLE", HelpText = "Expect T2C rate in next generation sequencing data")]
    public double ExpectRate { get; set; }

    [Option('p', "pvalue", Required = false, DefaultValue = 0.05, MetaValue = "DOUBLE", HelpText = "Maximum pValue")]
    public double PValue { get; set; }

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
