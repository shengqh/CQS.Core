using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RCPA.Commandline;
using CommandLine;
using System.IO;
using RCPA;
using System.Xml.Linq;

namespace CQS.Genome.SmallRNA
{
  public class SmallRNADatabaseBuilderOptions : AbstractOptions, IXml
  {
    private const string DEFAULT_MiRBaseKey = "miRNA";

    public SmallRNADatabaseBuilderOptions()
    {
      this.ParamFile = string.Empty;
      this.MiRBaseFile = string.Empty;
      this.MiRBaseKey = DEFAULT_MiRBaseKey;
      this.UcscTrnaFile = string.Empty;
      this.UcscMatureTrnaFastaFile = string.Empty;
      this.EnsemblGtfFile = string.Empty;
      this.FastaFile = string.Empty;
      this.OutputFile = string.Empty;
    }

    [Option('f', "config", Required = false, MetaValue = "FILE", HelpText = "Input parameter file")]
    public string ParamFile { get; set; }

    [Option('m', "mirbase", Required = false, MetaValue = "FILE", HelpText = "Input miRBase file in gff/bed format")]
    public string MiRBaseFile { get; set; }

    [Option('k', "mirbase_key", Required = false, MetaValue = "STRING", DefaultValue = DEFAULT_MiRBaseKey, HelpText = "Input miRBase category (miRNA or miRNA_primary_transcript)")]
    public string MiRBaseKey { get; set; }

    [Option('t', "trna", Required = false, MetaValue = "FILE", HelpText = "Input tRNA file in bed format")]
    public string UcscTrnaFile { get; set; }

    [Option("mature_trna", Required = false, MetaValue = "FILE", HelpText = "mature tRNA fasta file")]
    public string UcscMatureTrnaFastaFile { get; set; }
    
    [Option('e', "ensembl", Required = false, MetaValue = "FILE", HelpText = "Input Ensembl GTF file")]
    public string EnsemblGtfFile { get; set; }

    [Option('r', "rrna", Required = false, MetaValue = "FILE", HelpText = "rRNA fasta file")]
    public string RRNAFile { get; set; }

    [Option('g', "genome", Required = false, MetaValue = "FILE", HelpText = "Input genome sequence file in fasta format")]
    public string FastaFile { get; set; }

    [Option('o', "output", Required = false, MetaValue = "FILE", HelpText = "Output file")]
    public string OutputFile { get; set; }

