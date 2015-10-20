using CommandLine;
using RCPA.Commandline;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CQS.Genome.Sam
{
  public class BamCleanerOptions : AbstractOptions
  {
    public BamCleanerOptions()
    { }

    [Option('i', "rootDir", Required = true, MetaValue = "DIRECTORY", HelpText = "Root directory containing sub directories with bam files")]
    public string InputDir { get; set; }

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
