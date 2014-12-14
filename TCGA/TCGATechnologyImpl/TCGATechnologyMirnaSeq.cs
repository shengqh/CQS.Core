using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RCPA;
using CQS.TCGA.Microarray;

namespace CQS.TCGA.TCGATechnologyImpl
{
  public class TCGATechnologyMirnaSeq : AbstractTCGATechnology
  {
    public override string NodeName
    {
      get
      {
        return "mirnaseq";
      }
    }
    
    public override IFileReader<ExpressionData> GetReader()
    {
      return new ExpressionDataMapReader("miRNA_ID", "reads_per_million_miRNA_mapped");
    }

    public override IFileReader<ExpressionData> GetCountReader()
    {
      return new ExpressionDataMapReader("miRNA_ID", "read_count");
    }

    public override IParticipantFinder GetFinder(string tumorDir, string platformDir)
    {
      return new FindParticipantRnaSeq(FindSdrfFile(platformDir));
    }

    public override Func<string, bool> GetFilenameFilter()
    {
      return m => m.ToLower().EndsWith(".mirna.quantification.txt") && (!m.ToLower().Contains("hg19"));
    }

    public override bool HasCountData
    {
      get
      {
        return true;
      }
    }

    public override string DefaultPreferPlatform
    {
      get
      {
        return "illuminahiseq_rnaseqv2";
      }
    }

    public override string ValueName
    {
      get
      {
        return "RPM";
      }
    }
  }
}
