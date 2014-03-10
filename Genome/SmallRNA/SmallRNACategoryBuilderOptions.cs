using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CQS.Commandline;
using CommandLine;
using System.IO;

namespace CQS.Genome.SmallRNA
{
  public class SmallRNACategoryBuilderOptions : AbstractOptions
  {
    public static IList<string> DEFAULT_Categories = new List<string>(new[] { "miRNA", "tRNA", "lincRNA", "snoRNA", "snRNA", "rRNA", "misc_RNA" });

    private const bool DEFAULT_PdfGraph = false;
    public SmallRNACategoryBuilderOptions()
    {
      this.Categories = DEFAULT_Categories;
      this.PdfGraph = DEFAULT_PdfGraph;
    }

    [OptionList('c', "category", Required=false, MetaValue = "STRING", HelpText = "Priority based categories")]
    public IList<string> Categories { get; set; }

    [Option('m', "miRNA", Required = false, MetaValue = "FILE", HelpText = "miRNA mapped xml file")]
    public string MiRNAFile { get; set; }

    [Option('s', "smallRNA", Required = true, MetaValue = "FILE", HelpText = "Other smallRNA mapped xml file")]
    public string OtherFile { get; set; }

    [Option('o', "outputFile", Required = true, MetaValue = "FILE", HelpText = "Output category file")]
    public string OutputFile { get; set; }

    [Option("pdf", DefaultValue = DEFAULT_PdfGraph, MetaValue = "BOOL", HelpText = "Output graph as pdf format(default is png format)")]
    public bool PdfGraph { get; set; }

    public override bool PrepareOptions()
    {
      if (!string.IsNullOrEmpty(this.MiRNAFile) && !File.Exists(this.MiRNAFile))
      {
        ParsingErrors.Add(string.Format("miRNA mapped xml file not exists {0}.", this.MiRNAFile));
        return false;
      }

      if (!File.Exists(this.OtherFile))
      {
        ParsingErrors.Add(string.Format("Other smallRNA mapped xml file not exists {0}.", this.OtherFile));
        return false;
      }

      if (null == this.Categories)
      {
        this.Categories = DEFAULT_Categories;
      }

      return true;
    }
  }
}
