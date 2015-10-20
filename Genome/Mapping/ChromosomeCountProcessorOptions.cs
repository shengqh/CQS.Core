using System.Collections.Generic;
using System.Linq;
using RCPA.Commandline;
using CommandLine;
using System.IO;
using CQS.Genome.Sam;
using System;

namespace CQS.Genome.Mapping
{
  public class ChromosomeCountProcessorOptions : AbstractCountProcessorOptions
  {
    public static readonly long[] DEFAULT_Offsets = new long[] { 0, 1, 2 };

    public ChromosomeCountProcessorOptions()
    {
      this.Offsets = DEFAULT_Offsets.ToList();
    }

    [Option('i', "inputFile", Required = true, MetaValue = "FILE", HelpText = "Alignment sam/bam file")]
    public string InputFile { get; set; }

    [Option('p', "perferPrefix", Required = false, DefaultValue = "hsa", MetaValue = "String", HelpText = "The prefer prefix of chromosome name (miRNA name) that kept for multiple mapped reads")]
    public string PreferPrefix { get; set; }

    [Option('n', "perfectMappedNameFile", Required = false, MetaValue = "FILE", HelpText = "The name of reads that perfect mapped to genome")]
    public string PerfectMappedNameFile { get; set; }

    [OptionList("offsets", Required = false, Separator = ',', HelpText = "Allowed (prilority ordered) offsets from miRNA locus, default: 0,1,2")]
    public List<long> Offsets { get; set; }

    public override bool PrepareOptions()
    {
      var result = base.PrepareOptions();

      if (!File.Exists(this.InputFile))
      {
        ParsingErrors.Add(string.Format("Input file not exists {0}.", this.InputFile));
      }

      if (!string.IsNullOrEmpty(this.PerfectMappedNameFile) && !File.Exists(this.PerfectMappedNameFile))
      {
        ParsingErrors.Add(string.Format("Perfect mapped name file not exists {0}.", this.PerfectMappedNameFile));
      }

      if (this.Offsets == null || this.Offsets.Count == 0)
      {
        this.Offsets = DEFAULT_Offsets.ToList();
      }

      return result && ParsingErrors.Count == 0;
    }
  }
}
