using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RCPA;
using System.IO;

namespace CQS.TCGA.TCGATechnologyImpl
{
  public class TCGATechnologyRNAseqV1 : AbstractTCGATechnology
  {
    public override string NodeName
    {
      get
      {
        return "rnaseq";
      }
    }

    public override IFileReader<ExpressionData> GetReader()
    {
      return new ExpressionDataRawReader(2, -1);
    }

    public override IFileReader<ExpressionData> GetCountReader()
    {
      return new ExpressionDataRawReader(2, 1);
    }

    public override string ToString()
    {
      return "RPKM";
    }

    public override IParticipantFinder GetFinder(string tumorDir, string platformDir)
    {
      return new FindParticipantRnaSeq(FindSdrfFile(platformDir));
    }

    public override Func<string, bool> GetFilenameFilter()
    {
      return m => m.ToLower().EndsWith(".gene.quantification.txt");
    }
  }
}
