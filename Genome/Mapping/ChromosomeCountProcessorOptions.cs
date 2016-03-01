using CommandLine;
using CQS.Genome.SmallRNA;
using RCPA.Commandline;
using System.IO;

namespace CQS.Genome.Mapping
{
  public class ChromosomeCountProcessorOptions : AbstractOptions
  {
    public ChromosomeCountProcessorOptions()
    {
      this.MergeChromosomesByReads = false;
    }

    [Option('i', "inputFile", Required = true, MetaValue = "FILE", HelpText = "Alignment sam/bam file")]
    public string InputFile { get; set; }

    [Option('p', "perferPrefix", Required = false, DefaultValue = "hsa", MetaValue = "String", HelpText = "The prefer prefix of chromosome name (miRNA name) that kept for multiple mapped reads")]
    public string PreferPrefix { get; set; }

    [Option('o', "outputFile", Required = true, MetaValue = "FILE", HelpText = "Output count file")]
    public string OutputFile { get; set; }

    [Option('c', "countFile", Required = false, MetaValue = "FILE", HelpText = "Sequence/count file")]
    public string CountFile { get; set; }

    [Option('n', "perfectNameFile", Required = false, MetaValue = "FILE", HelpText = "Perfect matched query names (depercated)")]
    public string PerfectNameFile { get; set; }

    [Option('m', "mergeChromosomesByReads", Required = false, MetaValue = "BOOLEAN", HelpText = "Merge chromosomes by mapped reads")]
    public bool MergeChromosomesByReads { get; set; }

    private SmallRNACountMap cm;
    public virtual SmallRNACountMap GetCountMap()
    {
      if (cm == null)
      {
        cm = new SmallRNACountMap(this.CountFile);
      }
      return cm;
    }

    public override bool PrepareOptions()
    {
      if (!File.Exists(this.InputFile))
      {
        ParsingErrors.Add(string.Format("Input file not exists {0}.", this.InputFile));
      }

      if (!string.IsNullOrEmpty(this.CountFile) && !File.Exists(this.CountFile))
      {
        ParsingErrors.Add(string.Format("Count file not exists {0}.", this.CountFile));
      }

      return ParsingErrors.Count == 0;
    }
  }
}
