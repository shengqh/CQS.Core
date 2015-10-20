using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RCPA.Commandline;
using CommandLine;
using System.IO;

namespace CQS.Genome.Mirna
{
  public class MirnaMappedOverlapBuilderOptions : AbstractOptions
  {
    public MirnaMappedOverlapBuilderOptions()
    { }

    [Option('r', "referenceFile", Required = true, MetaValue = "FILE", HelpText = "Reference mapped file")]
    public string ReferenceFile { get; set; }

    [Option('s', "sampleFile", Required = true, MetaValue = "FILE", HelpText = "Sample mapped file")]
    public string SampleFile { get; set; }

    [Option('o', "outputFile", Required = false, MetaValue = "FILE", HelpText = "Output comparison file")]
    public string OutputFile { get; set; }

    public override bool PrepareOptions()
    {
      if (!File.Exists(this.ReferenceFile))
      {
        ParsingErrors.Add(string.Format("Reference file not exists {0}.", this.ReferenceFile));
        return false;
      }

      if (!File.Exists(this.SampleFile))
      {
        ParsingErrors.Add(string.Format("Sample file not exists {0}.", this.SampleFile));
        return false;
      }

      return true;
    }
  }
}
