using CommandLine;
using System.IO;

namespace CQS.Genome.SomaticMutation
{
  public abstract class AbstractPileupFilterProcessorOptions : PileupProcessorOptions
  {
    [Option("error_rate", MetaValue = "DOUBLE", DefaultValue = FilterProcessorOptions.DEFAULT_ErrorRate, HelpText = "Sequencing error rate for normal sample test")]
    public double ErrorRate { get; set; }

    [Option("glm_pvalue", MetaValue = "DOUBLE", DefaultValue = FilterProcessorOptions.DEFAULT_GlmPvalue, HelpText = "Maximum pvalue used for GLM test")]
    public double GlmPvalue { get; set; }

    [Option("glm_use_raw_pvalue", HelpText = "Use GLM raw pvalue rather than FDR adjusted pvalue")]
    public bool GlmFilterByRawPvalue { get; set; }

    [Option("glm_ignore_score_diff", DefaultValue = false, HelpText = "Ignore score difference in GLM model")]
    public bool GlmIgnoreScoreDifference { get; set; }

    [Option("glm_min_median_score_diff", MetaValue = "DOUBLE", DefaultValue = FilterProcessorOptions.DEFAULT_GlmPvalue, HelpText = "Minimum median score differience between minor alleles and major alleles")]
    public double GlmMinimumMedianScoreDiff { get; set; }

    [Option("zero_minor_allele_strategy_glm_pvalue", DefaultValue = FilterProcessorOptions.DEFAULT_ZeroMinorAlleleStrategyGlmPvalue, HelpText = "Maximum GLM pvalue for the candidate with zero minor allele in normal sample")]
    public double ZeroMinorAlleleStrategyGlmPvalue { get; set; }

    public AbstractPileupFilterProcessorOptions()
    {
      this.ErrorRate = FilterProcessorOptions.DEFAULT_ErrorRate;
      this.GlmPvalue = FilterProcessorOptions.DEFAULT_GlmPvalue;
      this.GlmFilterByRawPvalue = false;
      this.GlmIgnoreScoreDifference = false;
      this.GlmMinimumMedianScoreDiff = FilterProcessorOptions.DEFAULT_GlmMinimumMedianScoreDiff;
      this.ZeroMinorAlleleStrategyGlmPvalue = FilterProcessorOptions.DEFAULT_ZeroMinorAlleleStrategyGlmPvalue;
    }

    public override void PrintParameter(TextWriter tw)
    {
      base.PrintParameter(tw);
      tw.WriteLine("#error_rate={0}", this.ErrorRate);
      tw.WriteLine("#glm_pvalue={0}", this.GlmPvalue);
      tw.WriteLine("#glm_use_raw_pvalue={0}", this.GlmFilterByRawPvalue);
      tw.WriteLine("#glm_ignore_score_diff={0}", this.GlmIgnoreScoreDifference);
      tw.WriteLine("#glm_min_median_score_diff={0}", this.GlmMinimumMedianScoreDiff);
      tw.WriteLine("#zero_minor_allele_strategy_glm_pvalue={0}", this.ZeroMinorAlleleStrategyGlmPvalue);
    }

    public abstract FilterProcessorOptions GetFilterOptions();
  }
}