using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CQS.Commandline;
using CommandLine;
using System.IO;

namespace CQS.Genome.Fastq
{
  public class FastqTrimmerOptions : AbstractOptions
  {
    private const int DEFAULT_Start = 1;
    private const int DEFAULT_Last = 0;
    private const bool DEFAULT_Gzipped = false;
    private const bool DEFAULT_TrimN = false;

    public FastqTrimmerOptions()
    {
    }

    [Option('i', "inputFile", Required = true, MetaValue = "FILE", HelpText = "Fastq file")]
    public string InputFile { get; set; }

    [Option('f', "first", MetaValue = "INT", DefaultValue = DEFAULT_Start, HelpText = "First base to keep, 1=first base.")]
    public int Start { get; set; }

    [Option('l', "last", MetaValue = "INT", DefaultValue = DEFAULT_Last, HelpText = "Last base to keep. 0=entire read.")]
    public int Last { get; set; }

    [Option('o', "outputFile", Required = false, MetaValue = "FILE", HelpText = "Output fastq file")]
    public string OutputFile { get; set; }

    [Option('n', "trimN", DefaultValue = DEFAULT_TrimN, HelpText = "Trim terminal N")]
    public bool TrimN { get; set; }

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
        this.OutputFile = Path.ChangeExtension(InputFile, "_trim.fastq");
      }

      return true;
    }
  }
}
