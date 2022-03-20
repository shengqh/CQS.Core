using CommandLine;
using RCPA.Commandline;
using System.IO;

namespace CQS.Genome.Depth
{
  public class DepthProcessorOptions : AbstractOptions
  {
    private const int DefaultMinimimDepthInEachSample = 1;

    public DepthProcessorOptions()
    {
      MinimimDepthInEachSample = DefaultMinimimDepthInEachSample;
    }

    [Option('i', "inputFile", MetaValue = "FILE", HelpText = "Coordinate file (vcf format)")]
    public string InputFile { get; set; }

    [Option('d', "mindepth", MetaValue = "INT", DefaultValue = DefaultMinimimDepthInEachSample, HelpText = "Minimum depth in each sample")]
    public int MinimimDepthInEachSample { get; set; }

    [Option('o', "outputFile", MetaValue = "FILE", HelpText = "Output file (tab delimtered format)")]
    public string OutputFile { get; set; }

    public override bool PrepareOptions()
    {
      if (!string.IsNullOrEmpty(this.InputFile) && !File.Exists(this.InputFile))
      {
        ParsingErrors.Add(string.Format("Input file not exists {0}.", this.InputFile));
        return false;
      }

      return true;
    }
  }
}
