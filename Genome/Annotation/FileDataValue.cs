using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CQS.Genome.Annotation
{
  public class FileDataValue
  {
    public string Key { get; set; }

    public string[] Parts { get; set; }

    public string VNormal { get; set; }

    public string VTumor { get; set; }
  }
}
