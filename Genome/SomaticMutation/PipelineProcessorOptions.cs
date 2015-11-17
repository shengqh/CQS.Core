using System.IO;
using CommandLine;
using RCPA.Utils;

namespace CQS.Genome.SomaticMutation
{
  public class PipelineProcessorOptions : PileupProcessorOptions
  {
    [Option("error_rate", MetaValue = "DOUBLE", DefaultValue = FilterProcessorOptions.DEFAULT_ErrorRate, HelpText = "Sequencing error rate for normal sample test")]
    public double ErrorRate { get; set; }

    [Option("glm_pvalue", MetaValue = "DOUBLE", DefaultValue = PileupProcessorOptions.DEFAULT_FisherPvalue, HelpText = "Maximum adjusted pvalue used for GLM test")]
    public double GlmPvalue { get; set; }

    [Option("glm_use_raw_pvalue", MetaValue = "BOOLEAN", HelpText = "Use GLM raw pvalue rather than FDR adjusted pvalue")]
    public bool UseGlmRawPvalue { get; set; }

    public string AnnovarCommand { get; private set; }

    [Option("annovar_set_default", DefaultValue = false, HelpText = "Set current setting as annovar default setting")]
    public bool AnnovarSetDefault { get; set; }

    [Option("annovar_buildver", MetaValue = "STRING", HelpText = "Annovar database version, for example: hg19)")]
    public string AnnovarBuildVersion { get; set; }

    [Option("annovar_db", MetaValue = "DIRECTORY", HelpText = "The directory contains annovar databases")]
    public string AnnovarDatabaseDirectory { get; set; }

    [Option("annovar_protocol", MetaValue = "STRING", HelpText = "Annovar protocols, for example: refGene,snp138,cosmic68")]
    public string AnnovarProtocol { get; set; }

    [Option("annovar_operation", MetaValue = "STRING", HelpText = "Annovar operation, must match with annovar protocols, for example: g,f,f")]
    public string AnnovarOperation { get; set; }

    [Option("distance_insertion_bed", MetaValue = "FILE", HelpText = "Insertion bed file for distance annotation.")]
    public string DistanceInsertionBed { get; set; }

    [Option("distance_deletion_bed", MetaValue = "FILE", HelpText = "Deletion bed file for distance annotation.")]
    public string DistanceDeletionBed { get; set; }

    [Option("distance_junction_bed", MetaValue = "FILE", HelpText = "Junction bed file for distance annotation.")]
    public string DistanceJunctionBed { get; set; }

    [Option("distance_exon_gtf", MetaValue = "FILE", HelpText = "Exon gtf file for distance annotation.")]
    public string GtfFile { get; set; }

    [Option("rnaediting_db", MetaValue = "FILE", HelpText = "The rna editing database file")]
    public string RnaeditingDatabase { get; set; }

    public FilterProcessorOptions GetFilterOptions()
    {
      var result = new FilterProcessorOptions();

      result.Config = Config;
      result.IsPileup = true;
      result.InputFile = BaseFilename;
      result.OutputFile = FilterResultFile;
      result.GlmPvalue = GlmPvalue;
      result.ErrorRate = ErrorRate;
      result.UseGlmRawPvalue = UseGlmRawPvalue;

      return result;
    }

    private string FilterResultFile { get { return GetLinuxFile(OutputSuffix + ".tsv"); } }

    public AnnotationProcessorOptions GetAnnotationOptions()
    {
      var result = new AnnotationProcessorOptions();

      BeanUtils.CopyPropeties(this, result);
      result.IsPileup = true;
      result.InputFile = FilterResultFile;

      return result;
    }

    public override bool PrepareOptions()
    {
      base.PrepareOptions();

      var filterOption = GetFilterOptions();
      if (!filterOption.PrepareOptions())
      {
        ParsingErrors.AddRange(filterOption.ParsingErrors);
      }

      var annoOption = GetAnnotationOptions();
      if (!annoOption.PrepareOptions())
      {
        ParsingErrors.AddRange(annoOption.ParsingErrors);
      }

      return ParsingErrors.Count == 0;
    }
  }
}