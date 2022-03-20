using CommandLine;
using RCPA.Commandline;
using System.IO;

namespace CQS.Genome.Mapping
{
  public class CountToRPKMProcessorOptions : AbstractOptions
  {
    [Option('i', "inputFile", Required = true, MetaValue = "FILE", HelpText = "Input count file")]
    public string InputFile { get; set; }

    [Option('n', "nameCountMapFile", Required = true, MetaValue = "FILE", HelpText = "Name/TotalCount map file")]
    public string NameCountMapFile { get; set; }

    [Option('l', "lengthFile", Required = true, MetaValue = "FILE", HelpText = "GeneId/GeneSymbol/Length file from gtf_buildmap")]
    public string LengthFile { get; set; }

    [Option('o', "outputFile", Required = true, MetaValue = "FILE", HelpText = "Output rpkm file")]
    public string OutputFile { get; set; }

    public override bool PrepareOptions()
    {
      if (!File.Exists(this.InputFile))
      {
        ParsingErrors.Add(string.Format("Input file not exists {0}.", this.InputFile));
      }

      if (!File.Exists(this.NameCountMapFile))
      {
        ParsingErrors.Add(string.Format("Name/TotalCount map file not exists {0}.", this.NameCountMapFile));
      }

      if (!File.Exists(this.LengthFile))
      {
        ParsingErrors.Add(string.Format("Length file not exists {0}.", this.LengthFile));
      }

      return ParsingErrors.Count == 0;
    }
  }
}
