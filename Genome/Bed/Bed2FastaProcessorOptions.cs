using CommandLine;
using RCPA.Commandline;
using System;
using System.IO;

namespace CQS.Genome.Bed
{
  public class Bed2FastaProcessorOptions : AbstractOptions
  {
    private const bool DEFAULT_KeepChrInName = false;
    public Bed2FastaProcessorOptions()
    {
      this.KeepChrInName = DEFAULT_KeepChrInName;
      this.AcceptName = m => true;
    }

    [Option('i', "inputFile", Required = true, MetaValue = "FILE", HelpText = "Input bed file")]
    public string InputFile { get; set; }

    [Option('f', "genomeFastaFile", Required = true, MetaValue = "FILE", HelpText = "Genome fasta file")]
    public string GenomeFastaFile { get; set; }

    [Option('o', "outputFile", Required = true, MetaValue = "FILE", HelpText = "Output fasta file")]
    public string OutputFile { get; set; }

    [Option("keepChrInName", DefaultValue = DEFAULT_KeepChrInName, HelpText = "Keep \"chr\" at chromosome name")]
    public bool KeepChrInName { get; set; }

    public Func<string, bool> AcceptName { get; set; }

    public override bool PrepareOptions()
    {
      if (!"-".Equals(this.InputFile) && !File.Exists(this.InputFile))
      {
        ParsingErrors.Add(string.Format("Input file not exists {0}.", this.InputFile));
        return false;
      }

      if (!File.Exists(this.GenomeFastaFile))
      {
        ParsingErrors.Add(string.Format("Genome fasta file not exists {0}.", this.GenomeFastaFile));
        return false;
      }

      return true;
    }
  }
}
