using System;

namespace CQS.Genome.Cuffdiff
{
  public class CuffdiffItem
  {
    public string TestId { get; set; }
    public string GeneId { get; set; }
    public string Gene { get; set; }
    public string Locus { get; set; }
    public string Sample1 { get; set; }
    public string Sample2 { get; set; }
    public string Status { get; set; }
    public double Value1 { get; set; }
    public double Value2 { get; set; }
    public double Log2FoldChange { get; set; }
    public double TestStat { get; set; }
    public double PValue { get; set; }
    public double QValue { get; set; }
    public bool Significant { get; set; }
    public bool UpSample2
    {
      get
      {
        return this.Value2 > this.Value1;
      }
    }
    public string Line { get; set; }

    private static double ParseDouble(string value)
    {
      double result;
      if (double.TryParse(value, out result))
      {
        return result;
      }

      if (value.Equals("-inf"))
      {
        return double.NegativeInfinity;
      }

      if (value.Equals("inf"))
      {
        return double.PositiveInfinity;
      }

      if (value.Contains("nan"))
      {
        return double.NaN;
      }

      throw new ArgumentException("Unknow format of double " + value);
    }

    private static string FormatDouble(double value, double reference = 0)
    {
      if (double.IsNegativeInfinity(value))
      {
        return "-inf";
      }

      if (double.IsPositiveInfinity(value))
      {
        return "inf";
      }

      if (double.IsNaN(value))
      {
        if (double.IsNegativeInfinity(reference))
        {
          return "-nan";
        }
        else
        {
          return "nan";
        }
      }

      return value.ToString();
    }

    public string Log2FoldChangeString
    {
      get
      {
        return FormatDouble(this.Log2FoldChange, 0);
      }
    }

    public string TestStatString
    {
      get
      {
        return FormatDouble(this.TestStat, this.Log2FoldChange);
      }
    }

    public string SignificantString
    {
      get
      {
        return this.Significant ? "yes" : "no";
      }
    }

    public static CuffdiffItem Parse(string line)
    {
      var parts = line.Split('\t');
      var result = new CuffdiffItem();
      result.Line = line;
      result.TestId = parts[0];
      result.GeneId = parts[1];
      result.Gene = parts[2];
      result.Locus = parts[3];
      result.Sample1 = parts[4];
      result.Sample2 = parts[5];
      result.Status = parts[6];
      result.Value1 = ParseDouble(parts[7]);
      result.Value2 = ParseDouble(parts[8]);
      result.Log2FoldChange = ParseDouble(parts[9]);
      result.TestStat = ParseDouble(parts[10]);
      result.PValue = ParseDouble(parts[11]);
      result.QValue = ParseDouble(parts[12]);
      result.Significant = parts[13].Equals("yes");
      return result;
    }
  }
}
