using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RCPA.Commandline;
using CommandLine;
using System.IO;
using RCPA.Gui.Command;

namespace CQS.Genome.Annotation
{
  public class AnnovarSummaryBamDistillerOptions : AbstractOptions
  {
    [Option('i', "inputFile", Required = true, MetaValue = "FILE", HelpText = "Annovar summary report file")]
    public string AnnovarFile { get; set; }

    [Option('a', "affyAnnoationFile", Required = false, MetaValue = "FILE", HelpText = "Affymetrix annotation file used to mapping gene symbol with gene description")]
    public string AffyAnnotationFile { get; set; }

    [Option('b', "bamFile", Required = true, MetaValue = "FILE", HelpText = "Original bam file")]
    public string BamFile { get; set; }

    [Option('t', "targetDir", Required = true, MetaValue = "DIRECTORY", HelpText = "Target directroy used to store bam files")]
    public string TargetDir { get; set; }

    [Option('s', "suffix", Required = true, MetaValue = "STRING", HelpText = "Suffix append to each bam file name")]
    public string Suffix { get; set; }

    public override bool PrepareOptions()
    {
      if (!File.Exists(this.AnnovarFile))
      {
        ParsingErrors.Add(string.Format("Annovar file not exists {0}.", this.AnnovarFile));
        return false;
      }

      if (!File.Exists(this.BamFile))
      {
        ParsingErrors.Add(string.Format("Bam file not exists {0}.", this.BamFile));
        return false;
      }

      if (!Directory.Exists(this.TargetDir))
      {
        ParsingErrors.Add(string.Format("Output directory not exists {0}.", this.TargetDir));
        return false;
      }

      if (!string.IsNullOrEmpty(this.AffyAnnotationFile) && !File.Exists(this.AffyAnnotationFile))
      {
        ParsingErrors.Add(string.Format("Affymetrix annotation file not exists {0}.", this.AffyAnnotationFile));
        return false;
      }

      return true;
    }
  }

  public class AnnovarSummaryBamDistillerCommand : ICommandLineCommand
  {
    public string Name
    {
      get { return "annovar_bam"; }
    }

    public string Description
    {
      get { return "Extract bam file for annovar entries"; }
    }

    public string SoftwareName { get; set; }

    public string SoftwareVersion { get; set; }

    public bool Process(string[] args)
    {
      var options = new AnnovarSummaryBamDistillerOptions();
      bool result = CommandLine.Parser.Default.ParseArguments(args, options);
      if (result)
      {
        if (!options.PrepareOptions())
        {
          Console.Out.WriteLine(options.GetUsage());
          result = false;
        }
        else
        {
          var files = new AnnovarSummaryBamDistiller(options.AffyAnnotationFile, options.BamFile, options.TargetDir, options.Suffix).Process(options.AnnovarFile);
          Console.WriteLine("Run shell file to extract bam files:\n" + files.Merge("\n"));
        }
      }

      return result;
    }
  }
}
