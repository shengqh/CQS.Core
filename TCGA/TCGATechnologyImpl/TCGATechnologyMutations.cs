using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RCPA;
using CQS.TCGA.Microarray;
using System.IO;

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
      return new Level3MicroarrayDataTxtReader();
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
