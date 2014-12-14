using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CQS.Commandline;
using CommandLine;
using System.IO;
using CQS.Genome.Sam;
using CQS.Genome.Gtf;
using RCPA.Seq;
using CQS.Genome.Feature;

namespace CQS.Genome.Mapping
{
  public class CountProcessorOptions : AbstractOptions, ISAMAlignedItemParserOptions
  {
    private static string MIRNA = "ATGCUN";

    private const int DEFAULT_MinimumReadLength = 12;
    private const int DEFAULT_MaximumMismatchCount = 1;
    private const bool DEFAULT_IgnoreScore = false;
    private const int DefaultEngineType = 1;
    private const string DefaultGtfKey = "";

    public CountProcessorOptions()
    {
      this.GtfFeatureName = DefaultGtfKey;
      this.MinimumReadLength = DEFAULT_MinimumReadLength;
      this.MaximumReadLength = int.MaxValue;
      this.MaximumMismatchCount = DEFAULT_MaximumMismatchCount;
      this.IgnoreScore = DEFAULT_IgnoreScore;
      this.EngineType = DefaultEngineType;
    }

    [Option('i', "inputFile", Required = true, MetaValue = "FILE", HelpText = "Alignment sam/bam file")]
    public string InputFile { get; set; }

    [Option('g', "coordinateFile", Required = true, MetaValue = "FILE", HelpText = "Genome annotation coordinate file (can be gff or bed format. But the coordinates in bed format should be same format as gff)")]
    public string CoordinateFile { get; set; }

    [Option("gtf_key", DefaultValue = DefaultGtfKey, HelpText = "GTF feature name (such as miRNA)")]
    public string GtfFeatureName { get; set; }

    [Option('f', "coordinateFastaFile", Required = false, MetaValue = "FILE", HelpText = "Genome annotation coordinate fasta file")]
    public string FastaFile { get; set; }

    [Option('c', "countFile", Required = false, MetaValue = "FILE", HelpText = "Sequence/count file")]
    public string CountFile { get; set; }

    [Option('q', "fastqFile", Required = false, MetaValue = "FILE", HelpText = "Original fastq file")]
    public string FastqFile { get; set; }

    [Option('o', "outputFile", Required = false, MetaValue = "FILE", HelpText = "Output count file")]
    public string OutputFile { get; set; }

    [Option('l', "minlen", MetaValue = "INT", DefaultValue = DEFAULT_MinimumReadLength, HelpText = "Minimum read length")]
    public int MinimumReadLength { get; set; }

    [Option("maxlen", MetaValue = "INT", DefaultValue = int.MaxValue, HelpText = "Maximum read length")]
    public int MaximumReadLength { get; set; }

    [Option('m', "maxMismatch", MetaValue = "INT", DefaultValue = DEFAULT_MaximumMismatchCount, HelpText = "Maximum mismatch count")]
    public int MaximumMismatchCount { get; set; }

    [Option('s', "ignoreScore", DefaultValue = DEFAULT_IgnoreScore, HelpText = "Ignore score difference between matches from same query")]
    public bool IgnoreScore { get; set; }

    [Option('e', "engineType", DefaultValue = DefaultEngineType, MetaValue = "INT", HelpText = "Engine type (1:bowtie1, 2:bowtie2, 3:bwa)")]
    public int EngineType { get; set; }

    [Option("samtools", Required = false, MetaValue = "FILE", HelpText = "Samtools location (required for bam file)")]
    public string Samtools { get; set; }

    [Option("bed_as_gtf", DefaultValue = false, HelpText = "Consider bed coordinate (zero-based) as gtf format (one-based)")]
    public bool BedAsGtf { get; set; }

    [Option("matched_file", DefaultValue = false, HelpText = "Output detail matched information to file")]
    public bool MatchedFile { get; set; }

    [Option("no_mapped_file", DefaultValue = false, HelpText = "Don't output detail mapped information to file")]
    public bool NoMappedFile { get; set; }

    [Option("unmapped_fastq", DefaultValue = false, HelpText = "Output unmapped reads to fastq")]
    public bool UnmappedFastq { get; set; }

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

      if (!File.Exists(this.CoordinateFile))
      {
        ParsingErrors.Add(string.Format("Gff file not exists {0}.", this.CoordinateFile));
        return false;
      }

      if (!string.IsNullOrEmpty(this.FastaFile) && !File.Exists(this.FastaFile))
      {
        ParsingErrors.Add(string.Format("Fasta file not exists {0}.", this.FastaFile));
        return false;
      }

      if (!string.IsNullOrEmpty(this.CountFile) && !File.Exists(this.CountFile))
      {
        ParsingErrors.Add(string.Format("Count file not exists {0}.", this.CountFile));
        return false;
      }

      if (!string.IsNullOrEmpty(this.FastqFile) && !File.Exists(this.FastqFile))
      {
        ParsingErrors.Add(string.Format("Fastq file not exists {0}.", this.FastqFile));
        return false;
      }

      if (string.IsNullOrEmpty(this.OutputFile))
      {
        this.OutputFile = this.InputFile + ".count";
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
      if (this.IgnoreScore)
      {
        return new SAMAlignedItemCandidateBuilder(this);
      }
      else
      {
        return new SAMAlignedItemBestScoreCandidateBuilder(this);
      }
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

    public virtual List<FeatureLocation> GetSequenceRegions()
    {
      //Read sequence regions
      var result = SequenceRegionUtils.GetSequenceRegions(CoordinateFile, GtfFeatureName, BedAsGtf);
      result.ForEach(m =>
      {
        m.Seqname = m.Seqname.StringAfter("chr");
      });

      //Fill sequence information
      var sr = result.FirstOrDefault(m => m.Name.Contains(":"));
      if (sr != null)
      {
        var sequence = sr.Name.StringAfter(":");
        if (sequence.All(m => MIRNA.Contains(m)))
        {
          result.ForEach(m => m.Sequence = m.Name.StringAfter(":"));
          result.ForEach(m => m.Name = m.Name.StringBefore(":"));
        }
      }

      if (!string.IsNullOrEmpty(this.FastaFile))
      {
        Console.WriteLine("Reading sequence from {0} ...", this.FastaFile);
        var seqs = SequenceUtils.Read(new FastaFormat(), this.FastaFile).ToDictionary(m => m.Name);
        result.ForEach(m =>
        {
          if (seqs.ContainsKey(m.Name))
          {
            m.Sequence = seqs[m.Name].SeqString;
          }
          else
          {
            Console.WriteLine("Missing sequence: " + m.Name);
          }
        });
        seqs.Clear();
      }

      return result.ConvertAll(m => new FeatureLocation(m)).ToList();
    }
  }
}
