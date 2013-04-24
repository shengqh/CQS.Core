using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RCPA;
using System.Text.RegularExpressions;

namespace CQS.TCGA.TCGATechnologyImpl
{
  public class FindParticipantSNP : FindParticipantBySdrfFile
  {
    public FindParticipantSNP(string sdrfFile)
      : base(sdrfFile, "Derived Array Data File", "Comment [Aliquot Barcode]")
    { }
  }
}
