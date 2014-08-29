using System.Collections.Generic;
using System.Linq;
using CQS.Commandline;
using CommandLine;
using System.IO;
using CQS.Genome.Sam;

namespace CQS.Genome.Mapping
{
  public class ChromosomeCountProcessorOptions : AbstractOptions, ISAMAlignedItemParserOptions
  {
    private const int DEFAULT_EngineType = 1;
    private static IList<long> DEFAULT_Offsets;

    private const int DEFAULT_MinimumReadLength = 12;
    private const int DEFAULT_MaximumMismatchCount = 1;

    static ChromosomeCountProcessorOptions()
    {
      DEFAULT_Offsets = new[] { 1L, 2L, 3L }.ToList();
    }

    public ChromosomeCountProcessorOptions()
    {
      EngineType = DEFAULT_EngineType;
      MinimumReadLength = DEFAULT_MinimumReadLength;
      MaximumReadLength = int.MaxValue;
      MaximumMismatchCount = DEFAULT_MaximumMismatchCount;
      Offsets = DEFAULT_Offsets;
    }

    [Option('i', "inputFile", Required = true, MetaValue = "FILE", HelpText = "Alignment sam/bam file")]
    public string InputFile { get; set; }

    [Option('c', "countFile", Required = false, MetaValue = "FILE", HelpText = "Sequence/count file")]
    public string CountFile { get; set; }

    [Option('n', "perferName", Required = false, DefaultValue="hsa", MetaValue = "String", HelpText = "The prefer name that kept for multiple mapped reads")]
    public string PerferPrefix { get; set; }

    [Option('p', "perfectMappedNameFile", Required = false, MetaValue = "FILE", HelpText = "The name of reads that perfect mapped to genome")]
    public string PerfectMappedNameFile { get; set; }

    [Option('o', "outputFile", Required = false, MetaValue = "FILE", HelpText = "Output count file")]
    public string OutputFile { get; set; }

    [OptionList('f', "offsets", MetaValue = "INT", Separator = ',', HelpText = "Valid offsets (0 means any offsets and 1,2,3 for miRNA, default is 1,2,3)")]
    public IList<long> Offsets { get; set; }

    [Option('e', "engineType", DefaultValue = DEFAULT_EngineType, MetaValue = "INT", HelpText = "Engine type (1:bowtie1, 2:bowtie2, 3:bwa)")]
    public int EngineType { get; set; }

    [Option("samtools", Required = false, MetaValue = "FILE", HelpText = "Samtools location (required for bam file)")]
    public string Samtools { get; set; }

    public override bool PrepareOptions()
    {
      if (!File.Exists(this.InputFile))
      {
        ParsingErrors.Add(string.Format("Input file not exists {0}.", this.InputFile));
        return false;
      }

      if (this.InputFile.ToLower().EndsWith(".bam") && !File.Exists(this.Samtools))
      {
        ParsingErrors.Add(string.Format("Samtools location is not defined or not exists: {0}", this.Samtools));
        return false;
      }

      if (!string.IsNullOrEmpty(this.CountFile) && !File.Exists(this.CountFile))
      {
        ParsingErrors.Add(string.Format("Count file not exists {0}.", this.CountFile));
        return false;
      }

      if (!string.IsNullOrEmpty(this.PerfectMappedNameFile) && !File.Exists(this.PerfectMappedNameFile))
      {
        ParsingErrors.Add(string.Format("Perfect mapped name file not exists {0}.", this.PerfectMappedNameFile));
        return false;
      }

      if (string.IsNullOrEmpty(this.OutputFile))
      {
        this.OutputFile = this.InputFile + ".count";
      }

      if (Offsets == null)
      {
        Offsets = DEFAULT_Offsets;
      }

      return true;
    }

    public virtual ISAMFormat GetSAMFormat()
    {
      switch (this.EngineType)
      {
        case 2: return SAMFormat.Bowtie2;
        case 3: return SAMFormat.Bwa;
        default: return SAMFormat.Bowtie1;
      }
    }

    public virtual ICandidateBuilder GetCandidateBuilder()
    {
      return new SAMAlignedItemCandidateBuilder(this);
    }

    private CountMap cm;

    public virtual CountMap GetCountMap()
    {
      if (cm == null)
      {
        cm = new CountMap(this.CountFile);
      }
      return cm;
    }

    public int MinimumReadLength { get; set; }

    public int MaximumReadLength { get; set; }

    public int MaximumMismatchCount { get; set; }
  }
}
