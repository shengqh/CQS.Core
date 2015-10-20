using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RCPA.Commandline;
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
    private const int DEFAULT_MINLEN = 0;

    public FastqTrimmerOptions()
    {
      MinimumLength = DEFAULT_MINLEN;
    }

    [OptionList('i', "inputFiles", Required = true, MetaValue = "FILES", Separator = ',', HelpText = "Input FASTQ files")]
    public IList<string> InputFiles { get; set; }

    [Option('f', "first", MetaValue = "INT", DefaultValue = DEFAULT_Start, HelpText = "First base to keep, 1=first base.")]
    public int Start { get; set; }

    [Option('l', "last", MetaValue = "INT", DefaultValue = DEFAULT_Last, HelpText = "Last base to keep. 0=entire read.")]
    public int Last { get; set; }

    [OptionList('o', "outputFiles", Required = false, MetaValue = "FILES", Separator = ',', HelpText = "Output FASTQ files")]
    public IList<string> OutputFiles { get; set; }

    [Option('n', "trimN", DefaultValue = DEFAULT_TrimN, HelpText = "Trim terminal N")]
    public bool TrimN { get; set; }

    [Option('m', "minimumLength", DefaultValue = DEFAULT_MINLEN, HelpText = "Minimum length of reads")]
    public int MinimumLength { get; set; }

    [Option('z', "gzipped", DefaultValue = DEFAULT_Gzipped, HelpText = "Output file gzipped")]
    public bool Gzipped { get; set; }

    public override bool PrepareOptions()
    {
      foreach (var file in this.InputFiles)
      {
        if (!File.Exists(file))
        {
          ParsingErrors.Add(string.Format("Input file not exists {0}.", file));
        }
      }

      if (this.OutputFiles == null || this.OutputFiles.Count != this.InputFiles.Count)
      {

        this.OutputFiles = (from f in this.InputFiles select Path.ChangeExtension(f, "_trim.fastq")).ToList();
      }

      return ParsingErrors.Count == 0;
    }
  }
}
