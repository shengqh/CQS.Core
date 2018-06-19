using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RCPA.Commandline;
using CommandLine;
using System.IO;

namespace CQS.Genome.SmallRNA
{
  public class SmallRNACountTableBuilderOptions : SimpleDataTableBuilderOptions, ISmallRNAExport
  {
    [Option("noCategory", HelpText = "No category specified")]
    public bool NoCategory { get; set; }

    [Option("exportYRNA", HelpText = "Export yRNA individually")]
    public bool ExportYRNA { get; set; }

    [Option("exportSnRNA", HelpText = "Export snRNA individually")]
    public bool ExportSnRNA { get; set; }

    [Option("exportSnoRNA", HelpText = "Export snoRNA individually")]
    public bool ExportSnoRNA { get; set; }

    public SmallRNACountTableBuilderOptions()
    { }

    public string IsomirFile
    {
      get
      {
        return Path.ChangeExtension(this.OutputFile, ".isomiR.count");
      }
    }

    public string NTAFile
    {
      get
      {
        return Path.ChangeExtension(this.OutputFile, ".NTA.count");
      }
    }

    public string IsomirNTAFile
    {
      get
      {
        return Path.ChangeExtension(this.OutputFile, ".isomiR_NTA.count");
      }
    }
  }
}
