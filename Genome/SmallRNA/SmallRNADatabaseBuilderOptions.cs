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

    [Option('e', "ensembl", Required = false, MetaValue = "FILE", HelpText = "Input Ensembl GTF file")]
    public string EnsemblGtfFile { get; set; }

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

        Console.WriteLine("param:mirna=" + op.MiRBaseFile);
        Console.WriteLine("param:mirbase_key=" + op.MiRBaseKey);
        Console.WriteLine("param:ensembl=" + op.EnsemblGtfFile);
        Console.WriteLine("param:trna=" + op.UcscTrnaFile);
        Console.WriteLine("param:genome=" + op.FastaFile);
        Console.WriteLine("param:output=" + op.OutputFile);

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

      if (!string.IsNullOrEmpty(this.EnsemblGtfFile) && !File.Exists(this.EnsemblGtfFile))
      {
        ParsingErrors.Add(string.Format("Input ensembl GTF file not exists {0}.", this.EnsemblGtfFile));
      }

      if (!File.Exists(this.FastaFile))
      {
        ParsingErrors.Add(string.Format("Input genome sequence file not exists {0}.", this.FastaFile));
      }

      return ParsingErrors.Count == 0;
    }

    public void Save(XElement parentNode)
    {
      parentNode.Add(
        new XElement("param", new XAttribute("name", "miRNAFile"), new XAttribute("value", MiRBaseFile)),
        new XElement("param", new XAttribute("name", "miRNAKey"), new XAttribute("value", MiRBaseKey)),
        new XElement("param", new XAttribute("name", "tRNAFile"), new XAttribute("value", UcscTrnaFile)),
        new XElement("param", new XAttribute("name", "ensemblFile"), new XAttribute("value", EnsemblGtfFile)),
        new XElement("param", new XAttribute("name", "fastaFile"), new XAttribute("value", FastaFile)),
        new XElement("param", new XAttribute("name", "outputFile"), new XAttribute("value",OutputFile))
      );
    }

    private string ParseParameter(XElement parentNode, string name)
    {
      var ele = parentNode.FindFirstDescendant("param", "name", name);
      return ele.Attribute("value").Value;
    }

    public void Load(XElement parentNode)
    {
      MiRBaseFile = ParseParameter(parentNode, "miRNAFile");
      MiRBaseKey = ParseParameter(parentNode, "miRNAKey");
      UcscTrnaFile = ParseParameter(parentNode, "tRNAFile");
      EnsemblGtfFile = ParseParameter(parentNode, "ensemblFile");
      FastaFile = ParseParameter(parentNode, "fastaFile");
      OutputFile = ParseParameter(parentNode, "outputFile");
    }
  }
}
