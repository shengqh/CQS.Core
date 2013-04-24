using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CQS.Sample
{
  [System.AttributeUsage(System.AttributeTargets.Property)]
  public class SampleInfo : System.Attribute
  {
    public SampleInfo()
    {
    }
  }

  [System.AttributeUsage(System.AttributeTargets.Property)]
  public class StatusInfo : System.Attribute
  {
    public StatusInfo()
    {
    }
  }
}
