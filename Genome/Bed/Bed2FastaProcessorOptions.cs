using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommandLine;
using CQS.Commandline;
using System.IO;

namespace CQS.Genome.Bed
{
  public class Bed2FastaProcessorOptions : AbstractOptions
  {
    private const bool DEFAULT_KeepChrInName = false;
    public Bed2FastaProcessorOptions()
    {
      this.KeepChrInName = DEFAULT_KeepChrInName;
    }

    [Option('i', "inputFile", Required = true, MetaValue = "FILE", HelpText = "Input bam file")]
    public string InputFile { get; set; }

    [Option('f', "genomeFastaFile", Required = true, MetaValue = "FILE", HelpText = "Genome fasta file")]
    public string GenomeFastaFile { get; set; }

    [Option('o', "outputFile", Required = true, MetaValue = "FILE", HelpText = "Output fastq file")]
    public string OutputFile { get; set; }

    [Option("keepChrInName", DefaultValue = DEFAULT_KeepChrInName, HelpText = "Keep \"chr\" at chromosome name")]
    public bool KeepChrInName { get; set; }

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
