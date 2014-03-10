using System.IO;
using CommandLine;
using RCPA.Utils;

namespace CQS.Genome.SomaticMutation
{
  public class AllOptions : PileupOptions
  {
    [Option("annovar", HelpText = "Annotation by annovar.")]
    public bool Annovar { get; set; }

    [Option("annovar_set_default", DefaultValue = false, HelpText = "Set current setting as annovar default setting")]
    public bool AnnovarSetDefault { get; set; }

    [Option("annovar_db", MetaValue = "DIRECTORY", HelpText = "The directory contains annovar databases")]
    public string AnnovarDatabaseDirectory { get; set; }

    [Option("annovar_buildver", MetaValue = "STRING", HelpText = "Annovar database version (such as hg19, mm10)")]
    public string AnnovarBuildVersion { get; set; }

    [Option("annovar_verdbsnp", MetaValue = "STRING", HelpText = "dbSNP version to use")]
    public string AnnovarVerdbsnp { get; set; }

    [Option("annovar_ver1000g", MetaValue = "STRING", HelpText = "1000G version")]
    public string AnnovarVer1000g { get; set; }

    [Option("annovar_veresp", MetaValue = "STRING", HelpText = "ESP version")]
    public string AnnovarVeresp { get; set; }

    [Option("annovar_genetype", MetaValue = "STRING", DefaultValue = "refgene",
      HelpText = "gene definition can be refgene , knowngene, ensgene")]
    public string AnnovarGenetype { get; set; }

    [Option("distance", HelpText = "Annotation by distance to entries from assigned bed files.")]
    public bool Distance { get; set; }

    [Option("distance_insertion_bed", MetaValue = "FILE", HelpText = "Insertion bed file for distance calculation.")]
    public string DistanceInsertionBed { get; set; }

    [Option("distance_deletion_bed", MetaValue = "FILE", HelpText = "Deletion bed file for distance calculation.")]
    public string DistanceDeletionBed { get; set; }

    [Option("distance_junction_bed", MetaValue = "FILE", HelpText = "Junction bed file for distance calculation.")]
    public string DistanceJunctionBed { get; set; }

    [Option("rnaediting", HelpText = "Annotation by rna editing database.")]
    public bool Rnaediting { get; set; }

    [Option("rnaediting_db", MetaValue = "FILE", HelpText = "The rna editing database file")]
    public string RnaeditingDatabase { get; set; }

    public FilterOptions GetFilterOptions()
    {
      var result = new FilterOptions
      {
        Config = Config,
        IsPileup = true,
        InputFile = BaseFilename,
        OutputFile = FilterResultFile,
        PValue = PValue
      };

      return result;
    }

    private string FilterResultFile { get { return GetLinuxFile(OutputSuffix + ".tsv"); } }

    public AnnotationOptions GetAnnotationOptions()
    {
      var result = new AnnotationOptions();

      BeanUtils.CopyPropeties(this, result);
      result.IsPileup = true;
      result.InputFile = FilterResultFile;

      return result;
    }

    public override bool PrepareOptions()
    {
      if (!base.PrepareOptions())
      {
        return false;
      }

      var filterOption = GetFilterOptions();
      if (!filterOption.PrepareOptions())
      {
        ParsingErrors.AddRange(filterOption.ParsingErrors);
        return false;
      }

      var annoOption = GetAnnotationOptions();
      if (!annoOption.PrepareOptions())
      {
        ParsingErrors.AddRange(annoOption.ParsingErrors);
        return false;
      }

      return true;
    }
  }
}