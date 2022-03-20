using CommandLine;
using RCPA;
using RCPA.Commandline;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CQS.TCGA
{
  public class TCGADatatableBuilderOptions : AbstractOptions
  {
    /// <summary>
    /// TCGA root directory
    /// </summary>
    [Option('i', "tcgaDir", Required = true, MetaValue = "DIRECTORY", HelpText = "TCGA root directory")]
    public string TCGADirectory { get; set; }

    /// <summary>
    /// Data type, including microarray, mirna, mirnaseq, rnaseqv1 and rnaseqv2
    /// </summary>
    [Option('d', "dataType", Required = true, MetaValue = "STRING", HelpText = "Data type, including microarray, mirna, mirnaseq, rnaseqv1 and rnaseqv2")]
    public string DataType { get; set; }

    /// <summary>
    /// Tumor types, for example 'brca,lusc'
    /// </summary>
    [OptionList('t', "tumorTypes", Required = true, MetaValue = "STRINGS", Separator = ',', HelpText = "Tumor types, separated by ',', for example 'brca,lusc'")]
    public IList<string> TumorTypes { get; set; }

    /// <summary>
    /// Platform types, for example 'illuminahiseq_rnaseqv2,illuminaga_rnaseqv2'
    /// </summary>
    [OptionList('p', "platforms", Required = true, MetaValue = "STRINGS", Separator = ',', HelpText = "Platform types, separated by ',', for example 'illuminahiseq_rnaseqv2,illuminaga_rnaseqv2'")]
    public IList<string> Platforms { get; set; }

    public string PreferPlatform { get; set; }

    /// <summary>
    /// Output file
    /// </summary>
    [Option('o', "outputFile", Required = true, MetaValue = "FILE", HelpText = "Output file")]
    public string OutputFile { get; set; }

    /// <summary>
    /// Extract count data or normal value
    /// </summary>
    [Option('c', "count", DefaultValue = false, HelpText = "Extract count data or normal value")]
    public bool IsCount { get; set; }

    /// <summary>
    /// With clinical information only
    /// </summary>
    [Option("clinical", DefaultValue = false, HelpText = "With clinical information only")]
    public bool WithClinicalInformationOnly { get; set; }

    /// <summary>
    /// TCGA sample types, for example 'TP,NT', TP is Primary solid Tumor, NT is Solid Tissue Normal
    /// </summary>
    [OptionList('s', "sampleCodes", MetaValue = "STRINGS", Separator = ',', HelpText = "TCGA sample types, separated by ',', for example 'TP,NT', TP is Primary solid Tumor, NT is Solid Tissue Normal")]
    public IList<string> TCGASampleCodeStrings { get; set; }

    public IList<TCGASampleCode> GetTCGASampleCodes()
    {
      List<TCGASampleCode> result = new List<TCGASampleCode>();
      foreach (var s in TCGASampleCodeStrings)
      {
        var code = TCGASampleCode.Find(s);
        if (code == null)
        {
          throw new ArgumentException("Cannot find sample code for {0}", s);
        }
        result.Add(code);
      }
      return result;
    }

    public ITCGATechnology GetTechnology()
    {
      return TCGATechnology.Parse(this.DataType);
    }

    public override bool PrepareOptions()
    {
      if (!Directory.Exists(this.TCGADirectory))
      {
        ParsingErrors.Add(string.Format("Directory not exists {0}.", this.TCGADirectory));
        return false;
      }

      try
      {
        TCGATechnology.Parse(this.DataType);
      }
      catch (Exception ex)
      {
        ParsingErrors.Add(ex.Message);
        return false;
      }

      foreach (var tumor in this.TumorTypes)
      {
        var tumordir = this.TCGADirectory + "/" + tumor;
        if (!Directory.Exists(tumordir))
        {
          ParsingErrors.Add(string.Format("Directory not exists {0}.", tumordir));
          return false;
        }
      }

      if (TCGASampleCodeStrings == null || TCGASampleCodeStrings.Count == 0)
      {
        TCGASampleCodeStrings = (from v in TCGASampleCode.GetSampleCodes()
                                 select v.ShortLetterCode).ToList();
      }
      else
      {
        try
        {
          GetTCGASampleCodes();
        }
        catch (Exception ex)
        {
          ParsingErrors.Add(ex.Message);
          return false;
        }
      }

      try
      {
        GetTechnology();
      }
      catch (Exception ex)
      {
        ParsingErrors.Add(ex.Message);
        return false;
      }

      if (this.Platforms == null || this.Platforms.Count == 0)
      {
        var tec = GetTechnology();
        this.Platforms = (from tumor in TumorTypes
                          let dir = Path.Combine(this.TCGADirectory, tumor)
                          let tecdir = tec.GetTechnologyDirectory(dir)
                          from subdir in Directory.GetDirectories(tecdir)
                          select Path.GetFileName(subdir)).Distinct().OrderBy(m => m).ToList();
      }

      return true;
    }

    public string DesignFile
    {
      get
      {
        return FileUtils.ChangeExtension(this.OutputFile, ".design.tsv");
      }
    }
  }
}
