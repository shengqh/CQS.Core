using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RCPA.Commandline;
using CommandLine;
using System.IO;

namespace CQS.Genome.Mirna
{
  public class MiRBaseGff3SequenceCombinerOptions : AbstractOptions
  {
    [Option('g', "gffFile", Required = true, MetaValue = "FILE", HelpText = "miRBase gff file")]
    public string GffFile { get; set; }

    [Option('f', "fastaFile", Required = true, MetaValue = "FILE", HelpText = "miRBase fasta file")]
    public string FastaFile { get; set; }

    [Option('o', "outputFile", Required = false, MetaValue = "FILE", HelpText = "Output gff file")]
    public string OutputFile { get; set; }

    public override bool PrepareOptions()
    {
      if (!File.Exists(this.GffFile))
      {
        ParsingErrors.Add(string.Format("Gff file not exists {0}.", this.GffFile));
        return false;
      }

      if (!File.Exists(this.FastaFile))
      {
        ParsingErrors.Add(string.Format("Fasta file not exists {0}.", this.FastaFile));
        return false;
      }

      return true;
    }
  }
}
