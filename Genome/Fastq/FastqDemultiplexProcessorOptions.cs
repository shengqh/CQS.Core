using CommandLine;
using RCPA.Commandline;
using System.IO;

namespace CQS.Genome.Fastq
{
  public class FastqDemultiplexProcessorOptions : AbstractOptions
  {
    private const bool DEFAULT_Ungzipped = false;
    private const bool DEFAULT_UntrimTerminalN = false;

    public FastqDemultiplexProcessorOptions()
    {
      this.UntrimTerminalN = DEFAULT_UntrimTerminalN;
    }

    [Option('m', "mappingFile", Required = true, MetaValue = "FILE", HelpText = "Mapping file, first column is index and second column is filename")]
    public string MappingFile { get; set; }

    [Option('i', "inputFile", Required = true, MetaValue = "FILE", HelpText = "Fastq file")]
    public string InputFile { get; set; }

    [Option('o', "OutputDirectory", Required = true, MetaValue = "DIRECTORY", HelpText = "Output directory")]
    public string OutputDirectory { get; set; }

    [Option('u', "untrimTerminalN", DefaultValue = DEFAULT_UntrimTerminalN, HelpText = "Untrim terminal N")]
    public bool UntrimTerminalN { get; set; }

    [Option('s', "summaryFile", Required = true, MetaValue = "FILE", HelpText = "Output count summary file name, will be writen into output directory")]
    public string SummaryFile { get; set; }

    public override bool PrepareOptions()
    {
      if (!File.Exists(this.MappingFile))
      {
        ParsingErrors.Add(string.Format("Mapping file not exists {0}.", this.MappingFile));
        return false;
      }

      if (!File.Exists(this.InputFile))
      {
        ParsingErrors.Add(string.Format("Input file not exists {0}.", this.InputFile));
        return false;
      }

      return true;
    }
  }
}
