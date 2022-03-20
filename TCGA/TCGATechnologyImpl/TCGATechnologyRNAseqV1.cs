using System;

namespace CQS.TCGA.TCGATechnologyImpl
{
  public class TCGATechnologyRNAseqV1 : AbstractTCGATechnologyRNAseq
  {
    public override string NodeName
    {
      get
      {
        return "rnaseq";
      }
    }

    public override Func<string, bool> GetFilenameFilter()
    {
      return m => m.ToLower().EndsWith(".gene.quantification.txt");
    }

    public override string ValueName
    {
      get { return "RPKM"; }
    }
  }
}
