using CommandLine;
using RCPA.Commandline;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CQS.Genome.Parclip
{
  public interface ITargetBuilderOptions
  {
    string OutputFile { get; set; }
    string RefgeneFile { get; set; }
    string TargetFile { get; set; }
    double MinimumCoverage { get; set; }
    string GenomeFastaFile { get; set; }
    int MinimumSeedLength { get; set; }
    int SeedOffset { get; set; }
  }
}
