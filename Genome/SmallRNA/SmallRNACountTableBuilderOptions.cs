using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RCPA.Commandline;
using CommandLine;
using System.IO;

namespace CQS.Genome.SmallRNA
{
  public class SmallRNACountTableBuilderOptions : SimpleDataTableBuilderOptions
  {
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
