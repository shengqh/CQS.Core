using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CQS.Genome.Annotation
{
  public interface IAnnotationTsvExporter
  {
    string GetHeader();

    string GetValue(string chrom, long start, long end);
  }
}
