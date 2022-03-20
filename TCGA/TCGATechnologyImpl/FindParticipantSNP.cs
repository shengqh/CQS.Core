namespace CQS.TCGA.TCGATechnologyImpl
{
  public class FindParticipantSNP : FindParticipantBySdrfFile
  {
    public FindParticipantSNP(string sdrfFile)
      : base(sdrfFile, "Derived Array Data File", "Comment [Aliquot Barcode]")
    { }
  }
}
