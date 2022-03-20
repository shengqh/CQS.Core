namespace CQS.Genome.Refseq
{
  public class GeneLocationItem
  {
    public string Chromosome { get; set; }

    public string Database { get; set; }

    public string ItemType { get; set; }

    public long TSS
    {
      get
      {
        if (Strand == '+')
        {
          return Start;
        }
        else
        {
          return End;
        }
      }
    }

    public long Start { get; set; }

    public long End { get; set; }

    public double Unknown1 { get; set; }

    public char Strand { get; set; }

    public char Unknown2 { get; set; }

    public string GeneId { get; set; }

    public string TranscriptId { get; set; }
  }
}
