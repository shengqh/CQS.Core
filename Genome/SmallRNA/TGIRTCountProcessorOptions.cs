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
using CQS.Genome.Mapping;
using CQS.Genome.Gsnap;

namespace CQS.Genome.SmallRNA
{
  public class TGIRTCountProcessorOptions : AbstractSmallRNACountProcessorOptions
  {
    private const int DEFAULT_EngineType = 5;
    private const int DEFAULT_MaximumLengthOfShortReads = 40;
    private const int DEFAULT_MaximumMismatchForShortRead = 2;// for short reads
    private const int DEFAULT_MaximumMismatch = 4;//for long reads

    [OptionList('i', "inputFile", Required = true, MetaValue = "FILE", Separator = ',', HelpText = "tRNA alignment sam/bam files")]
    public override IList<string> InputFiles { get; set; }

    [Option("other", Required = true, MetaValue = "FILE", HelpText = "other smallRNA alignment sam/bam files")]
    public string OtherFile { get; set; }

    [Option('e', "engineType", DefaultValue = DEFAULT_EngineType, MetaValue = "INT", HelpText = "Engine type (1:bowtie1, 2:bowtie2, 3:bwa, 4:gsnap, 5:star)")]
    public override int EngineType { get; set; }

    [Option("maxLengthOfShortRead", DefaultValue = DEFAULT_MaximumLengthOfShortReads, MetaValue = "INT", HelpText = "Maximum length of short read")]
    public int MaximumLengthOfShortRead { get; set; }

    [Option('m', "maxMismatch", MetaValue = "INT", DefaultValue = DEFAULT_MaximumMismatch, HelpText = "Maximum number of mismatch for long reads")]
    public override int MaximumMismatch { get; set; }

    [Option("maxMismatchForShortRead", DefaultValue = DEFAULT_MaximumMismatchForShortRead, MetaValue = "INT", HelpText = "Maximum number of mismatch for short read")]
    public int MaximumMismatchForShortRead { get; set; }

    public TGIRTCountProcessorOptions()
    {
      this.EngineType = DEFAULT_EngineType;
      this.MaximumMismatch = DEFAULT_MaximumMismatch;
      this.MaximumMismatchForShortRead = DEFAULT_MaximumMismatchForShortRead;
    }

    public override bool PrepareOptions()
    {
      var result = base.PrepareOptions();

      if (!File.Exists(OtherFile))
      {
        ParsingErrors.Add(string.Format("File not exists : {0}", OtherFile));
      }

      return result && ParsingErrors.Count == 0;
    }
  }
}
