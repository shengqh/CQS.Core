using CommandLine;
using RCPA.Commandline;
using System.IO;

namespace CQS.Genome.Bacteria
{
  public class GffToBedConverterOptions : AbstractOptions
  {
    [Option('i', "inputFile", Required = true, MetaValue = "FILE", HelpText = "Input GFF file")]
    public string InputFile { get; set; }

    [Option('o', "outputFile", Required = true, MetaValue = "FILE", HelpText = "output bed file")]
    public string OutputFile { get; set; }

    public override bool PrepareOptions()
    {
      if (!File.Exists(this.InputFile))
      {
        ParsingErrors.Add(string.Format("File not exists {0}.", this.InputFile));
      }

      return ParsingErrors.Count == 0;
    }
  }
}
