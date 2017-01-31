using CommandLine;
using CQS.Genome.Mapping;
using System.Collections.Generic;

namespace CQS.Genome.SmallRNA
{
  public class SmallRNACountProcessorOptions : AbstractSmallRNACountProcessorOptions, ICountProcessorOptions
  {
    [Option("noCategory", HelpText = "No category in coordindates file, treat all as one category")]
    public bool NoCategory { get; set; }

    [Option("ccaFile", Required = false, MetaValue = "FILE", HelpText = "NTA with 3' CC from original CCA file")]
    public string CCAFile { get; set; }

    [Option("exportYRNA", HelpText = "Export yRNA individually")]
    public bool ExportYRNA { get; set; }

    public SmallRNACountProcessorOptions()
    {
    }
  }
}
