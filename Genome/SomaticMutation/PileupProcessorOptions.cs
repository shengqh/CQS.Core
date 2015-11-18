﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CommandLine;
using CQS.Genome.Pileup;
using RCPA.Seq;
using System.Windows.Forms;
using CQS.Genome.Samtools;

namespace CQS.Genome.SomaticMutation
{
  public class PileupProcessorOptions : MpileupOptions
  {
    private const int DEFAULT_MinimumReadDepth = 10;
    private const int DEFAULT_MinimumReadsOfMinorAlleleInTumor = 5;

    private const double DEFAULT_MaximumPercentageOfMinorAlleleInNormal = 0.02;
    private const double DEFAULT_MinimumPercentageOfMinorAlleleInTumor = 0.1;
    public const double DEFAULT_FisherPvalue = 0.05;

    private const int DEFAULT_ThreadCount = 1;

    public PileupProcessorOptions()
    {
      From = DataSourceType.BAM;
      IgnoreInsertionDeletion = true;
      IgnoreTerminalBase = false;
      IgnoreN = true;

      MinimumReadDepth = DEFAULT_MinimumReadDepth;

      MaximumPercentageOfMinorAlleleInNormal = DEFAULT_MaximumPercentageOfMinorAlleleInNormal;
      MinimumReadsOfMinorAlleleInTumor = DEFAULT_MinimumReadsOfMinorAlleleInTumor;
      MinimumPercentageOfMinorAlleleInTumor = DEFAULT_MinimumPercentageOfMinorAlleleInTumor;
      FisherPvalue = DEFAULT_FisherPvalue;
      ThreadCount = DEFAULT_ThreadCount;
    }

    public bool IgnoreInsertionDeletion { get; set; }

    public bool IgnoreTerminalBase { get; set; }

    public bool IgnoreN { get; set; }

    [Option('t', "type", MetaValue = "STRING", Required = true, HelpText = "Where to read/generate mpileup result file (bam/mpileup/console)")]
    public DataSourceType From { get; set; }

    [Option("normal", MetaValue = "FILE", Required = false, HelpText = "Bam file from normal sample (when type==bam)")]
    public string NormalBam { get; set; }

    [Option("tumor", MetaValue = "FILE", Required = false, HelpText = "Bam file from tumor sample (when type==bam)")]
    public string TumorBam { get; set; }

    [OptionList('r', "chromosomes", MetaValue = "STRING", Separator = ',', Required = false, HelpText = "(Default: all chromosomes in genome fasta file) Chromosome names (separted by ',') (when type==bam)")]
    public IList<string> ChromosomeNames { get; set; }

    [Option('m', "mpileup", MetaValue = "FILE", Required = false, HelpText = "Input samtools mpileup result file (when type==mpileup)")]
    public string MpileupFile { get; set; }

    [Option('d', "read_depth", MetaValue = "INT", DefaultValue = DEFAULT_MinimumReadDepth, HelpText = "Minimum read depth of base passed mapping quality filter in each sample")]
    public int MinimumReadDepth { get; set; }

    [Option("fisher_pvalue", MetaValue = "DOUBLE", DefaultValue = DEFAULT_FisherPvalue, HelpText = "Maximum pvalue used for fisher exact test")]
    public double FisherPvalue { get; set; }

    [Option("max_normal_percentage", MetaValue = "DOUBLE", DefaultValue = DEFAULT_MaximumPercentageOfMinorAlleleInNormal, HelpText = "Maximum percentage of minor allele at normal sample")]
    public double MaximumPercentageOfMinorAlleleInNormal { get; set; }

    [Option("min_tumor_read", MetaValue = "INT", DefaultValue = DEFAULT_MinimumReadsOfMinorAlleleInTumor, HelpText = "Minimum read count of minor allele at tumor sample")]
    public int MinimumReadsOfMinorAlleleInTumor { get; set; }

    [Option("min_tumor_percentage", MetaValue = "DOUBLE", DefaultValue = DEFAULT_MinimumPercentageOfMinorAlleleInTumor, HelpText = "Minimum percentage of minor allele at tumor sample")]
    public double MinimumPercentageOfMinorAlleleInTumor { get; set; }

    [Option('c', "thread_count", MetaValue = "INT", DefaultValue = DEFAULT_ThreadCount, HelpText = "Number of thread, only valid when type is bam")]
    public int ThreadCount { get; set; }

    [Option('o', "output", MetaValue = "STRING", Required = true, HelpText = "Output file suffix")]
    public string OutputSuffix { get; set; }

    [Option("exclude_bed", MetaValue = "FILE", Required = false, HelpText = "Exclude the range in bed format file")]
    public string ExcludeBedFile { get; set; }

    public string CandidatesDirectory
    {
      get { return GetOutputDirectory() + "/temp"; }
    }

    private string GetOutputDirectory()
    {
      if (!OutputSuffix.Contains('/') && !OutputSuffix.Contains('\\'))
      {
        return Path.GetDirectoryName(new FileInfo("./" + OutputSuffix).FullName);
      }
      return Path.GetDirectoryName(new FileInfo(OutputSuffix).FullName);
    }

    public PileupItemParser GetPileupItemParser(bool limitMinimumReadDepth = true)
    {
      if (limitMinimumReadDepth)
      {
        return new PileupItemParser(MinimumReadDepth, MinimumBaseQuality, IgnoreInsertionDeletion, IgnoreN,
          IgnoreTerminalBase);
      }
      else
      {
        return new PileupItemParser(0, MinimumBaseQuality, IgnoreInsertionDeletion, IgnoreN,
          IgnoreTerminalBase);
      }
    }

