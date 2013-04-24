using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RCPA;
using System.Text.RegularExpressions;

namespace CQS.TCGA.TCGATechnologyImpl
{
  public class FindParticipantRnaSeq  : FindParticipantBySdrfFile
  {
    public FindParticipantRnaSeq(string sdrfFile)
      : base(sdrfFile, "Derived Data File", "Comment [TCGA Barcode]")
    { }
  }
}

