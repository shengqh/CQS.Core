using CommandLine;
using RCPA.Commandline;
using System.IO;

namespace CQS.Genome.CNV
{
  public class CNVItemTableBuilderOptions : AbstractOptions
  {
    [Option('i', "inputFile", Required = true, MetaValue = "FILE", HelpText = "Input file")]
    public string InputFile { get; set; }

    [Option('b', "bedFile", Required = true, MetaValue = "FILE", HelpText = "Bed file")]
    public string BedFile { get; set; }

    [Option('o', "outputFile", Required = false, MetaValue = "FILE", HelpText = "Output table file")]
    public string OutputFile { get; set; }

    public override bool PrepareOptions()
    {
      if (!File.Exists(this.InputFile))
      {
        ParsingErrors.Add(string.Format("Input file not exists {0}.", this.InputFile));
        return false;
      }

      if (!File.Exists(this.BedFile))
      {
        ParsingErrors.Add(string.Format("Bed file not exists {0}.", this.BedFile));
        return false;
      }

      return true;
    }
  }
}
