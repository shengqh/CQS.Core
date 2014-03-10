using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CQS.Commandline;
using CommandLine;
using System.IO;

namespace CQS.Genome.Fastq
{
  public class IdenticalQueryBuilderOptions : AbstractOptions
  {
    private const int DEFAULT_MinimumReadLength = 12;
    private const bool DEFAULT_NotOutputScores = false;
    private const bool DEFAULT_Gzipped = false;

    public IdenticalQueryBuilderOptions()
    {
      this.MinimumReadLength = DEFAULT_MinimumReadLength;
      this.NotOutputScores = DEFAULT_NotOutputScores;
    }

    [Option('i', "inputFile", Required = true, MetaValue = "FILE", HelpText = "Fastq file")]
    public string InputFile { get; set; }

    [Option('l', "minlen", MetaValue = "INT", DefaultValue = DEFAULT_MinimumReadLength, HelpText = "Minimum read length")]
    public int MinimumReadLength { get; set; }

    [Option('o', "outputFile", Required = false, MetaValue = "FILE", HelpText = "Output fastq file")]
    public string OutputFile { get; set; }

    [Option('n', "notOutputScore", DefaultValue = DEFAULT_NotOutputScores, HelpText = "Do not output score file")]
    public bool NotOutputScores { get; set; }

    [Option('z', "gzipped", DefaultValue = DEFAULT_Gzipped, HelpText = "Output file gzipped")]
    public bool Gzipped { get; set; }

    public override bool PrepareOptions()
    {
      if (!File.Exists(this.InputFile))
      {
        ParsingErrors.Add(string.Format("Input file not exists {0}.", this.InputFile));
        return false;
      }

      if (string.IsNullOrEmpty(this.OutputFile))
      {
        this.OutputFile = Path.ChangeExtension(InputFile, "_identical.fastq");
      }

      return true;
    }
  }
}
