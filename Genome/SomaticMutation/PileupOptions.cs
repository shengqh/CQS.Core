using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CommandLine;
using CQS.Genome.Pileup;
using RCPA.Seq;
using System.Windows.Forms;

namespace CQS.Genome.SomaticMutation
{
  public class PileupOptions : AbstractProgramOptions
  {
    public enum DataSourceType
    {
      BAM,
      Mpileup,
      Console
    }
    public const int DefaultMinimumBaseQuality = 20;
    public const double DefaultPValue = 0.05;

    private const int DefaultMinimumReadDepth = 10;
    private const int DefaultMpileupMinimumReadQuality = 20;
    private const double DEFAULT_MaximumPercentageOfMinorAlleleInNormal = 0.05;
    private const int DEFAULT_MinimumReadsOfMinorAlleleInTumor = 3;
    private const double DEFAULT_MinimumPercentageOfMinorAlleleInTumor = 0.1;
    private const int DefaultThreadCount = 1;

    public PileupOptions()
    {
      From = DataSourceType.BAM;
      IgnoreInsertionDeletion = true;
      IgnoreTerminalBase = false;
      IgnoreN = true;

      MinimumReadDepth = DefaultMinimumReadDepth;
      MpileupMinimumReadQuality = DefaultMpileupMinimumReadQuality;
      MinimumBaseQuality = DefaultMinimumBaseQuality;

      MaximumPercentageOfMinorAlleleInNormal = DEFAULT_MaximumPercentageOfMinorAlleleInNormal;
      MinimumReadsOfMinorAlleleInTumor = DEFAULT_MinimumReadsOfMinorAlleleInTumor;
      MinimumPercentageOfMinorAlleleInTumor = DEFAULT_MinimumPercentageOfMinorAlleleInTumor;
      PValue = DefaultPValue;
      ThreadCount = DefaultThreadCount;
    }

    public bool IgnoreInsertionDeletion { get; set; }

    public bool IgnoreTerminalBase { get; set; }

    public bool IgnoreN { get; set; }

    [Option('t', "type", MetaValue = "STRING", Required = true, HelpText = "Where to read/generate mpileup result file (bam/mpileup/console)")]
    public DataSourceType From { get; set; }

    [Option("normal", MetaValue = "FILE", Required = false, HelpText = "Bam file from normal sample")]
    public string NormalBam { get; set; }

    [Option("tumor", MetaValue = "FILE", Required = false, HelpText = "Bam file from tumor sample")]
    public string TumorBam { get; set; }

    [Option('f', "fasta", MetaValue = "FILE", Required = false, HelpText = "Genome fasta file for samtools mpileup")]
    public string GenomeFastaFile { get; set; }

    [OptionList('r', "chromosomes", MetaValue = "STRING", Separator = ',', Required = false,
      HelpText = "Default(all chromosomes in genome fasta file) Chromosome names (separted by ',')")]
    public IList<string> ChromosomeNames { get; set; }

    [Option('n', "read_quality", MetaValue = "INT", DefaultValue = DefaultMpileupMinimumReadQuality,
      HelpText = "Minimum mapQ of read for samtools mpileup")]
    public int MpileupMinimumReadQuality { get; set; }

    [Option('m', "mpileup", MetaValue = "FILE", Required = false, HelpText = "Samtools mpileup result file")]
    public string MpileupFile { get; set; }

    [Option('e', "pvalue", MetaValue = "DOUBLE", DefaultValue = DefaultPValue,  HelpText = "pvalue used for significance test")]
    public double PValue { get; set; }

    [Option('q', "base_quality", MetaValue = "INT", DefaultValue = DefaultMinimumBaseQuality,
      HelpText = "Minimum base quality for mpileup result filter")]
    public int MinimumBaseQuality { get; set; }

    [Option("max_normal_percentage", MetaValue = "DOUBLE", DefaultValue = DEFAULT_MaximumPercentageOfMinorAlleleInNormal,
      HelpText = "Maximum percentage of minor allele at normal sample")]
    public double MaximumPercentageOfMinorAlleleInNormal { get; set; }

    [Option("min_read_tumor", MetaValue = "INT", DefaultValue = DEFAULT_MinimumReadsOfMinorAlleleInTumor,
      HelpText = "Minimum read count of minor allele at tumor sample")]
    public int MinimumReadsOfMinorAlleleInTumor { get; set; }

    [Option('g', "percentage", MetaValue = "DOUBLE", DefaultValue = DEFAULT_MinimumPercentageOfMinorAlleleInTumor,
      HelpText = "Minimum percentage of minor allele at tumor sample")]
    public double MinimumPercentageOfMinorAlleleInTumor { get; set; }

