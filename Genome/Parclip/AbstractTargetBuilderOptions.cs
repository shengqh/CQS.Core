using CommandLine;
using RCPA.Commandline;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CQS.Genome.Parclip
{
  public class AbstractTargetBuilderOptions : AbstractOptions
  {
    private const double DEFAULT_MINIMUM_COVERAGE = 2.0;

    public AbstractTargetBuilderOptions()
    {
      this.MinimumCoverage = DEFAULT_MINIMUM_COVERAGE;
    }

    [Option('o', "outputFile", Required = true, MetaValue = "FILE", HelpText = "Refined result file")]
    public string OutputFile { get; set; }

    /// <summary>
    /// @"H:\shengquanhu\projects\database\hg19_refgene.tsv"
    /// #bin	name	chrom	strand	txStart	txEnd	cdsStart	cdsEnd	exonCount	exonStarts	exonEnds	score	name2	cdsStartStat	cdsEndStat	exonFrames
    /// </summary>
    [Option('r', "refgeneFile", Required = true, MetaValue = "FILE", HelpText = "Refgene file downloaded from UCSC website, the 2nd column is gene name and the 13th column is gene symbol")]
    public string RefgeneFile { get; set; }

    [Option('t', "targetXmlFile", Required = true, MetaValue = "FILE", HelpText = "Target (such as 3'UTR) count xml file")]
    public string TargetXmlFile { get; set; }

    /// <summary>
    /// Minimum coverage of the 3'utr, default=2.0
    /// </summary>
    [Option('s', "minCoverage", Required = false, DefaultValue = DEFAULT_MINIMUM_COVERAGE, MetaValue = "DOUBLE", HelpText = "Minimum coverage of the target")]
    public double MinimumCoverage { get; set; }

    /// <summary>
    /// Genome sequence fasta file
    /// </summary>
    [Option('g', "genomeFastaFile", Required = true, MetaValue = "FILE", HelpText = "Genome fasta file")]
    public string GenomeFastaFile { get; set; }

    public override bool PrepareOptions()
    {
      if (!File.Exists(this.TargetXmlFile))
      {
        ParsingErrors.Add(string.Format("Target count xml file not exists {0}.", this.TargetXmlFile));
      }

      if (!File.Exists(this.RefgeneFile))
      {
        ParsingErrors.Add(string.Format("Refgene file not exists {0}.", this.RefgeneFile));
      }

      if (!File.Exists(this.GenomeFastaFile))
      {
        ParsingErrors.Add(string.Format("Genome fasta file not exists {0}.", this.GenomeFastaFile));
      }

      return ParsingErrors.Count == 0;
    }
  }
}
