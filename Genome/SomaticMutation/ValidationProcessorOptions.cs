using System.IO;
using CommandLine;
using RCPA.Utils;
using System;

namespace CQS.Genome.SomaticMutation
{
  public class ValidationProcessorOptions : PileupProcessorOptions
  {
    [Option('v', "validation_file", MetaValue = "FILE", Required = false, HelpText = "Bed format file for somatic mutation validation")]
    public string ValidationFile { get; set; }

    [Option("glm_pvalue", MetaValue = "DOUBLE", DefaultValue = FilterProcessorOptions.DEFAULT_GlmPvalue, HelpText = "Maximum pvalue used for GLM test")]
    public double GlmPvalue { get; set; }

    [Option("error_rate", MetaValue = "DOUBLE", DefaultValue = FilterProcessorOptions.DEFAULT_ErrorRate, HelpText = "Sequencing error rate for normal sample test")]
    public double ErrorRate { get; set; }

    public override bool PrepareOptions()
    {
      base.PrepareOptions();

      if (null == ValidationFile)
      {
        ParsingErrors.Add("Validation file not defined.");
      }
      else if (!File.Exists(ValidationFile))
      {
        ParsingErrors.Add(string.Format("Validation file not exists {0}.", ValidationFile));
      }

      return ParsingErrors.Count == 0;
    }

    public override AbstractPileupProcessor GetProcessor()
    {
      return new ValidationProcessor(this);
    }

    public override void PrintParameter()
    {
      base.PrintParameter();
      Console.Out.WriteLine("#validation file: " + this.ValidationFile);
    }
  }
}