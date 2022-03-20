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
