using CommandLine;
using RCPA.Commandline;
using System;
using System.IO;

namespace CQS.Genome.SomaticMutation
{
  public class SomaticMutationTableBuilderOptions : AbstractOptions
  {
    public SomaticMutationTableBuilderOptions()
    {
      this.AcceptChromosome = m => true;
    }

    [Option('i', "input", MetaValue = "FILE", Required = true, HelpText = "Input file, a list of glmvc result files")]
    public string InputFile { get; set; }

    [Option('o', "output", MetaValue = "FILE", Required = true, HelpText = "Output file")]
    public string OutputFile { get; set; }

    public Func<string, bool> AcceptChromosome { get; set; }

    public override bool PrepareOptions()
    {
      if (!File.Exists(InputFile))
      {
        ParsingErrors.Add(string.Format("Input file not exists {0}.", InputFile));
      }

      try
      {
        new MapItemReader(0, 1).ReadFromFile(this.InputFile);
      }
      catch (Exception ex)
      {
        ParsingErrors.Add(string.Format("Error reading list file {0} : {1}", this.InputFile, ex.Message));
      }

      return ParsingErrors.Count == 0;
    }
  }
}