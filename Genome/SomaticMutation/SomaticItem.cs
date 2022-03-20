namespace CQS.Genome.SomaticMutation
{
  public class SomaticItem : RCPA.Annotation
  {
    private string chrom;
    public string Chrom
    {
      get
      {
        return chrom;
      }
      set
      {
        if (value.Equals("M"))
        {
          chrom = "MT";
        }
        else
        {
          chrom = value;
        }
      }
    }

    public int StartPosition { get; set; }
    public string ID { get; set; }
    public string RefAllele { get; set; }
    public string AltAllele { get; set; }
    public string RefGeneFunc { get; set; }
    public string RefGeneName { get; set; }
    public string RefGeneExonicFunc { get; set; }
    public string RefGeneAAChange { get; set; }

    public double Score { get; set; }
    public string Sample { get; set; }
    //public string Filter { get; set; }
    public int NormalMajorCount { get; set; }
    public int NormalMinorCount { get; set; }
    public int TumorMajorCount { get; set; }
    public int TumorMinorCount { get; set; }
    public string LogisticScore { get; set; }
    public string LogisticStrand { get; set; }
    public string LogisticPosition { get; set; }
    public string LogisticGroupFdr { get; set; }

    public string Key
    {
      get
      {
        return GenomeUtils.GetKey(Chrom, StartPosition);
      }
    }
  }
}
