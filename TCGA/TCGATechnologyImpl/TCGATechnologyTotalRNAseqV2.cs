using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RCPA;
using System.IO;
using System.Text.RegularExpressions;

namespace CQS.TCGA.TCGATechnologyImpl
{
  public class TCGATechnologyTotalRNAseqV2 : TCGATechnologyRNAseqV2
  {
    public override string NodeName
    {
      get
      {
        return "totalrnaseqv2";
      }
    }

  }
}
