using CommandLine;
using CQS.Commandline;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CQS.Genome.Mapping
{
  public class CountMappingSummaryBuilderOptions : AbstractOptions
  {
    [Option('i', "inputFile", Required = true, MetaValue = "FILE", HelpText = "Input configuration file")]
    public string InputFile { get; set; }

    [Option('o', "outputFile", Required = true, MetaValue = "FILE", HelpText = "Output summary file")]
    public string OutputFile { get; set; }

    public override bool PrepareOptions()
    {
      if (!File.Exists(this.InputFile))
      {
        ParsingErrors.Add(string.Format("Input file not exists {0}.", this.InputFile));
        return false;
      }

      return true;
    }
  }
}
