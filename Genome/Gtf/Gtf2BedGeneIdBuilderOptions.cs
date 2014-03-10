using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CQS.Commandline;
using System.IO;
using CommandLine;

namespace CQS.Genome.Gtf
{
  public class Gtf2BedGeneIdBuilderOptions : AbstractOptions
  {
    [Option('i', "InputFile", Required = true, MetaValue = "FILE", HelpText = "Input gtf file")]
    public string InputFile { get; set; }

    [Option('o', "OutputPrefix", Required = true, MetaValue = "FILE", HelpText = "output map file")]
    public string OutputFile { get; set; }

    public override bool PrepareOptions()
    {
      if (!File.Exists(this.InputFile))
      {
        ParsingErrors.Add(string.Format("Directory not exists {0}.", this.InputFile));
        return false;
      }

      return true;
    }
  }
}
