using CommandLine;
using CQS.Genome.Feature;
using RCPA;
using RCPA.Seq;
using RCPA.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CQS.Genome.Mapping
{
  public class MappedCountProcessorOptions : AbstractCountProcessorOptions
  {
    private static string MIRNA = "ATGCUN";
    private const string DefaultGtfKey = "";

    private const double DefaultMinimumOverlapPercentage = 0.0;
    private const bool DefaultExportLengthDistribution = false;
    private const bool DefaultExportSequenceCount = false;
    private const bool DefaultNTA = false;
    private const bool DefaultOrientationFree = false;
    private const bool DefaultNotSmallRNA = false;

    public MappedCountProcessorOptions()
    {
      this.MinimumOverlapPercentage = DefaultMinimumOverlapPercentage;
      this.ExportLengthDistribution = DefaultExportLengthDistribution;
      this.ExportSequenceCount = DefaultExportSequenceCount;
      this.NTA = DefaultNTA;
      this.OrientationFree = DefaultOrientationFree;
      this.NotSmallRNA = DefaultNotSmallRNA;
      this.GtfFeatureName = DefaultGtfKey;
    }

    [Option('i', "inputFile", Required = true, MetaValue = "FILE", HelpText = "Alignment sam/bam file")]
    public string InputFile { get; set; }

    [Option('g', "coordinateFile", Required = true, MetaValue = "FILE", HelpText = "Genome annotation coordinate file (can be gff or bed format. But the coordinates in bed format should be same format as gff)")]
    public string CoordinateFile { get; set; }

    [Option("gtf_key", DefaultValue = DefaultGtfKey, HelpText = "GTF feature name (such as miRNA)")]
    public string GtfFeatureName { get; set; }

    [Option('f', "coordinateFastaFile", Required = false, MetaValue = "FILE", HelpText = "Genome annotation coordinate fasta file")]
    public string FastaFile { get; set; }

    [Option('q', "fastqFile", Required = false, MetaValue = "FILE", HelpText = "Original fastq file")]
    public string FastqFile { get; set; }

    [Option("bed_as_gtf", DefaultValue = false, HelpText = "Consider bed coordinate (zero-based) as gtf format (one-based)")]
    public bool BedAsGtf { get; set; }

    [Option("no_mapped_file", DefaultValue = false, HelpText = "Don't output detail mapped information to file")]
    public bool NoMappedFile { get; set; }

    [Option("unmapped_fastq", DefaultValue = false, HelpText = "Output reads to fastq which are not mapped to features and not perfect mapped to genome")]
    public bool UnmappedFastq { get; set; }

    [Option("min_overlap", DefaultValue = DefaultMinimumOverlapPercentage, HelpText = "Minimum overlap percentage between region and read (0.0 indicates at least 1 base overlap)")]
    public double MinimumOverlapPercentage { get; set; }

    [Option("length", DefaultValue = DefaultExportLengthDistribution, HelpText = "Export read length distribution for each subject")]
    public bool ExportLengthDistribution { get; set; }

    [Option("sequence", DefaultValue = DefaultExportSequenceCount, HelpText = "Export sequence and count for for each subject")]
    public bool ExportSequenceCount { get; set; }

    [Option("orientation_free", DefaultValue = DefaultOrientationFree, HelpText = "Same orientation is not required for matching read to coordinate")]
    public bool OrientationFree { get; set; }

    //[Option("nta", DefaultValue = DefaultNTA, HelpText = "Consider miRNA NTA")]
    public bool NTA { get; set; }

    [Option("not_smallrna", DefaultValue = DefaultNotSmallRNA, HelpText = "Not small RNA data, may contain huge reads")]
    public bool NotSmallRNA { get; set; }

    public override bool PrepareOptions()
    {
      var result = base.PrepareOptions();

      if (!File.Exists(this.InputFile))
      {
        ParsingErrors.Add(string.Format("Input file not exists {0}.", this.InputFile));
      }

      if (!File.Exists(this.CoordinateFile))
      {
        ParsingErrors.Add(string.Format("Gff file not exists {0}.", this.CoordinateFile));
      }

      if (!string.IsNullOrEmpty(this.FastaFile) && !File.Exists(this.FastaFile))
      {
        ParsingErrors.Add(string.Format("Fasta file not exists {0}.", this.FastaFile));
      }

      if (!string.IsNullOrEmpty(this.FastqFile) && !File.Exists(this.FastqFile))
      {
        ParsingErrors.Add(string.Format("Fastq file not exists {0}.", this.FastqFile));
      }

      return result && ParsingErrors.Count == 0;
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

    public override ICandidateBuilder GetCandidateBuilder()
    {
      if (NotSmallRNA)
      {
        Console.WriteLine("Not small RNA, using SAMAlignedItemCandidateFilterBuilder ...");
        var regions = GetSequenceRegions().ToGroupDictionary(m => m.Seqname.StringAfter("chr"));
        return new SAMAlignedItemCandidateFilterBuilder(this, new SAMAlignedItemParsingMRNAFilter(regions, this.MinimumOverlapPercentage, true));
      }

      return base.GetCandidateBuilder();
    }
  }
}