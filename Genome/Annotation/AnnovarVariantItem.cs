using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CQS.Genome.Annotation
{
  public class AnnovarVariantItem : IChromosomeRegion
  {
    public string Chrom { get; set; }

    public long ChromStart { get; set; }

    public long ChromEnd { get; set; }

    public long Length
    {
      get { return this.ChromEnd - this.ChromStart + 1; }
    }

    /// <summary>
    /// nonsynonymous SNV, synonymous SNV, frameshift insertion, frameshift deletion, nonframeshift insertion, nonframeshift deletion, frameshift block substitution, nonframshift block substitution
    /// </summary>
    public string VariantType { get; set; }

    /// <summary>
    /// contains the gene name, the transcript identifier and the sequence change in the corresponding transcript.
    /// </summary>
    public string VariantAnnotation { get; set; }
  }

  public static class AnnovarVariantItemExtension
  {
    public static Action<string, AnnovarVariantItem> EmptyFunc = (m, n) => { };
    public static Action<string, AnnovarVariantItem> VariantTypeFunc = (m, n) => n.VariantType = m;
    public static Action<string, AnnovarVariantItem> VariantAnnotationFunc = (m, n) => n.VariantAnnotation = m;
    public static Action<string, AnnovarVariantItem> ChromFunc = (m, n) => n.Chrom = m;
    public static Action<string, AnnovarVariantItem> ChromStartFunc = (m, n) => n.ChromStart = long.Parse(m);
    public static Action<string, AnnovarVariantItem> ChromEndFunc = (m, n) => n.ChromEnd = long.Parse(m);
  }
}
