using CommandLine;
using CQS.Genome.Gsnap;
using CQS.Genome.Mapping;
using System.Collections.Generic;

namespace CQS.Genome.SmallRNA
{
  public class SmallRNACountProcessorOptions : AbstractSmallRNACountProcessorOptions, ICountProcessorOptions, ISmallRNAExport
  {
    [Option("noCategory", HelpText = "No category in coordindates file, treat all as one category")]
    public bool NoCategory { get; set; }

    [Option("ccaFile", Required = false, MetaValue = "FILE", HelpText = "NTA with 3' CC from original CCA file")]
    public string CCAFile { get; set; }

    [Option("exportYRNA", HelpText = "Export yRNA individually")]
    public bool ExportYRNA { get; set; }

    [Option("exportSnRNA", HelpText = "Export snRNA individually")]
    public bool ExportSnRNA { get; set; }

    [Option("exportSnoRNA", HelpText = "Export snoRNA individually")]
    public bool ExportSnoRNA { get; set; }

    [Option("newMethod", HelpText = "Using fast method to parse result")]
    public bool NewMethod { get; set; }

    public SmallRNACountProcessorOptions()
    {
    }

    public override ICandidateBuilder GetCandidateBuilder()
    {
      if (this.NewMethod)
      {
        return new SmallRNACandidateBuilder(this);
      }

      return base.GetCandidateBuilder();
    }
  }
}
