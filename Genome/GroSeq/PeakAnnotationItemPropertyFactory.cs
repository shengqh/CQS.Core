using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RCPA.Converter;
using RCPA;

namespace CQS.Genome.GroSeq
{
  public class PeakAnnotationItemPropertyFactory : PropertyConverterFactory<PeakAnnotationItem>
  {
    private PeakAnnotationItemPropertyFactory() { }

    public static PeakAnnotationItemPropertyFactory GetInstance()
    {
      PeakAnnotationItemPropertyFactory result = new PeakAnnotationItemPropertyFactory();
      result.RegisterConverter(new AliasPropertyConverter<PeakAnnotationItem>(m => m.StartsWith("PeakID"), (m, n) => m.PeakId = n, m => m.PeakId));
      result.RegisterConverter(new PropertyConverter<PeakAnnotationItem>("Chr", (m, n) => m.Chromosome = n, m => m.Chromosome));
      result.RegisterConverter(new PropertyConverter<PeakAnnotationItem>("Start", (m, n) => m.Start = long.Parse(n), m => m.Start.ToString()));
      result.RegisterConverter(new PropertyConverter<PeakAnnotationItem>("End", (m, n) => m.End = long.Parse(n), m => m.End.ToString()));
      result.RegisterConverter(new PropertyConverter<PeakAnnotationItem>("Strand", (m, n) => m.Strand = n[0], m => m.Strand.ToString()));
      result.RegisterConverter(new PropertyConverter<PeakAnnotationItem>("Annotation", (m, n) => m.Annotation = n, m => m.Annotation));
      result.RegisterConverter(new PropertyConverter<PeakAnnotationItem>("Detailed Annotation", (m, n) => m.DetailedAnnotation = n, m => m.DetailedAnnotation));

      return result;
    }

    public override PeakAnnotationItem Allocate()
    {
      return new PeakAnnotationItem();
    }
  }
}
