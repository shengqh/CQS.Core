using System.Collections.Generic;

namespace CQS.Genome.Annotation
{
  public class GenebankItem
  {
    public GenebankItem()
    {
      this.Accession = string.Empty;
      this.Features = new List<GenebankFeature>();
    }

    public string Accession { get; set; }

    public List<GenebankFeature> Features { get; set; }
  }
}
