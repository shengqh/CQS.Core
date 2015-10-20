using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RCPA.Commandline;
using CommandLine;
using System.IO;

namespace CQS.Genome.Fastq
{
  public class ValidFastqExtractorOptions : AbstractOptions
  {
    public ValidFastqExtractorOptions()
    { }

    [Option('i', "inputFile", Required = true, MetaValue = "FILE", HelpText = "Input FASTQ file")]
    public string InputFile { get; set; }

    [Option('o', "outputFile", Required = false, MetaValue = "FILE", HelpText = "Output FASTQ file")]
    public string OutputFile { get; set; }

    public override bool PrepareOptions()
    {
      if (!File.Exists(InputFile))
      {
        ParsingErrors.Add(string.Format("Input file not exists {0}.", InputFile));
      }

      return ParsingErrors.Count == 0;
    }
  }
}
