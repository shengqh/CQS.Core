using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RCPA.Commandline;
using CommandLine;
using System.IO;

namespace CQS.Genome.Fastq
{
  public class RestoreCCABuilderOptions : AbstractOptions
  {
    public RestoreCCABuilderOptions()
    { }

    [Option('i', "inputFile", Required = true, MetaValue = "FILE", HelpText = "Trimmed fastq file")]
    public string InputFile { get; set; }

    [Option('u', "untrimmedFile", Required = true, MetaValue = "FILE", HelpText = "Untrimmed fastq file")]
    public string UntrimmedFile { get; set; }

    [Option('o', "outputFile", Required = true, MetaValue = "FILE", HelpText = "Output fastq file")]
    public string OutputFile { get; set; }

    public override bool PrepareOptions()
    {
      if (!File.Exists(this.InputFile))
      {
        ParsingErrors.Add(string.Format("Input file not exists {0}.", this.InputFile));
      }

      if (!File.Exists(this.UntrimmedFile))
      {
        ParsingErrors.Add(string.Format("Untrimmed file not exists {0}.", this.UntrimmedFile));
      }

      return ParsingErrors.Count == 0;
    }
  }
}
