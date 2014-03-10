using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CQS.Commandline;
using CommandLine;
using System.IO;

namespace CQS.Genome.Mirna
{
  public class MirnaCountTableBuilderOptions : SimpleDataTableBuilderOptions
  {
    public MirnaCountTableBuilderOptions()
    { }

    public string IsomirFile
    {
      get
      {
        return Path.ChangeExtension(this.OutputFile, ".isomir.tsv");
      }
    }
  }
}
