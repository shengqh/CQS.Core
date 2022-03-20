using RCPA;

namespace CQS.TCGA.TCGATechnologyImpl
{
  public abstract class AbstractTCGATechnologyRNAseq : AbstractTCGATechnology
  {
    public override IFileReader<ExpressionData> GetReader()
    {
      return new ExpressionDataRawReader(2, -1);
    }

    public override IFileReader<ExpressionData> GetCountReader()
    {
      return new ExpressionDataRawReader(2, 1);
    }

    public override IParticipantFinder GetFinder(string tumorDir, string platformDir)
    {
      return new FindParticipantRnaSeq(FindSdrfFile(platformDir));
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
    public override string ToString()
    {
      return ValueName;
    }
  }
}
