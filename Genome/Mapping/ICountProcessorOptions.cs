using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RCPA.Commandline;
using CommandLine;
using System.IO;
using CQS.Genome.Sam;
using CQS.Genome.Gtf;
using RCPA.Seq;
using CQS.Genome.Feature;
using CQS.Genome.SmallRNA;

namespace CQS.Genome.Mapping
{
  public interface ICountProcessorOptions : ISAMAlignedItemParserOptions
  {
    string OutputFile { get; set; }

    string CountFile { get; set; }

    SmallRNACountMap GetCountMap();

    ICandidateBuilder GetCandidateBuilder();
  }
}
