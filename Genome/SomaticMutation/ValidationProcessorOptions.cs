using CommandLine;
using RCPA.Utils;
using System.IO;

namespace CQS.Genome.SomaticMutation
{
  public class ValidationProcessorOptions : AbstractPileupFilterProcessorOptions
  {
    [Option('v', "validation_file", MetaValue = "FILE", Required = false, HelpText = "Bed format file for somatic mutation validation")]
    public string ValidationFile { get; set; }

    public ValidationProcessorOptions()
    {
      this.IgnoreDepthLimitation = true;
    }

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

    public override void PrintParameter(TextWriter tw)
    {
      base.PrintParameter(tw);
      tw.WriteLine("#validation_file={0}", this.ValidationFile);
    }

    public override FilterProcessorOptions GetFilterOptions()
    {
      var result = new FilterProcessorOptions();
      BeanUtils.CopyPropeties(this, result);

      result.Config = Config;
      result.IsPileup = false;
      result.IsValidation = true;
      result.InputFile = BaseFilename;
      result.OutputFile = GetLinuxFile(OutputSuffix + ".r.tsv");

      return result;
    }
  }
}