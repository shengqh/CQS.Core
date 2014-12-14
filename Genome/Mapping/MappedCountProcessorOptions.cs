using CommandLine;
using CQS.Genome.Gtf;
using System.Collections.Generic;
using System.Linq;
using System;
using RCPA;
using RCPA.Seq;
using RCPA.Utils;
using CQS.Genome.Feature;

namespace CQS.Genome.Mapping
{
  public class MappedCountProcessorOptions : CountProcessorOptions
  {
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
    }

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

    public override ICandidateBuilder GetCandidateBuilder()
    {
      if (NotSmallRNA)
      {
        Console.WriteLine("Not small RNA, using SAMAlignedItemCandidateFilterBuilder ...");
        var regions = GetSequenceRegions().ToGroupDictionary(m => m.Seqname.StringAfter("chr"));
        return new SAMAlignedItemCandidateFilterBuilder(this, new SAMAlignedItemParsingMRNAFilter(regions, this.MinimumOverlapPercentage, true));
      }

      if (this.IgnoreScore)
      {
        return new SAMAlignedItemCandidateBuilder(this);
      }

      return new SAMAlignedItemBestScoreCandidateBuilder(this);
    }
  }
}