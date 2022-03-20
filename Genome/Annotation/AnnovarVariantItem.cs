using System;

namespace CQS.Genome.Annotation
{
  public class AnnovarVariantItem : SequenceRegion
  {
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
    public static Action<string, AnnovarVariantItem> ChromFunc = (m, n) => n.Seqname = m;
    public static Action<string, AnnovarVariantItem> ChromStartFunc = (m, n) => n.Start = long.Parse(m);
    public static Action<string, AnnovarVariantItem> ChromEndFunc = (m, n) => n.End = long.Parse(m);
  }
}
