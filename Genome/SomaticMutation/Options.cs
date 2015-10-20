using CommandLine;
using CommandLine.Text;

namespace CQS.Genome.SomaticMutation
{
  public class Options
  {
    [VerbOption("pileup", HelpText = "Initialize candidates from samtools mpileup result.")]
    public PileupProcessorOptions InitVerb { get; set; }

    [VerbOption("filter", HelpText = "Filter candidates by logistic regression model.")]
    public FilterProcessorOptions FilterVerb { get; set; }

    [VerbOption("annotation", HelpText = "Annotate mutation using varies tools.")]
    public AnnotationProcessorOptions AnnotationVerb { get; set; }

    [VerbOption("all", HelpText = "pileup/filter/annotate data")]
    public PipelineProcessorOptions AllVerb { get; set; }

    [VerbOption("validation", HelpText = "Validate somatic mutations in vcf/bed file.")]
    public ValidationProcessorOptions ValidationVerb { get; set; }

    [HelpVerbOption]
    public string GetUsage(string verb)
    {
      return HelpText.AutoBuild(this, verb);
    }
  }
}
