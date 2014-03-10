using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CQS.Genome.Annotation
{
  public class FileData
  {
    public string File { get; set; }

    public string Normal { get; set; }

    public string Tumor { get; set; }

    public string[] Headers { get; set; }

    public int InfoIndex { get; set; }

    public int FormatIndex { get; set; }

    public int NormalIndex { get; set; }

    public int TumorIndex { get; set; }

    public Dictionary<string, FileDataValue> Data { get; set; }
  }
}
