using RCPA;
using System.Collections.Generic;

namespace CQS.Genome.GroSeq
{
  public class PeakAnnotationItem : IAnnotation
  {
    public string PeakId { get; set; }

    public string Chromosome { get; set; }

    public long Start { get; set; }

    public long End { get; set; }

    public char Strand { get; set; }

    public string Annotation { get; set; }

    public string DetailedAnnotation { get; set; }

    private Dictionary<string, object> _annotations;

    public bool HasAnnotations
    {
      get
      {
        return (_annotations != null) && (_annotations.Count > 0);
      }
    }

    public Dictionary<string, object> Annotations
    {
      get
      {
        if (null == _annotations)
        {
          _annotations = new Dictionary<string, object>();
        }
        return _annotations;
      }
    }
  }
}
