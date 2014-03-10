using CommandLine;
using CQS.Commandline;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CQS.Genome.Sam
{
  public class BamSummaryBuilderOptions : AbstractOptions
  {
    public BamSummaryBuilderOptions()
    { }

    [Option('i', "rootDir", Required = true, MetaValue = "DIRECTORY", HelpText = "Root directory containing sub directories with samtools flagstat file")]
    public string InputDir { get; set; }

    [Option('o', "outputFile", Required = false, MetaValue = "FILE", HelpText = "Output file")]
    public string OutputFile { get; set; }

    public override bool PrepareOptions()
    {
      if (!Directory.Exists(this.InputDir))
      {
        ParsingErrors.Add(string.Format("Root directory not exists {0}.", this.InputDir));
        return false;
      }

      return true;
    }
  }
}
