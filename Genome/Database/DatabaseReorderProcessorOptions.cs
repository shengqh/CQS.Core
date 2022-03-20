using CommandLine;
using RCPA.Commandline;
using System.IO;

namespace CQS.Genome.Database
{
  public class DatabaseReorderProcessorOptions : AbstractOptions
  {
    public DatabaseReorderProcessorOptions() { }

    [Option('i', "inputFile", Required = true, MetaValue = "FILE", HelpText = "Fasta file (fasta format)")]
    public string InputFile { get; set; }

    [Option('o', "outputFile", Required = true, MetaValue = "FILE", HelpText = "Output file")]
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
