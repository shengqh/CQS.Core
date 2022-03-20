using CommandLine;
using RCPA.Commandline;
using System.IO;

namespace CQS.Genome.Gtf
{
  public class Gtf2BedGeneIdBuilderOptions : AbstractOptions
  {
    [Option('i', "InputFile", Required = true, MetaValue = "FILE", HelpText = "Input gtf file")]
    public string InputFile { get; set; }

    [Option('n', "ByName", MetaValue = "BOOLEAN", HelpText = "Extract by name (default is by gene_id)")]
    public bool ByName { get; set; }

    [Option('o', "OutputPrefix", Required = true, MetaValue = "FILE", HelpText = "output map file")]
    public string OutputFile { get; set; }

    public override bool PrepareOptions()
    {
      if (!File.Exists(this.InputFile))
      {
        ParsingErrors.Add(string.Format("Directory not exists {0}.", this.InputFile));
        return false;
      }

      return true;
    }
  }
}
