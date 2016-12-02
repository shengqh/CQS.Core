using CommandLine;
using CQS.Genome.SmallRNA;

namespace CQS.Genome.Mapping
{
  public class ChromosomeCountTableBuilderOptions : SimpleDataTableBuilderOptions
  {
    public ChromosomeCountTableBuilderOptions()
    {
      this.MinimumOverlapRate = SmallRNASequenceCountTableBuilderOptions.DEFAULT_MinimumOverlapRate;
      this.MaximumExtensionBase = SmallRNASequenceCountTableBuilderOptions.DEFAULT_MaximumExtensionBase;
    }

    [Option("categoryMapFile", Required = false, MetaValue = "FILE", HelpText = "Category mapping file, each line contains Id and Species")]
    public string CategoryMapFile { get; set; }

    [Option("outputReadTable", Required = false, MetaValue = "BOOELAN", HelpText = "Output unique read count table")]
    public bool OutputReadTable { get; set; }

    [Option("outputReadContigTable", Required = false, MetaValue = "BOOELAN", HelpText = "Output read contig count table")]
    public bool OutputReadContigTable { get; set; }

    [Option("minOverlap", DefaultValue = SmallRNASequenceCountTableBuilderOptions.DEFAULT_MinimumOverlapRate, MetaValue = "DOUBLE", HelpText = "Minimum overlap percentage to merge two reads")]
    public double MinimumOverlapRate { get; set; }

    [Option("maxExtensionBase", DefaultValue = SmallRNASequenceCountTableBuilderOptions.DEFAULT_MaximumExtensionBase, MetaValue = "INT", HelpText = "Maximum number of base extension each iteration for merge two reads. (0 means no limitation)")]
    public int MaximumExtensionBase { get; set; }

    public override bool PrepareOptions()
    {
      base.PrepareOptions();

      if (!string.IsNullOrEmpty(CategoryMapFile))
      {
        CheckFile("categoryMapFile", CategoryMapFile);
      }

      return ParsingErrors.Count == 0;
    }
  }
}
