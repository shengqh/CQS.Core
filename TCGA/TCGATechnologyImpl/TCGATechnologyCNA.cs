using RCPA;
using System;

namespace CQS.TCGA.TCGATechnologyImpl
{
  public class TCGATechnologyCNA : AbstractTCGATechnology
  {
    public override string NodeName
    {
      get
      {
        return "cna";
      }
    }

    public override IFileReader<ExpressionData> GetReader()
    {
      throw new Exception("Unimplemented!");
      //      return new ExpressionDataMapReader("miRNA_ID", "reads_per_million_miRNA_mapped");
    }

    public override IFileReader<ExpressionData> GetCountReader()
    {
      //return new ExpressionDataMapReader("miRNA_ID", "read_count");
      throw new Exception("Unimplemented!");
    }

    public override IParticipantFinder GetFinder(string tumorDir, string platformDir)
    {
      return new FindParticipantRnaSeq(FindSdrfFile(platformDir));
    }

    public override Func<string, bool> GetFilenameFilter()
    {
      return m => m.ToLower().EndsWith("segment.tsv");
    }
  }
}
