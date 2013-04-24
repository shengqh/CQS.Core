using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RCPA;
using CQS.TCGA.Microarray;
using System.IO;

namespace CQS.TCGA.TCGATechnologyImpl
{
  public class TCGATechnologyMicroarray : AbstractTCGATechnology
  {
    public override string NodeName
    {
      get
      {
        return "transcriptome";
      }
    }

    public override IFileReader<ExpressionData> GetReader()
    {
      return new Level3MicroarrayDataTxtReader();
    }

    public override bool IsData(SpiderTreeNode node)
    {
      return base.IsData(node) && (!node.Parent.Name.Equals("illuminaga_mrna_dge"));
    }

    public override IParticipantFinder GetFinder(string tumorDir, string platformDir)
    {
      return new FindParticipantMicroarray(FindSdrfFile(platformDir));
    }

    public override Func<string, bool> GetFilenameFilter()
    {
      return m => m.ToLower().EndsWith(".gene.tcga_level3.data.txt");
    }
  }
}
