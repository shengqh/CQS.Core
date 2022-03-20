using CommandLine;
using RCPA.Commandline;
using System.IO;

namespace CQS.Genome.Vcf
{
  public class VcfGenotypeTableBuilderOptions : AbstractOptions
  {
    public VcfGenotypeTableBuilderOptions()
    {
      this.RecallGenotype = false;
    }

    [Option('i', "input", Required = true, MetaValue = "FILE", HelpText = "Input cluster file")]
    public string InputFile { get; set; }

    [Option('o', "output", Required = true, MetaValue = "FILE", HelpText = "Output file")]
    public string OutputFile { get; set; }

    [Option('d', "minDepth", Required = true, MetaValue = "INT", HelpText = "Minimum minor allele depth in one of sample")]
    public int MinimumDepth { get; set; }

    [Option('g', "genotype", Required = false, MetaValue = "BOOL", HelpText = "Using fisher-exact test to recall genotype for sites with only 1 ref allele or alt allele")]
    public bool RecallGenotype { get; set; }

    public override bool PrepareOptions()
    {
      if (!File.Exists(this.InputFile))
      {
        ParsingErrors.Add(string.Format("Input file not exists {0}.", this.InputFile));
      }

      return ParsingErrors.Count == 0;
    }
  }
}
