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

    [Option("no-BAQ", MetaValue = "BOOL", HelpText = "disable BAQ (per-Base Alignment Quality) for samtools mpileup (when type==bam)")]
    public bool DisableBAQ { get; set; }

    [Option("read_quality", MetaValue = "INT", DefaultValue = DEFAULT_MpileupMinimumReadQuality, HelpText = "Minimum mapQ of read for samtools mpileup (when type==bam)")]
    public int MinimumReadQuality { get; set; }

    [Option("base_quality", MetaValue = "INT", DefaultValue = DEFAULT_MinimumBaseQuality, HelpText = "Minimum base quality")]
    public int MinimumBaseQuality { get; set; }

    [Option('f', "fasta", MetaValue = "FILE", Required = false, HelpText = "Genome fasta file for samtools mpileup (when type==bam)")]
    public string GenomeFastaFile { get; set; }

    public MpileupOptions()
    {
      this.MinimumBaseQuality = DEFAULT_MinimumBaseQuality;
      this.MinimumReadQuality = DEFAULT_MpileupMinimumReadQuality;
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

    public virtual void PrintParameter()
    {
      Console.Out.WriteLine("#mpileup minimum read quality: " + MinimumReadQuality);
      Console.Out.WriteLine("#mpileup Minimum base quality: " + MinimumBaseQuality);
      Console.Out.WriteLine("#disable BAQ (per-Base Alignment Quality): " + DisableBAQ.ToString());
      Console.Out.WriteLine("#genome fasta file: " + GenomeFastaFile);
    }
  }
}
