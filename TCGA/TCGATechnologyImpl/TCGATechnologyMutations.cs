using RCPA;
using System;

namespace CQS.TCGA.TCGATechnologyImpl
{
  public class TCGATechnologyMutations : AbstractTCGATechnology
  {
    public override string NodeName
    {
      get
      {
        return "mutations";
      }
    }

    public override IFileReader<ExpressionData> GetReader()
    {
      return new Level3MutationDataTxtReader();
    }

    public override IParticipantFinder GetFinder(string tumorDir, string platformDir)
    {
      return new FindParticipantMicroarray(FindSdrfFile(platformDir));
    }

    public override Func<string, bool> GetFilenameFilter()
    {
      return m => m.ToLower().EndsWith(".somatic.maf");
    }
  }
}
