using System;
using System.Collections.Generic;
using System.Linq;

namespace CQS
{
  public class ExpressionData : ICloneable
  {
    public ExpressionData()
    {
      this.Values = new List<ExpressionValue>();
      this.SampleBarcode = string.Empty;
      this.IsLog2Value = false;
    }

    public string SampleBarcode { get; set; }

    public List<ExpressionValue> Values { get; private set; }

    /// <summary>
    /// Get gene names
    /// </summary>
    /// <returns>Gene name list</returns>
    public List<string> GetNames()
    {
      return (from v in Values
              select v.Name).ToList();
    }

    public object Clone()
    {
      var result = new ExpressionData();
      result.SampleBarcode = this.SampleBarcode;
      result.Values = new List<ExpressionValue>(this.Values);
      result.IsLog2Value = this.IsLog2Value;
      return result;
    }

    public bool IsLog2Value { get; set; }
  }

  public static class Level3ExpressionDataExtension
  {
    public static HashSet<string> GetSamples<T>(this IEnumerable<T> datas) where T : ExpressionData
    {
      return new HashSet<string>(from data in datas
                                 select data.SampleBarcode);
    }

    public static HashSet<string> GetGenes<T>(this IEnumerable<T> datas) where T : ExpressionData
    {
      return new HashSet<string>(from data in datas
                                 from v in data.Values
                                 select v.Name);
    }

    public static HashSet<string> GetCommonGenes<T>(this IEnumerable<T> datas) where T : ExpressionData
    {
      var lst = datas.ToList();
      if (lst.Count == 0)
      {
        return new HashSet<string>();
      }

      HashSet<string> result = new HashSet<string>(lst[0].GetNames());
      for (int i = 1; i < lst.Count; i++)
      {
        result.IntersectWith(lst[i].GetNames());
      }

      return result;
    }

    public static void SortBarcode<T>(this List<T> datas) where T : ExpressionData
    {
      datas.Sort((m1, m2) => m1.SampleBarcode.CompareTo(m2.SampleBarcode));
    }

    public static void CommonAndSortGenes<T>(this IEnumerable<T> datas) where T : ExpressionData
    {
      //get all gene names
      var genes = datas.GetCommonGenes();

      foreach (var data in datas)
      {
        //keep common gene only
        data.Values.RemoveAll(m => !genes.Contains(m.Name));

        //sort all
        data.Values.Sort((m1, m2) => m1.Name.CompareTo(m2.Name));
      }
    }

    public static void FillAndSortGenes<T>(this IEnumerable<T> datas) where T : ExpressionData
    {
      //get all gene names
      var genes = datas.GetGenes();

      foreach (var data in datas)
      {
        //get missed gene names
        var missgenes = genes.Except(from e in data.Values
                                     select e.Name);

        //add missed gene names
        foreach (var gene in missgenes)
        {
          data.Values.Add(new ExpressionValue(gene, double.NaN));
        }

        //sort all
        data.Values.Sort((m1, m2) => m1.Name.CompareTo(m2.Name));
      }
    }

    public static List<ExpressionData> CloneData(this IEnumerable<ExpressionData> source)
    {
      var result = new List<ExpressionData>();
      foreach (var data in source)
      {
        result.Add(data.Clone() as ExpressionData);
      }
      return result;
    }
  }
}
