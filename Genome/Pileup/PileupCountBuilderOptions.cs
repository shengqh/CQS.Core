using CommandLine;
using CQS.Genome.Sam;
using RCPA.Commandline;
using System.IO;

namespace CQS.Genome.Pileup
{
  public class PileupCountBuilderOptions : AbstractOptions
  {
    private const double DEFAULT_MinimumAlternativeAlleleFrequency = 0.5;
    private const bool DEFAULT_BedAsGtf = false;
    private const double DEFAULT_FisherPValue = 0.01;
    private const int DEFAULT_MinimumReadLength = 12;
    private const int DEFAULT_EngineType = 1;
    private const bool DEFAULT_ExportIgvScript = false;

    public PileupCountBuilderOptions()
    {
      MinimumReadLength = DEFAULT_MinimumReadLength;
      EngineType = DEFAULT_EngineType;
      BedAsGtf = DEFAULT_BedAsGtf;
      MinimumAlternativeAlleleFrequency = DEFAULT_MinimumAlternativeAlleleFrequency;
      FisherPValue = DEFAULT_FisherPValue;
      ExportIgvScript = DEFAULT_ExportIgvScript;
    }

    [Option('i', "inputFile", Required = true, MetaValue = "FILE", HelpText = "Alignment sam/bam file")]
    public string InputFile { get; set; }

    [Option('g', "coordinateFile", Required = true, MetaValue = "FILE", HelpText = "Genome annotation coordinate file (can be gff or bed format. But the coordinates in bed format should be same format as gff)")]
    public string CoordinateFile { get; set; }

    [Option('c', "countFile", Required = false, MetaValue = "FILE", HelpText = "Sequence/count file")]
    public string CountFile { get; set; }

    [Option('o', "outputFile", Required = false, MetaValue = "FILE", HelpText = "Output SNV file (tab delimtered format)")]
    public string OutputFile { get; set; }

    [Option('l', "minlen", MetaValue = "INT", DefaultValue = DEFAULT_MinimumReadLength, HelpText = "Minimum read length")]
    public int MinimumReadLength { get; set; }

    [Option('p', "pvalue", MetaValue = "FLOAT", DefaultValue = DEFAULT_FisherPValue, HelpText = "Maxmium fisher exact test PValue")]
    public double FisherPValue { get; set; }

    [Option('e', "engineType", DefaultValue = DEFAULT_EngineType, MetaValue = "INT", HelpText = "Engine type (1:bowtie1, 2:bowtie2, 3:bwa)")]
    public int EngineType { get; set; }

    [Option("bed_as_gtf", DefaultValue = DEFAULT_BedAsGtf, HelpText = "Consider bed coordinate (zero-based) as gtf format (one-based)")]
    public bool BedAsGtf { get; set; }

    [Option('m', "minimum_alter_allele", DefaultValue = DEFAULT_MinimumAlternativeAlleleFrequency, HelpText = "Minimum alternative allele frequency")]
    public double MinimumAlternativeAlleleFrequency { get; set; }

    [Option("export_igv", DefaultValue = DEFAULT_ExportIgvScript, HelpText = "Export igv script to save read image for each position)")]
    public bool ExportIgvScript { get; set; }

    public override bool PrepareOptions()
    {
      if (!File.Exists(this.InputFile))
      {
        ParsingErrors.Add(string.Format("Input file not exists {0}.", this.InputFile));
        return false;
      }

      if (!File.Exists(this.CoordinateFile))
      {
        ParsingErrors.Add(string.Format("Gff file not exists {0}.", this.CoordinateFile));
        return false;
      }

      if (!string.IsNullOrEmpty(this.CountFile) && !File.Exists(this.CountFile))
      {
        ParsingErrors.Add(string.Format("Count file not exists {0}.", this.CountFile));
        return false;
      }

      if (string.IsNullOrEmpty(this.OutputFile))
      {
        this.OutputFile = this.InputFile + ".vcf";
      }

      return true;
    }

    public ISAMFormat GetSAMFormat()
    {
      switch (this.EngineType)
      {
        case 1: return SAMFormat.Bowtie1;
        case 3: return SAMFormat.Bwa;
        default: return SAMFormat.Bowtie2;
      }
    }
  }
}
