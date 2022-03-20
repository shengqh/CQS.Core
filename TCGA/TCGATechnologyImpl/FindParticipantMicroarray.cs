namespace CQS.TCGA.TCGATechnologyImpl
{
  public class FindParticipantMicroarray : FindParticipantBySdrfFile
  {
    public FindParticipantMicroarray(string sdrfFile)
      : base(sdrfFile, "Derived Array Data Matrix File", "Normalization Name")
    { }
  }
}
