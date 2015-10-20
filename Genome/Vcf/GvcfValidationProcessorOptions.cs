using System.Collections.Generic;
using Bio.Util;
using CommandLine;
using RCPA.Commandline;
using System;
using System.IO;
using System.Linq;

namespace CQS.Genome.Vcf
{
  public class GvcfValidationProcessorOptions : AbstractOptions
  {
    public GvcfValidationProcessorOptions()
    {
    }

    [Option('i', "input", Required = true, MetaValue = "DIRECTORY", HelpText = "Directory/subdirectories including gvcf files")]
    public string InputDirectory { get; set; }

    public override bool PrepareOptions()
    {
      if (!Directory.Exists(this.InputDirectory))
      {
        ParsingErrors.Add(string.Format("Input directory not exists {0}.", this.InputDirectory));
      }

      var gvcffiles = GetGvcfFiles();
      if (gvcffiles.Length == 0)
      {
        ParsingErrors.Add(string.Format("No .g.vcf file found in directory {0}.", this.InputDirectory));
      }

      return ParsingErrors.Count == 0;
    }

    public string[] GetGvcfFiles()
    {
      return Directory.GetFiles(this.InputDirectory, "*.g.vcf", SearchOption.AllDirectories);
    }
  }
}
