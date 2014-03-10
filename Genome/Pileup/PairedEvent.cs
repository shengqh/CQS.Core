namespace CQS.Genome.Pileup
{
  public class PairedEvent
  {
    public PairedEvent()
    {
      MajorEvent = string.Empty;
      MinorEvent = string.Empty;
    }

    public PairedEvent(string major, string minor)
    {
      MajorEvent = major;
      MinorEvent = minor;
    }

    public string MajorEvent { get; set; }

    public string MinorEvent { get; set; }
  }
}