    public override bool PrepareOptions()
    {
      if (File.Exists(ParamFile))
      {
        SmallRNADatabaseBuilderOptions op = new SmallRNADatabaseBuilderOptions();
        op.LoadFromFile(ParamFile);

        if (string.IsNullOrEmpty(this.MiRBaseFile))
        {
          this.MiRBaseFile = op.MiRBaseFile;
        }

        if (string.IsNullOrEmpty(this.MiRBaseKey))
        {
          this.MiRBaseKey = op.MiRBaseKey;
        }

        if (string.IsNullOrEmpty(this.UcscTrnaFile))
        {
          this.UcscTrnaFile = op.UcscTrnaFile;
        }

        if (string.IsNullOrEmpty(this.UcscMatureTrnaFastaFile))
        {
          this.UcscMatureTrnaFastaFile = op.UcscMatureTrnaFastaFile;
        }

        if (string.IsNullOrEmpty(this.RRNAFile))
        {
          this.RRNAFile = op.RRNAFile;
        }

        if (string.IsNullOrEmpty(this.EnsemblGtfFile))
        {
          this.EnsemblGtfFile = op.EnsemblGtfFile;
        }

        if (string.IsNullOrEmpty(this.FastaFile))
        {
          this.FastaFile = op.FastaFile;
        }

        if (string.IsNullOrEmpty(this.OutputFile))
        {
          this.OutputFile = op.OutputFile;
        }
      }

      if (!string.IsNullOrEmpty(this.MiRBaseFile) && !File.Exists(this.MiRBaseFile))
      {
        ParsingErrors.Add(string.Format("Input miRBase file not exists {0}.", this.MiRBaseFile));
      }

      if (!string.IsNullOrEmpty(this.UcscTrnaFile) && !File.Exists(this.UcscTrnaFile))
      {
        ParsingErrors.Add(string.Format("Input tRNA file not exists {0}.", this.UcscTrnaFile));
      }

      if (!string.IsNullOrEmpty(this.UcscMatureTrnaFastaFile) && !File.Exists(this.UcscMatureTrnaFastaFile))
      {
        ParsingErrors.Add(string.Format("Input mature tRNA FASTA file not exists {0}.", this.UcscMatureTrnaFastaFile));
      }

      if (!string.IsNullOrEmpty(this.RRNAFile) && !File.Exists(this.RRNAFile))
      {
        ParsingErrors.Add(string.Format("Input rRNA file not exists {0}.", this.RRNAFile));
      }

      if (!string.IsNullOrEmpty(this.EnsemblGtfFile) && !File.Exists(this.EnsemblGtfFile))
      {
        ParsingErrors.Add(string.Format("Input ensembl GTF file not exists {0}.", this.EnsemblGtfFile));
      }

      if (!File.Exists(this.FastaFile))
      {
        ParsingErrors.Add(string.Format("Input genome sequence file not exists {0}.", this.FastaFile));
      }

      Console.WriteLine("param:mirna=" + this.MiRBaseFile);
      Console.WriteLine("param:mirbase_key=" + this.MiRBaseKey);
      Console.WriteLine("param:ensembl=" + this.EnsemblGtfFile);
      Console.WriteLine("param:trna=" + this.UcscTrnaFile);
      Console.WriteLine("param:mature_trna=" + this.UcscMatureTrnaFastaFile);
      Console.WriteLine("param:rrna=" + this.RRNAFile);
      Console.WriteLine("param:genome=" + this.FastaFile);
      Console.WriteLine("param:output=" + this.OutputFile);

      return ParsingErrors.Count == 0;
    }

    public void Save(XElement parentNode)
    {
      Console.WriteLine("Saving parameters ...");
      parentNode.Add(
        new XElement("param", new XAttribute("name", "miRNAFile"), new XAttribute("value", MiRBaseFile)),
        new XElement("param", new XAttribute("name", "miRNAKey"), new XAttribute("value", MiRBaseKey)),
        new XElement("param", new XAttribute("name", "tRNAFile"), new XAttribute("value", UcscTrnaFile)),
        new XElement("param", new XAttribute("name", "matureTRNAFile"), new XAttribute("value", UcscMatureTrnaFastaFile)),
        new XElement("param", new XAttribute("name", "rRNAFile"), new XAttribute("value", RRNAFile)),
        new XElement("param", new XAttribute("name", "ensemblFile"), new XAttribute("value", EnsemblGtfFile)),
        new XElement("param", new XAttribute("name", "fastaFile"), new XAttribute("value", FastaFile)),
        new XElement("param", new XAttribute("name", "outputFile"), new XAttribute("value",OutputFile))
      );
    }

    private string ParseParameter(XElement parentNode, string name, bool canBeEmpty=false)
    {
      var ele = parentNode.FindFirstDescendant("param", "name", name);
      if(ele == null)
      {
        if (canBeEmpty)
        {
          return string.Empty;
        }
        else
        {
          throw new Exception(string.Format("Cannot find {0}", name));
        }
      }
      return ele.Attribute("value").Value;
    }

    public void Load(XElement parentNode)
    {
      MiRBaseFile = ParseParameter(parentNode, "miRNAFile");
      MiRBaseKey = ParseParameter(parentNode, "miRNAKey");
      UcscTrnaFile = ParseParameter(parentNode, "tRNAFile");
      UcscMatureTrnaFastaFile = ParseParameter(parentNode, "matureTRNAFile", true);
      RRNAFile = ParseParameter(parentNode, "rRNAFile", true);
      EnsemblGtfFile = ParseParameter(parentNode, "ensemblFile");
      FastaFile = ParseParameter(parentNode, "fastaFile");
      OutputFile = ParseParameter(parentNode, "outputFile");
    }
  }
}
