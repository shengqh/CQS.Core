﻿using CommandLine;
using CQS.Commandline;
using RCPA;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CQS.TCGA
{
  public class TCGADatatableBuilderOptions : AbstractOptions
  {
    [Option('i', "tcgaDir", Required = true, MetaValue = "DIRECTORY", HelpText = "TCGA root directory")]
    public string TCGADirectory { get; set; }

    [Option('d', "dataType", Required = true, MetaValue = "STRING", HelpText = "Data type, including microarray, mirna, mirnaseq, rnaseqv1 and rnaseqv2")]
    public string DataType { get; set; }

    [OptionList('t', "tumorTypes", Required = true, MetaValue = "STRINGS", Separator = ',', HelpText = "Tumor types, separated by ',', for example 'brca,lusc'")]
    public IList<string> TumorTypes { get; set; }

    [Option('o', "outputFile", Required = true, MetaValue = "FILE", HelpText = "Output file")]
    public string OutputFile { get; set; }

    [Option('c', "count", DefaultValue = false, HelpText = "Extract count data or normal value")]
    public bool IsCount { get; set; }

    [Option('p', "patient", DefaultValue = false, HelpText = "With clinical information only")]
    public bool WithClinicalInformationOnly { get; set; }

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