    public override bool PrepareOptions()
    {
      if (!PrepareOutputDirectory())
      {
        return false;
      }

      if (!string.IsNullOrWhiteSpace(ExcludeBedFile) && !File.Exists(ExcludeBedFile))
      {
        ParsingErrors.Add("Exclude file not exists:" + ExcludeBedFile);
        return false;
      }

      switch (From)
      {
        case DataSourceType.Mpileup:
          if (null == MpileupFile)
          {
            ParsingErrors.Add("Mpileup file not defined.");
            return false;
          }
          if (!File.Exists(MpileupFile))
          {
            ParsingErrors.Add(string.Format("Mpileup file not exists {0}.", MpileupFile));
            return false;
          }
          Console.Out.WriteLine("#mpileup file: " + MpileupFile);
          break;
        case DataSourceType.BAM:
          Console.WriteLine("BAM file...");

          if (null == NormalBam)
          {
            ParsingErrors.Add("Bam file for normal sample not defined.");
          }
          else if (!File.Exists(NormalBam))
          {
            ParsingErrors.Add(string.Format("Bam file for normal sample not exists {0}", NormalBam));
          }
          else
          {
            Console.Out.WriteLine("#mpileup normal file: " + this.NormalBam);
          }

          if (null == TumorBam)
          {
            ParsingErrors.Add("Bam file for tumor sample not defined.");
          }
          else if (!File.Exists(TumorBam))
          {
            ParsingErrors.Add(string.Format("Bam file for tumor sample is not exists {0}", TumorBam));
          }
          else
          {
            Console.Out.WriteLine("#mpileup tumor file: " + this.TumorBam);
          }

          if (ThreadCount >= 2)
          {
            Console.WriteLine("Checking chromosome names for thread mode ...");
            if (ChromosomeNames == null || ChromosomeNames.Count == 0 && File.Exists(GenomeFastaFile))
            {
              var fai = GenomeFastaFile + ".fai";
              if (File.Exists(fai))
              {
                var lines = File.ReadAllLines(fai);
                ChromosomeNames = lines.ToList().ConvertAll(m =>
                {
                  var pos = m.IndexOfAny(new[] { '\t', ' ' });
                  if (pos == -1)
                  {
                    return m;
                  }
                  return m.Substring(0, pos);
                });
              }
              else
              {
                Console.WriteLine("Reading chromosome names from fasta file ...");
                ChromosomeNames = SequenceUtils.ReadFastaNames(GenomeFastaFile);
                if (ChromosomeNames.Count == 0)
                {
                  ParsingErrors.Add(string.Format("Genome fasta file doesn't contain chromosome names, {0}.",
                    GenomeFastaFile));
                  return false;
                }
              }

              foreach (var chr in ChromosomeNames)
              {
                Console.WriteLine(chr);
              }
            }
          }
          else
          {
            if (ChromosomeNames != null && ChromosomeNames.Count > 0)
            {
              Console.Out.WriteLine("#mpileup chromosome names: " + ChromosomeNames.Merge(","));
            }
          }

          break;
        case DataSourceType.Console:
          Console.Out.WriteLine("#mpileup from console.");
          break;
      }

      return true;
    }

    public string GetMpileupChromosomes()
    {
      if (ChromosomeNames != null && ChromosomeNames.Count > 0)
      {
        return ChromosomeNames.Merge(",");
      }
      return null;
    }

    protected static string GetLinuxFile(string filename)
    {
      return Path.GetFullPath(filename).Replace("\\", "/");
    }

    public string BaseFilename { get { return GetLinuxFile(OutputSuffix + ".bases"); } }
    public string SummaryFilename { get { return GetLinuxFile(OutputSuffix + ".summary"); } }
    public string CandidatesFilename { get { return GetLinuxFile(OutputSuffix + ".candidates"); } }

    private bool PrepareOutputDirectory()
    {
      var outputdir = GetOutputDirectory();

      if (!Directory.Exists(outputdir))
      {
        try
        {
          Directory.CreateDirectory(outputdir);
        }
        catch (Exception ex)
        {
          ParsingErrors.Add(string.Format("Cannot create directory {0} : {1}", outputdir, ex.Message));
          return false;
        }
      }

      if (!Directory.Exists(CandidatesDirectory))
      {
        try
        {
          Directory.CreateDirectory(CandidatesDirectory);
        }
        catch (Exception ex)
        {
          ParsingErrors.Add(string.Format("Cannot create directory {0} : {1}", CandidatesDirectory, ex.Message));
          return false;
        }
      }

      return true;
    }

    public virtual AbstractPileupProcessor GetProcessor()
    {
      if (this.ThreadCount < 2)
        return new PileupProcessorSingleThread(this);
      else
        return new PileupProcessorParallelChromosome(this);
    }

    public override void PrintParameter()
    {
      Console.Out.WriteLine("#output directory: " + this.OutputSuffix);
      Console.Out.WriteLine("#minimum count: " + this.MinimumReadDepth);

      if (this.From == DataSourceType.BAM)
      {
        base.PrintParameter();
      }
      else
      {
        Console.Out.WriteLine("#minimum base quality: " + this.MinimumBaseQuality);
      }

      Console.Out.WriteLine("#maximum percentage of minor allele in normal: " + this.MaximumPercentageOfMinorAlleleInNormal);
      Console.Out.WriteLine("#minimum percentage of minor allele in tumor: " + this.MinimumPercentageOfMinorAlleleInTumor);
      Console.Out.WriteLine("#minimum reads of minor allele in tumor: " + this.MinimumReadsOfMinorAlleleInTumor);
      Console.Out.WriteLine("#pvalue: " + this.FisherPvalue);
      Console.Out.WriteLine("#thread count: " + this.ThreadCount);
    }
  }
}