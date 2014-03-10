using System.IO;
using CommandLine;
using CQS.Commandline;

namespace CQS.Genome.Fastq
{
  public class Bam2FastqProcessorOptions : AbstractOptions
  {
    public Bam2FastqProcessorOptions()
    {
      UnGzipped = false;
    }

    [Option('i', "inputFile", Required = true, MetaValue = "FILE", HelpText = "Input bam file")]
    public string InputFile { get; set; }

    [Option('o', "outputPrefix", Required = true, MetaValue = "STRING", HelpText = "Output file prefix")]
    public string OutputPrefix { get; set; }

    [Option('u', "ungzip", DefaultValue = false, HelpText = "Ungzip the result")]
    public bool UnGzipped { get; set; }

    public override bool PrepareOptions()
    {
      if (!"-".Equals(InputFile) && !File.Exists(InputFile))
      {
        ParsingErrors.Add(string.Format("Input file not exists {0}.", InputFile));
        return false;
      }

      return true;
    }
  }
}