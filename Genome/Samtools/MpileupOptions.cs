using CommandLine;
using CQS.Genome.SomaticMutation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CQS.Genome.Samtools
{
  public class MpileupOptions : AbstractProgramOptions
  {
    public const int DEFAULT_MinimumBaseQuality = 20;
    private const int DEFAULT_MpileupMinimumReadQuality = 20;
    private const int DEFAULT_MinimumReadDepth = 0;
    private const int DEFAULT_MaximumReadDepth = 0;

    [Option("no-BAQ", MetaValue = "BOOL", HelpText = "disable BAQ (per-Base Alignment Quality) for samtools mpileup (when type==bam)")]
    public bool DisableBAQ { get; set; }

    [Option("read_quality", MetaValue = "INT", DefaultValue = DEFAULT_MpileupMinimumReadQuality, HelpText = "Minimum mapQ of read for samtools mpileup (when type==bam)")]
    public int MinimumReadQuality { get; set; }

    [Option("base_quality", MetaValue = "INT", DefaultValue = DEFAULT_MinimumBaseQuality, HelpText = "Minimum base quality")]
    public int MinimumBaseQuality { get; set; }

    [Option('f', "fasta", MetaValue = "FILE", Required = false, HelpText = "Genome fasta file for samtools mpileup (when type==bam)")]
    public string GenomeFastaFile { get; set; }

    [Option("min_read_depth", MetaValue = "INT", DefaultValue = DEFAULT_MinimumReadDepth, HelpText = "Minimum read depth of base passed mapping quality filter in each sample")]
    public virtual int MinimumReadDepth { get; set; }

    [Option("max_read_depth", MetaValue = "INT", DefaultValue = DEFAULT_MaximumReadDepth, HelpText = "Maximum read depth of base passed mapping quality filter in each sample")]
    public virtual int MaximumReadDepth { get; set; }

    /// <summary>
    /// For somatic mutation call, IgnoreDepthLimitation should be false to accerate the call.
    /// For somatic mutation validation, IgnoreDepthLimitation should be true to keep all the information.
    /// </summary>
    public bool IgnoreDepthLimitation { get; set; }

    public MpileupOptions()
    {
      this.MinimumBaseQuality = DEFAULT_MinimumBaseQuality;
      this.MinimumReadQuality = DEFAULT_MpileupMinimumReadQuality;
      this.MinimumReadDepth = DEFAULT_MinimumReadDepth;
      this.MaximumReadDepth = DEFAULT_MaximumReadDepth;
      this.IgnoreDepthLimitation = false;
    }

    public string GetSamtoolsCommand()
    {
      var result = new FileInfo(Application.ExecutablePath).DirectoryName + "/samtools";
      if (!File.Exists(result))
      {
        result = "samtools";
      }
      return result;
    }

    public override bool PrepareOptions()
    {
      if (null == GenomeFastaFile)
      {
        ParsingErrors.Add("Genome fasta file not defined.");
      }
      else if (!File.Exists(GenomeFastaFile))
      {
        ParsingErrors.Add(string.Format("Genome fasta file not exists {0}.", GenomeFastaFile));
      }
      else
      {
        Console.Out.WriteLine("#mpileup genome fasta: " + GenomeFastaFile);
      }

      return ParsingErrors.Count == 0;
    }

    public virtual void PrintParameter(TextWriter tw)
    {
      tw.WriteLine("#no-BAQ={0}", DisableBAQ.ToString());
      tw.WriteLine("#read_quality={0}", MinimumReadQuality);
      tw.WriteLine("#base_quality={0}", MinimumBaseQuality);
      tw.WriteLine("#fasta={0}", GenomeFastaFile);
      tw.WriteLine("#min_read_depth={0}", MinimumReadDepth);
      tw.WriteLine("#max_read_depth={0}", MaximumReadDepth);
    }
  }
}
