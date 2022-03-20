using CommandLine;
using RCPA.Commandline;
using System.Collections.Generic;
using System.IO;

namespace CQS.Genome.Gwas
{
  public class Impute2ResultDistillerOptions : AbstractOptions
  {
    [OptionList('i', "inputFiles", Required = true, Separator = ',', MetaValue = "FILE LIST", HelpText = "Input impute2 files, separate by ','")]
    public IList<string> InputFiles { get; set; }

    [Option('t', "targetSnpFile", Required = true, MetaValue = "FILE", HelpText = "Target snp file")]
    public string TargetSnpFile { get; set; }

    [Option('o', "outputFile", Required = false, MetaValue = "FILE", HelpText = "Output impute2 filtered file")]
    public string OutputFile { get; set; }

    public override bool PrepareOptions()
    {
      foreach (var file in this.InputFiles)
      {
        if (!File.Exists(file))
        {
          ParsingErrors.Add(string.Format("Input file not exists {0}.", file));
        }
      }

      if (!File.Exists(this.TargetSnpFile))
      {
        ParsingErrors.Add(string.Format("Target SNP file not exists {0}.", this.TargetSnpFile));
      }

      return ParsingErrors.Count == 0;
    }
  }
}
