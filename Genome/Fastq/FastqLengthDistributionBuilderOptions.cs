using CommandLine;
using RCPA.Commandline;
using System.Collections.Generic;
using System.IO;

namespace CQS.Genome.Fastq
{
  public class FastqLengthDistributionBuilderOptions : AbstractOptions
  {
    public FastqLengthDistributionBuilderOptions()
    {
      this.CheckCCA = false;
    }
    [OptionList('i', "inputFiles", Required = true, MetaValue = "FILES", Separator = ',', HelpText = "Input fastq files, separated by ','")]
    public IList<string> InputFiles { get; set; }

    [Option('c', "checkCCA", Required = false, MetaValue = "STRING", HelpText = "Check CCA and CCAA on 3' terminal")]
    public bool CheckCCA { get; set; }

    [Option('o', "outputFile", Required = false, MetaValue = "FILE", HelpText = "Output file")]
    public string OutputFile { get; set; }

    public override bool PrepareOptions()
    {
      foreach (var file in InputFiles)
      {
        if (!File.Exists(file))
        {
          ParsingErrors.Add(string.Format("Fastq file not exists {0}.", file));
          return false;
        }
      }

      if (string.IsNullOrEmpty(this.OutputFile))
      {
        this.OutputFile = InputFiles[0] + ".len";
      }

      return true;
    }
  }
}
