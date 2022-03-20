namespace CQS.TCGA
{
  public class DataBuilder
  {
    public string TCGARoot { get; set; }
    public string TargetDirectory { get; set; }
    public string TargetFilePrefix { get; set; }
    public string[] Tumors { get; set; }
    public TCGASampleCode[] SampleCodes { get; set; }

    public DataBuilder()
    {
      SampleCodes = new TCGASampleCode[0];
    }

    protected string GetParticipant(string barcode)
    {
      return barcode.Substring(0, 12);
    }

    public void ExtractData(string datatype, string platform, bool outputCountDataOnly = false)
    {
      ExtractData(datatype, new string[] { platform }, outputCountDataOnly);
    }

    public void ExtractData(string datatype, string[] platforms, bool outputCountDataOnly = false)
    {
      TCGAUtils.ExtractData(TCGARoot, TargetDirectory, TargetFilePrefix, Tumors, datatype, platforms, SampleCodes, outputCountDataOnly);
    }
  }
}
