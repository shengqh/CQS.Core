using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RCPA.Commandline;
using CommandLine;
using System.IO;
using CQS.Genome.Sam;

namespace CQS.Genome.Pileup
{
  public class SomaticMutationPileupBuilderOptions : AlignedPositionMapBuilderOptions
  {
    private const double DEFAULT_Pvalue = 0.01;
    private const double DEFAULT_MaxNormalFrequency = 0.1;
    private const double DEFAULT_MinTumorFrequency = 0.1;
    private const int DEFAULT_MinNormalDepth = 10;
    private const int DEFAULT_MinTumorDepth = 10;
    private const int DEFAULT_Thread = 1;


    public SomaticMutationPileupBuilderOptions()
    {
      this.Pvalue = DEFAULT_Pvalue;
      this.MaxNormalFrequency = DEFAULT_MaxNormalFrequency;
      this.MinTumorFrequency = DEFAULT_MinTumorFrequency;
      this.ThreadCount = DEFAULT_Thread;
    }

    [Option('n', "normal", MetaValue = "FILE", Required = true, HelpText = "SAM/BAM files for normal sample")]
    public string NormalFile { get; set; }

    [Option('t', "tumor", MetaValue = "FILE", Required = true, HelpText = "SAM/BAM files for tumor sample")]
    public string TumorFile { get; set; }

    [OptionList('c', "chromosomes", MetaValue = "STRING", Separator = ',', Required = false, HelpText = "Chromosome names (separted by ',', default is all chromosomes)")]
    public IList<string> ChromosomeNames { get; set; }

    [Option('p', "pvalue", MetaValue = "DOUBLE", DefaultValue = DEFAULT_Pvalue, HelpText = "pvalue used for significance test")]
    public double Pvalue { get; set; }

    [Option("max_normal_frequency", MetaValue = "DOUBLE", DefaultValue = DEFAULT_MaxNormalFrequency, HelpText = "Maximum frequency of minor allele at normal sample")]
    public double MaxNormalFrequency { get; set; }

    [Option("min_tumor_frequency", MetaValue = "DOUBLE", DefaultValue = DEFAULT_MinTumorFrequency, HelpText = "Minimum frequency of minor allele at tumor sample (second sample)")]
    public double MinTumorFrequency { get; set; }

    [Option("min_normal_depth", MetaValue = "INT", DefaultValue = DEFAULT_MinNormalDepth, HelpText = "Minimum read depth of base passed mapping quality filter in normal sample")]
    public int MinimumNormalDepth { get; set; }

    [Option("min_tumor_depth", MetaValue = "INT", DefaultValue = DEFAULT_MinTumorDepth, HelpText = "Minimum read depth of base passed mapping quality filter in tumor sample")]
    public int MinimumTumorDepth { get; set; }

    [Option("thread", MetaValue = "INT", DefaultValue = DEFAULT_Thread, HelpText = "Number of thread")]
    public int ThreadCount { get; set; }

    [Option('o', "output", MetaValue = "FILE", Required = true, HelpText = "Output file")]
    public string OutputFile { get; set; }

    public string CandidatesDirectory
    {
      get
      {
        return Path.GetDirectoryName(this.OutputFile) + "/candidates";
      }
    }

    public override bool PrepareOptions()
    {

      if (!PrepareOutputDirectory())
      {
        return false;
      }

      try
      {
        using (SAMFactory.GetReader(this.NormalFile)) { }
        using (SAMFactory.GetReader(this.TumorFile)) { }
      }
      catch (Exception ex)
      {
        ParsingErrors.Add(ex.Message);
        return false;
      }

      if (this.ThreadCount >= 2)
      {
        Console.WriteLine("Checking chromosome names for thread mode ...");
        if (this.ChromosomeNames == null || this.ChromosomeNames.Count == 0)
        {
          this.ChromosomeNames = SAMUtils.GetChromosomes(this.NormalFile);
        }

        foreach (var chr in this.ChromosomeNames)
        {
          Console.WriteLine(chr);
        }
      }
      else
      {
        if (this.ChromosomeNames != null && this.ChromosomeNames.Count > 0)
        {
          Console.Out.WriteLine("#mpileup chromosome names: " + this.ChromosomeNames.Merge(","));
        }
      }

      return true;
    }

    private bool PrepareOutputDirectory()
    {
      if (!Directory.Exists(this.CandidatesDirectory))
      {
        try
        {
          Directory.CreateDirectory(this.CandidatesDirectory);
        }
        catch (Exception ex)
        {
          ParsingErrors.Add(string.Format("Cannot create directory {0} : {1}", this.CandidatesDirectory, ex.Message));
          return false;
        }
      }

      return true;
    }
  }
}
