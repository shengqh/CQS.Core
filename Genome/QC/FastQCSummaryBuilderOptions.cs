using CommandLine;
using RCPA.Commandline;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace CQS.Genome.QC
{
  public class FastQCSummaryBuilderOptions : AbstractOptions
  {
    public FastQCSummaryBuilderOptions()
    { }

    [Option('i', "inputDir", Required = true, MetaValue = "DIR", HelpText = "Input directory")]
    public string InputDir { get; set; }

    [Option('o', "outputFile", Required = false, MetaValue = "FILE", HelpText = "Output file")]
    public string OutputFile { get; set; }

    public override bool PrepareOptions()
    {
      if (!Directory.Exists(this.InputDir))
      {
        ParsingErrors.Add(string.Format("Input directory not exists {0}.", this.InputDir));
      }
      
      return ParsingErrors.Count == 0;
    }
  }
}
