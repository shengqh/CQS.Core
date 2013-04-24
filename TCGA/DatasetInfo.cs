using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RCPA;

namespace CQS.TCGA
{
  public class DatasetInfo
  {
    public IFileReader<ExpressionData> Reader { get; set; }

    public void KeepSamples(HashSet<string> samples)
    {
      var keys = BarInfoListMap.Keys.ToList();
      foreach (var key in keys)
      {
        if (!samples.Contains(key))
        {
          BarInfoListMap.Remove(key);
        }
      }
    }

    public Dictionary<string, List<BarInfo>> BarInfoListMap { get; set; }

    public string Name { get; set; }

    public List<string> GetBarCodes()
    {
      return (from l in BarInfoListMap.Values
              from bi in l
              select bi.BarCode).ToList();
    }

    public void RemoveAll(TCGASampleType stype)
    {
      var keys = BarInfoListMap.Keys.ToList();
      foreach (var key in keys)
      {
        if (TCGAUtils.GetSampleType(key) == stype)
        {
          BarInfoListMap.Remove(key);
        }
      }
    }
  }

  public static class DatasetInfoExtension
  {
    public static HashSet<string> GetCommonSamples(this IEnumerable<DatasetInfo> diList)
    {
      bool bFirst = true;
      HashSet<string> result = new HashSet<string>();
      foreach (var di in diList)
      {
        if (bFirst)
        {
          result = new HashSet<string>(di.BarInfoListMap.Keys);
          bFirst = false;
        }
        else
        {
          result.IntersectWith(di.BarInfoListMap.Keys);
        }
      }
      return result;
    }
  }
}
