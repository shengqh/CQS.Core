using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace CQS.Ncbi.Geo
{
  public static class GeoUtils
  {
    public static HashSet<string> GetGsmNames(string directory)
    {
      return new HashSet<string>(from cel in Directory.GetFiles(directory, "*.CEL")
                                 select GeoUtils.GetGsmName(cel));
    }

    public static string GetGsmName(string cel)
    {
      var result = Path.GetFileName(cel).ToLower();

      if (result.Contains(".cel"))
      {
        result = result.Substring(0, result.IndexOf(".cel"));
      }

      if (result.StartsWith("gsm") && result.Contains("_"))
      {
        result = result.Substring(0, result.IndexOf("_"));
      }
      return result;
    }
  }
}
