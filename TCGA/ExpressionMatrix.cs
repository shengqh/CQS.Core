namespace CQS.TCGA
{
  public class ExpressionMatrix
  {
    public string[] Colnames { get; set; }
    public string[] Rownames { get; set; }
    public double?[,] Values { get; set; }
  }
}
