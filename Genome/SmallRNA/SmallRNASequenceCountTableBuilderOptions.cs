using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RCPA.Commandline;
using CommandLine;
using System.IO;
using RCPA;
using System.Xml.Linq;

namespace CQS.Genome.SmallRNA
{
  public class SmallRNASequenceCountTableBuilderOptions : SimpleDataTableBuilderOptions
  {
    private const int DEFAULT_TOP_NUMBER = 100;
    private const string DEFAULT_FilePattern = "*.dupcount";

    public SmallRNASequenceCountTableBuilderOptions()
    {
      this.TopNumber = DEFAULT_TOP_NUMBER;
    }

    [Option('n', "number", DefaultValue=DEFAULT_TOP_NUMBER,  MetaValue = "INT", HelpText = "Select top X reads in each file")]
    public int TopNumber { get; set; }

    [Option('f', "filePattern", DefaultValue = DEFAULT_FilePattern, MetaValue = "PATTERN", HelpText = "Pattern of data file")]
    public override string FilePattern { get; set; }
  }
}
