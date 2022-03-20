namespace CQS.TCGA.TCGATechnologyImpl
{
  public class FindParticipantRnaSeq : FindParticipantBySdrfFile
  {
    public FindParticipantRnaSeq(string sdrfFile)
      : base(sdrfFile, "Derived Data File", "Comment [TCGA Barcode]")
    { }
  }
}

