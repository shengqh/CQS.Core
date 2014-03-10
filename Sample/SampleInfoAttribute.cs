using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CQS.Sample
{
  [System.AttributeUsage(System.AttributeTargets.Property)]
  public class SampleInfoAttribute : System.Attribute
  {
    public SampleInfoAttribute()
    {
    }
  }

  [System.AttributeUsage(System.AttributeTargets.Property)]
  public class StatusInfoAttribute : System.Attribute
  {
    public StatusInfoAttribute()
    {
    }
  }
}
