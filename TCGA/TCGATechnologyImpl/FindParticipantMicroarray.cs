
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RCPA;
using System.Text.RegularExpressions;

namespace CQS.TCGA.TCGATechnologyImpl
{
  public class FindParticipantMicroarray : FindParticipantBySdrfFile
  {
    public FindParticipantMicroarray(string sdrfFile)
      : base(sdrfFile, "Derived Array Data Matrix File", "Normalization Name")
    { }
  }
}
