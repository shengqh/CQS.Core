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
using CQS.Genome.Gsnap;
using CQS.Genome.SmallRNA;

namespace CQS.Genome.Mapping
{
  public class AbstractCountProcessorOptions : SAMAlignedItemParserOptions, ICountProcessorOptions
  {
    public AbstractCountProcessorOptions()
    { }

    [Option('o', "outputFile", Required = true, MetaValue = "FILE", HelpText = "Output count file")]
    public string OutputFile { get; set; }

    [Option('c', "countFile", Required = false, MetaValue = "FILE", HelpText = "Sequence/count file")]
    public string CountFile { get; set; }

    public override bool PrepareOptions()
    {
      base.PrepareOptions();

      if (!string.IsNullOrEmpty(this.CountFile) && !File.Exists(this.CountFile))
      {
        ParsingErrors.Add(string.Format("Count file not exists {0}.", this.CountFile));
      }

      return ParsingErrors.Count == 0;
    }

    public virtual ICandidateBuilder GetCandidateBuilder()
    {
      if (this.EngineType == 4)
      {
        return new SAMAlignedItemCandidateGsnapBuilder(this);
      }
      else if (this.IgnoreScore)
      {
        return new SAMAlignedItemCandidateBuilder(this);
      }
      else
      {
        return new SAMAlignedItemBestScoreCandidateBuilder(this);
      }
    }

    private SmallRNACountMap cm;

    public virtual SmallRNACountMap GetCountMap()
    {
      if (cm == null)
      {
        cm = new SmallRNACountMap(this.CountFile);
      }
      return cm;
    }
  }
}
