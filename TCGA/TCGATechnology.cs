using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CQS.TCGA.TCGATechnologyImpl;

namespace CQS.TCGA
{
  public static class TCGATechnology
  {
    public static readonly ITCGATechnology RNAseq_RPKM = new TCGATechnologyRNAseqV1();

    public static readonly ITCGATechnology RNAseq_RSEM = new TCGATechnologyRNAseqV2();

    public static readonly ITCGATechnology TotalRNAseq_RSEM = new TCGATechnologyTotalRNAseqV2();

    public static readonly ITCGATechnology MirnaSeq = new TCGATechnologyMirnaSeq();

    public static readonly ITCGATechnology Microarray = new TCGATechnologyMicroarray();

    public static readonly ITCGATechnology Mirna = new TCGATechnologyMirna();

    public static readonly ITCGATechnology Methylation = new TCGATechnologyMethylation();

    public static readonly ITCGATechnology CNA = new TCGATechnologyCNA();

    public static readonly ITCGATechnology Mutations = new TCGATechnologyMutations();

    public static readonly ITCGATechnology[] Technoligies = new[] { Microarray, RNAseq_RPKM, RNAseq_RSEM, TotalRNAseq_RSEM, MirnaSeq, Mirna, Methylation, CNA, Mutations };

    public static bool TryParse(string name, out ITCGATechnology value)
    {
      var lname = name.ToLower();
      foreach (var tec in Technoligies)
      {
        if (tec.ToString().ToLower().Equals(lname))
        {
          value = tec;
          return true;
        }
      }

      value = null;
      return false;
    }

    public static ITCGATechnology Parse(string name)
    {
      var lname = name.ToLower();
      foreach (var tec in Technoligies)
      {
        if (tec.NodeName.ToLower().Equals(lname))
        {
          return tec;
        }
      }

      throw new ArgumentException(string.Format("Cannot find {0} in TCGA, only {1} are allowed", name, GetTechnologyNames()));
    }

    public static List<string> GetTechnologyNames()
    {
      return (from tec in Technoligies
              select tec.NodeName.ToLower()).ToList();
    }

  }
}
