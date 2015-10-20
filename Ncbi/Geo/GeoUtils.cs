using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using CQS.Microarray.Affymatrix;

namespace CQS.Ncbi.Geo
{
  public static class GeoUtils
  {
    public static Dictionary<string, string> GetGsmNameFileMap(string directory)
    {
      Console.WriteLine(directory);
      var files = CelFile.GetCelFiles(directory);
      var groups = files.GroupBy(m => GetGsmName(m));
      foreach(var group in groups){
        if(group.Count() > 1){
          Console.WriteLine("{0} : {1}", group.Key, group.ToList().ConvertAll(m => Path.GetFileName(m)).Merge(", "));
        }
      }

      return groups.ToDictionary(m => m.Key, m => m.First());
    }

    public static string GetGsmName(string cel)
    {
      var result = Path.GetFileName(cel).ToUpper();

      if (result.Contains(".CEL"))
      {
        result = result.Substring(0, result.IndexOf(".CEL"));
      }

      if (result.StartsWith("GSM") && result.Contains("_"))
      {
        result = result.Substring(0, result.IndexOf("_"));
      }
      return result;
    }
  }
}
