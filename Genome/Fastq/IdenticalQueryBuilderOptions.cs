using CommandLine;
using RCPA.Commandline;
using System.IO;

namespace CQS.Genome.Fastq
{
  public class IdenticalQueryBuilderOptions : AbstractOptions
  {
    private const int DEFAULT_MinimumReadLength = 16;
    private const bool DEFAULT_OutputScores = false;
    private const bool DEFAULT_Gunzipped = false;

    public IdenticalQueryBuilderOptions()
    {
      this.MinimumReadLength = DEFAULT_MinimumReadLength;
      this.OutputScores = DEFAULT_OutputScores;
      this.Gunzipped = DEFAULT_Gunzipped;
    }

    [Option('i', "inputFile", Required = true, MetaValue = "FILE", HelpText = "Fastq file")]
    public string InputFile { get; set; }

    [Option('l', "minlen", MetaValue = "INT", DefaultValue = DEFAULT_MinimumReadLength, HelpText = "Minimum read length")]
    public int MinimumReadLength { get; set; }

    [Option('o', "outputFile", Required = false, MetaValue = "FILE", HelpText = "Output fastq file")]
    public string OutputFile { get; set; }

    [Option('s', "outputScore", DefaultValue = DEFAULT_OutputScores, HelpText = "Output score file")]
    public bool OutputScores { get; set; }

    [Option('u', "unzip", DefaultValue = DEFAULT_Gunzipped, HelpText = "Do not compress output file")]
    public bool Gunzipped { get; set; }

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