    [Option('d', "read_depth", MetaValue = "INT", DefaultValue = DefaultMinimumReadDepth,
      HelpText = "Minimum read depth of base passed mapping quality filter in each sample")]
    public int MinimumReadDepth { get; set; }

    //[Option('c', "thread_count", MetaValue = "INT", DefaultValue = DefaultThreadCount, HelpText = "Number of thread, only valid when type is bam")]
    public int ThreadCount { get; set; }

    [Option('o', "output", MetaValue = "STRING", Required = true, HelpText = "Output file suffix")]
    public string OutputSuffix { get; set; }

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

    public PileupItemParser GetPileupItemParser()
    {
      return new PileupItemParser(MinimumReadDepth, MinimumBaseQuality, IgnoreInsertionDeletion, IgnoreN,
        IgnoreTerminalBase);
    }

    public string GetSamtoolsCommand()
    {
      return new FileInfo(Application.ExecutablePath).DirectoryName + "/samtools";
    }

    public override bool PrepareOptions()
    {
      if (!PrepareOutputDirectory())
      {
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
          if (!Config.Programs.ContainsKey("samtools"))
          {
            Config.Programs["samtools"] = new ProgramConfig("samtools", "samtools");
            Console.Out.WriteLine(
              "samtools is not defined, default will be samtools. You can also modify it at file {0}.",
              Config.ConfigFilename);
            Config.Save();
          }

          if (null == NormalBam)
          {
            ParsingErrors.Add("Bam file for normal sample not defined.");
            return false;
          }

          if (null == TumorBam)
          {
            ParsingErrors.Add("Bam file for tumor sample not defined.");
            return false;
          }

          if (!File.Exists(NormalBam))
          {
            ParsingErrors.Add(string.Format("Bam file for normal sample not exists {0}", NormalBam));
            return false;
          }

          if (!File.Exists(TumorBam))
          {
            ParsingErrors.Add(string.Format("Bam file for tumor sample is not exists {0}", TumorBam));
            return false;
          }

          if (null == GenomeFastaFile)
          {
            ParsingErrors.Add("Genome fasta file not defined.");
            return false;
          }
          if (!File.Exists(GenomeFastaFile))
          {
            ParsingErrors.Add(string.Format("Genome fasta file not exists {0}.", GenomeFastaFile));
            return false;
          }

          if (ThreadCount >= 2)
          {
            Console.WriteLine("Checking chromosome names for thread mode ...");
            if (ChromosomeNames == null || ChromosomeNames.Count == 0)
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
            }

            foreach (var chr in ChromosomeNames)
            {
              Console.WriteLine(chr);
            }
          }
          else
          {
            if (ChromosomeNames != null && ChromosomeNames.Count > 0)
            {
              Console.Out.WriteLine("#mpileup chromosome names: " + ChromosomeNames.Merge(","));
            }
          }

          Console.Out.WriteLine("#mpileup normal file: " + this.NormalBam);
          Console.Out.WriteLine("#mpileup tumor file: " + this.TumorBam);
          Console.Out.WriteLine("#mpileup minimum read quality: " + MpileupMinimumReadQuality);
          Console.Out.WriteLine("#mpileup genome fasta: " + GenomeFastaFile);

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

    public AbstractPileupProcessor GetProcessor()
    {
      if (this.ThreadCount < 2)
        return new PileupSingleProcessor(this);
      else
        return new PileupParallelChromosomeProcessor(this);

      //if (this.From != PileupOptions.DataSourceType.BAM || this.ThreadCount < 2)
      //{
      //  return new PileupSingleProcessor(this);
      //}

      //return new PileupParallelChromosomeProcessor(this);
    }

    public void PrintParameter()
    {
      Console.Out.WriteLine("#output directory: " + this.OutputSuffix);
      Console.Out.WriteLine("#minimum count: " + this.MinimumReadDepth);
      Console.Out.WriteLine("#minimum read quality: " + this.MpileupMinimumReadQuality);
      Console.Out.WriteLine("#minimum base quality: " + this.MinimumBaseQuality);
      Console.Out.WriteLine("#maximum percentage of minor allele in normal: " + this.MaximumPercentageOfMinorAlleleInNormal);
      Console.Out.WriteLine("#minimum percentage of minor allele in tumor: " + this.MinimumPercentageOfMinorAlleleInTumor);
      Console.Out.WriteLine("#minimum reads of minor allele in tumor: " + this.MinimumReadsOfMinorAlleleInTumor);
      Console.Out.WriteLine("#pvalue: " + this.PValue);
      Console.Out.WriteLine("#thread count: " + this.ThreadCount);
    }
  }
}