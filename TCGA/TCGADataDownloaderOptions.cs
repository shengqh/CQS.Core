using CommandLine;
using RCPA.Commandline;
using RCPA.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CQS.TCGA
{
  public class TCGADataDownloaderOptions : AbstractOptions
  {
    [Option('o', "outputDir", Required = true, MetaValue = "DIRECTORY", HelpText = "Output directory")]
    public string OutputDirectory { get; set; }

    [Option('x', "xml", Required = true, MetaValue = "FILE", HelpText = "TCGA xml format tree file")]
    public string XmlFile { get; set; }

    [Option('z', "7zip", Required = false, MetaValue = "PROGRAM", HelpText = "7zip location")]
    public string Zip7 { get; set; }

    [OptionList('d', "dataTypes", Required = false, MetaValue = "STRING", Separator = ',', HelpText = "Data types, can be any one or combination of microarray, mirna, mirnaseq, rnaseqv1 and rnaseqv2, separated by ','")]
    public IList<string> DataTypes { get; set; }

    [OptionList('t', "tumorTypes", Required = false, MetaValue = "STRING", Separator = ',', HelpText = "Tumor types, based on the TCGA xml format tree file, separated by ',', such like 'brca,lgg'")]
    public IList<string> TumorTypes { get; set; }

    private List<ITCGATechnology> _technologies = null;
    public List<ITCGATechnology> Technologies
    {
      get
      {
        if (_technologies == null)
        {
          return (from dt in DataTypes
                  select TCGATechnology.Parse(dt)).ToList();
        }
        else
        {
          return _technologies;
        }
      }
      set
      {
        _technologies = value;
      }
    }

    public override bool PrepareOptions()
    {
      if (!Directory.Exists(this.OutputDirectory))
      {
        ParsingErrors.Add(string.Format("Directory not exists {0}.", this.OutputDirectory));
        return false;
      }

      if (!File.Exists(this.XmlFile))
      {
        ParsingErrors.Add(string.Format("File not exists {0}.", this.XmlFile));
        return false;
      }

      if (!SystemUtils.IsLinux)
      {
        if (string.IsNullOrEmpty(this.Zip7))
        {
          ParsingErrors.Add("Define 7zip location first.");
          return false;
        }

        if (!File.Exists(this.Zip7))
        {
          ParsingErrors.Add(string.Format("File not exists {0}.", this.Zip7));
          return false;
        }

        if (DataTypes == null || DataTypes.Count == 0)
        {
          _technologies = TCGATechnology.Technoligies.ToList();
        }
        else
        {
          try
          {
            var tecs = this.Technologies;
          }
          catch (Exception ex)
          {
            ParsingErrors.Add(ex.Message);
            return false;
          }
        }
      }

      return true;
    }
  }
}
