using RCPA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CQS.Genome.SmallRNA
{
  //The feature name of smallRNA in ensembl gff file
  public enum SmallRNABiotype { miRNA, tRNA, lincRNA, snoRNA, snRNA, rRNA, misc_RNA };

  public static class SmallRNAConsts
  {
    /// <summary>
    /// The feature name of miRNA in miRBase gff file. It will be the prefix of microRNA feature name.
    /// </summary>
    public static readonly string miRNA = SmallRNABiotype.miRNA.ToString();

    /// <summary>
    /// The feature name used in pipeline analysis. It will be the prefix of tRNA feature name.
    /// </summary>
    public static readonly string tRNA = SmallRNABiotype.tRNA.ToString();

    public static readonly string lincRNA = SmallRNABiotype.lincRNA.ToString();

    public const string NTA_TAG = ":CLIP_";

    public static string[] Biotypes;

    static SmallRNAConsts()
    {
      Biotypes = EnumUtils.EnumToStringArray<SmallRNABiotype>();
    }
  }
}
