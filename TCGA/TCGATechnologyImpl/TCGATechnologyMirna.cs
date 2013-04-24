using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RCPA;
using CQS.TCGA.Microarray;

namespace CQS.TCGA.TCGATechnologyImpl
{
  public class TCGATechnologyMirna : TCGATechnologyMicroarray
  {
    public override string NodeName
    {
      get
      {
        return "mirna";
      }
    }
  }
}
