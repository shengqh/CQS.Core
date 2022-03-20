namespace CQS.Genome.Vcf
{
  /// <summary>
  /// Vcf format, 1 based, close boundary
  /// </summary>
  public class VcfItem
  {
    public string Seqname { get; set; }

    public long Start { get; set; }

    public long End { get; set; }

    public string RefAllele { get; set; }

    public string AltAllele { get; set; }

    public string Line { get; set; }
  }
}
