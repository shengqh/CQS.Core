using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CQS.Commandline;
using CommandLine;
using System.IO;

namespace CQS.Genome.SmallRNA
{
  public class SmallRNACategoryGroupBuilderOptions : AbstractOptions
  {
    private const bool DEFAULT_PdfGraph = false;
    public static IList<string> DEFAULT_Categories = new List<string>(new[] { "miRNA", "tRNA", "lincRNA", "snoRNA", "snRNA", "rRNA", "misc_RNA" });

    public SmallRNACategoryGroupBuilderOptions()
    {
      this.PdfGraph = DEFAULT_PdfGraph;
      this.Categories = DEFAULT_Categories;
    }

    [OptionList('c', "category", Required = false, MetaValue = "STRING", HelpText = "Priority based categories")]
    public IList<string> Categories { get; set; }

    [Option('i', "input", Required = true, MetaValue = "FILE", HelpText = "Input list file, each line includes [GroupName, SampleName, otherRNAFile, miRNAFile] which seperated by tab character")]
    public string InputFile { get; set; }

    [Option('o', "output", Required = true, MetaValue = "DIRECTORY", HelpText = "Output directory")]
    public string OutputDirectory { get; set; }

    [Option("pdf", DefaultValue = DEFAULT_PdfGraph, MetaValue = "BOOL", HelpText = "Output graph as pdf format(default is png format)")]
    public bool PdfGraph { get; set; }

    public override bool PrepareOptions()
    {
      if (!File.Exists(this.InputFile))
      {
        ParsingErrors.Add(string.Format("Input file not exists {0}.", this.InputFile));
        return false;
      }

      if (!Directory.Exists(this.OutputDirectory))
      {
        ParsingErrors.Add(string.Format("Directory not exists {0}.", this.OutputDirectory));
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
