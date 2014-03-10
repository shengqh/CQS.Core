using CommandLine;
using CQS.Commandline;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CQS.Genome.Annotation
{
  public class AnnovarGenomeSummaryRefinedResultBuilderOptions : AbstractOptions
  {
    private const bool DefaultPerformFisher = false;
    public AnnovarGenomeSummaryRefinedResultBuilderOptions()
    {
      this.PerformFisher = DefaultPerformFisher;
    }

    [Option('i', "inputFile", Required = true, MetaValue = "FILE", HelpText = "Annovar gene summary report file")]
    public string InputFile { get; set; }

    [Option('a', "affyAnnoationFile", Required = false, MetaValue = "FILE", HelpText = "Affymetrix annotation file used to mapping gene symbol with gene description")]
    public string AffyAnnotationFile { get; set; }

    [Option('o', "outputFile", Required = false, MetaValue = "FILE", HelpText = "Refined result file")]
    public string OutputFile { get; set; }

    //[Option('f', "fisher", DefaultValue = DEFAULT_PerformFisher, MetaValue = "BOOL", HelpText = "Fisher exact test for somatic mutation")]
    public bool PerformFisher { get; set; }

    public override bool PrepareOptions()
    {
      if (!File.Exists(this.InputFile))
      {
        ParsingErrors.Add(string.Format("Input file not exists {0}.", this.InputFile));
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
}
