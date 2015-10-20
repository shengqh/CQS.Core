using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RCPA.Commandline;
using CommandLine;
using System.IO;
using RCPA;

namespace CQS.Genome.Mirna
{
  public class MirnaNonTemplatedNucleotideAdditionsQueryBuilderOptions : AbstractOptions
  {
    private const int DEFAULT_MinimumReadLength = 16;

    public MirnaNonTemplatedNucleotideAdditionsQueryBuilderOptions()
    {
      this.MinimumReadLength = DEFAULT_MinimumReadLength;
    }

    [Option('i', "inputFile", Required = true, MetaValue = "FILE", HelpText = "Fastq file")]
    public string InputFile { get; set; }

    [Option('c', "countFile", Required = false, MetaValue = "FILE", HelpText = "Sequence/count file")]
    public string CountFile { get; set; }

    [Option('o', "outputFile", Required = false, MetaValue = "FILE", HelpText = "Output fastq file")]
    public string OutputFile { get; set; }

    [Option('l', "minlen", MetaValue = "INT", DefaultValue = DEFAULT_MinimumReadLength, HelpText = "Minimum read length")]
    public int MinimumReadLength { get; set; }

    public override bool PrepareOptions()
    {
      if (!File.Exists(this.InputFile))
      {
        ParsingErrors.Add(string.Format("Input file not exists {0}.", this.InputFile));
        return false;
      }

      if (!string.IsNullOrEmpty(this.CountFile) && !File.Exists(this.CountFile))
      {
        ParsingErrors.Add(string.Format("Count file not exists {0}.", this.CountFile));
        return false;
      }

      if (string.IsNullOrEmpty(this.OutputFile))
      {
        string infile = this.InputFile;
        if (infile.ToLower().EndsWith(".gz"))
        {
          infile = FileUtils.ChangeExtension(infile, "");
        }
        infile = FileUtils.ChangeExtension(infile, "");

        Console.WriteLine("infile=" + infile);
        this.OutputFile = infile + "_mirna.fastq";
      }

      return true;
    }

    private CountMap cm;

    public virtual CountMap GetCountMap()
    {
      if (cm == null)
      {
        cm = new CountMap(this.CountFile);
      }
      return cm;
    }
  }
}

