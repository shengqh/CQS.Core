using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RCPA.Commandline;
using CommandLine;
using System.IO;

namespace CQS.Genome.Quantification
{
  public class HTSeqCountToFPKMCalculatorOptions : AbstractOptions
  {
    private const string DEFAULT_KEY_REGEX = "";

    public HTSeqCountToFPKMCalculatorOptions()
    {
      KeyRegex = DEFAULT_KEY_REGEX;
    }

    [Option('i', "inputFile", Required = true, MetaValue = "FILE", HelpText = "Count table file, row is gene and column is sample")]
    public string InputFile { get; set; }

    [Option('p', "keyRegex", DefaultValue = DEFAULT_KEY_REGEX, MetaValue = "REGEX", HelpText = "Regex of gene name")]
    public string KeyRegex { get; set; }

    [Option('l', "geneLengthFile", Required = true, MetaValue = "FILE", HelpText = "Gene length file, the first column is gene_id and the column name of length should be 'length'")]
    public string GeneLengthFile { get; set; }

    [Option('n', "sampleReadsFile", Required = false, MetaValue = "FILE", HelpText = "Total reads file, the first column is sample name and the second column is total reads. If not provided, the total mapped reads will be used for each sample")]
    public string SampleReadsFile { get; set; }

    [Option('o', "outputFile", Required = true, MetaValue = "FILE", HelpText = "Output table file")]
    public string OutputFile { get; set; }

    public override bool PrepareOptions()
    {
      if (!File.Exists(this.InputFile))
      {
        ParsingErrors.Add(string.Format("Input file not exists {0}.", this.InputFile));
      }

      if (!File.Exists(this.GeneLengthFile))
      {
        ParsingErrors.Add(string.Format("Gene length file not exists {0}.", this.GeneLengthFile));
      }

      if (!string.IsNullOrEmpty(this.SampleReadsFile) && !File.Exists(this.SampleReadsFile))
      {
        ParsingErrors.Add(string.Format("Sample reads file not exists {0}.", this.SampleReadsFile));
      }

      return ParsingErrors.Count == 0;
    }
  }
}
