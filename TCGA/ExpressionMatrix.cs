using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CQS.TCGA
{
  public class ExpressionMatrix
  {
    public string[] Colnames { get; set; }
    public string[] Rownames { get; set; }
    public double?[,] Values { get; set; }
  }
}
