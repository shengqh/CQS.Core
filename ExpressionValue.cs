using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CQS
{
  public class ExpressionValue
  {
    public ExpressionValue()
    {
      Name = string.Empty;
      Value = double.NaN;
    }

    public ExpressionValue(string name, double value)
    {
      this.Name = name;
      this.Value = value;
    }

    public string Name { get; set; }
    public double Value { get; set; }
  }
}
