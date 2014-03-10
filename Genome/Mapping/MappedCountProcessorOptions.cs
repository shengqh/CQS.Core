using CommandLine;

namespace CQS.Genome.Mapping
{
  public class MappedCountProcessorOptions : CountProcessorOptions
  {
    private const string DefaultGtfKey = "";
    private const double DefaultMinimumOverlapPercentage = 0.0;
    private const bool DefaultExportLengthDistribution = false;
    private const bool DefaultExportSequenceCount = false;

    public MappedCountProcessorOptions()
    {
      MinimumOverlapPercentage = DefaultMinimumOverlapPercentage;
      GTFKey = DefaultGtfKey;
    }

    [Option("min_overlap", DefaultValue = DefaultMinimumOverlapPercentage,
      HelpText = "Minimum overlap percentage between region and read (0.0 indicates at least 1 base overlap)")]
    public double MinimumOverlapPercentage { get; set; }

    [Option("gtf_key", DefaultValue = DefaultGtfKey, HelpText = "GTF feature name (such as miRNA)")]
    public string GTFKey { get; set; }

    [Option("length", DefaultValue = DefaultExportLengthDistribution,
      HelpText = "Export read length distribution for each subject")]
    public bool ExportLengthDistribution { get; set; }

    [Option("sequence", DefaultValue = DefaultExportSequenceCount,
      HelpText = "Export sequence and count for for each subject")]
    public bool ExportSequenceCount { get; set; }
  }
}