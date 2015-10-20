using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RCPA.Commandline;
using CommandLine;
using System.IO;
using CQS.Genome.Sam;

namespace CQS.Genome.Pileup
{
  public class AlleleCountBuilderOptions : AbstractOptions
  {
    private const int DefaultMinimumReadQuality = 20;
    private const int DefaultMinimumBaseQuality = 20;

    public AlleleCountBuilderOptions()
    {
      MinimumReadQuality = DefaultMinimumReadQuality;
      MinimumBaseQuality = DefaultMinimumBaseQuality;
    }

    [Option('i', "inputFile", Required = true, MetaValue = "FILE", HelpText = "Coordinate file (vcf format)")]
    public string InputFile { get; set; }

    [Option('b', "listFile", Required = true, MetaValue = "FILE", HelpText = "File includes bam filename and its unique short name, tab delimtered, one file one line")]
    public string ListFile { get; set; }

    [Option('q', "read_quality", MetaValue = "INT", DefaultValue = DefaultMinimumReadQuality, HelpText = "Skip alignments with mapQ smaller than INT ")]
    public int MinimumReadQuality { get; set; }

    [Option('Q', "base_quality", MetaValue = "INT", DefaultValue = DefaultMinimumBaseQuality, HelpText = "Skip bases with baseQ/BAQ smaller than INT")]
    public int MinimumBaseQuality { get; set; }

    [Option('f', "genomeFile", Required = true, MetaValue = "FILE", HelpText = "Genomic sequence file (fasta format)")]
    public string GenomeFastaFile { get; set; }

    [Option('o', "outputFile", Required = false, MetaValue = "FILE", HelpText = "Output file (tab delimtered format)")]
    public string OutputFile { get; set; }

    [Option("samtools", Required = true, MetaValue = "FILE", HelpText = "Samtools location (required for bam file)")]
    public string Samtools { get; set; }

    public override bool PrepareOptions()
    {
      if (!File.Exists(this.InputFile))
      {
        ParsingErrors.Add(string.Format("Input file not exists {0}.", this.InputFile));
        return false;
      }

      if (!File.Exists(this.Samtools))
      {
        ParsingErrors.Add(string.Format("Samtools location is not defined or not exists: {0}", this.Samtools));
        return false;
      }

      if (!File.Exists(this.GenomeFastaFile))
      {
        ParsingErrors.Add(string.Format("Genome sequence file not exists {0}.", this.GenomeFastaFile));
        return false;
      }

      if (string.IsNullOrEmpty(this.OutputFile))
      {
        this.OutputFile = this.InputFile + ".alleles";
      }

      return true;
    }

    public PileupItemParser GetPileupItemParser()
    {
      return new PileupItemParser(0, MinimumBaseQuality, true, true, false);
    }
  }
}
