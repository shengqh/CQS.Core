using CommandLine;
using RCPA.Commandline;

namespace CQS.Genome.Mapping
{
  public class SamExtractorOptions : AbstractOptions
  {
    public SamExtractorOptions()
    { }

    [Option("bam", Required = true, MetaValue = "FILE", HelpText = "Input bam file")]
    public string BamFile { get; set; }

    [Option("count", Required = true, MetaValue = "FILE", HelpText = "Input count xml file")]
    public string CountFile { get; set; }

    [Option('o', "output", Required = true, MetaValue = "FILE", HelpText = "Output sam file")]
    public string OutputFile { get; set; }

    public override bool PrepareOptions()
    {
      CheckFile("bam", BamFile);

      CheckFile("count", CountFile);

      return ParsingErrors.Count == 0;
    }
  }
}