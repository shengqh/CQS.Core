namespace CQS.Genome.Rnaediting
{
  public class RnaeditItem
  {
    /// <summary>
    /// The name of the chromosome (e.g. chr3, chrY, chr2_random) or scaffold (e.g. scaffold10671).
    /// </summary>
    public string Chrom { get; set; }

    /// <summary>
    /// Position in sequence (starting from ?)
    /// </summary>
    public long Coordinate { get; set; }

    /// <summary>
    /// Defines the strand - either '+' or '-'.
    /// </summary>
    public char Strand { get; set; }

    public char NucleotideInChromosome { get; set; }

    public char NucleotideInRNA { get; set; }

    public string Gene { get; set; }

    public char SeqReg { get; set; }

    public char ExReg { get; set; }

    public string Source { get; set; }

    public string PubmedId { get; set; }
  }
}
