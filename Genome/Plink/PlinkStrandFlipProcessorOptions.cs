using CommandLine;
using RCPA.Commandline;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CQS.Genome.Plink
{
  public class PlinkStrandFlipProcessorOptions : AbstractOptions
  {
    [Option('i', "inputFile", Required = true, MetaValue = "FILE", HelpText = "Plink bed file")]
    public string InputFile { get; set; }

    [Option('d', "dbsnpFile", Required = true, MetaValue = "FILE", HelpText = "dbSNP vcf format file")]
    public string DbsnpFile { get; set; }

    [Option('f', "fastaFile", Required = true, MetaValue = "FILE", HelpText = "genome fasta file")]
    public string FastaFile { get; set; }

    [Option('g', "g1000File", Required = false, MetaValue = "FILE", HelpText = "1000 genome dbSNP vcf format file")]
    public string G1000File { get; set; }

    [Option('o', "outputPrefix", Required = false, MetaValue = "STRING", HelpText = "Output file prefix")]
    public string OutputPrefix { get; set; }

    public override bool PrepareOptions()
    {
      if (!File.Exists(this.InputFile))
      {
        ParsingErrors.Add(string.Format("Input file not exists {0}.", this.InputFile));
        return false;
      }

      if (!File.Exists(this.DbsnpFile))
      {
        ParsingErrors.Add(string.Format("Dbsnp file not exists {0}.", this.DbsnpFile));
        return false;
      }

      if (!File.Exists(this.FastaFile))
      {
        ParsingErrors.Add(string.Format("Genome fasta file not exists {0}.", this.FastaFile));
        return false;
      }

      if (!string.IsNullOrEmpty(this.G1000File) && !File.Exists(this.G1000File))
      {
        ParsingErrors.Add(string.Format("1000 genome file not exists {0}.", this.G1000File));
        return false;
      }

      return true;
    }
  }
}
