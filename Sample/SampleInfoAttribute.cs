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
