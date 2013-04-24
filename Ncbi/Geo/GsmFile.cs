using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CQS.BreastCancer;
using System.IO;
using CQS.Sample;
using CQS.Microarray;

namespace CQS.Ncbi.Geo
{
  public class GsmFile : ISummaryFile
  {
    private static string GetString(string excel)
    {
      if (excel.StartsWith("\""))
      {
        return excel.Substring(1, excel.Length - 2);
      }
      else
      {
        return excel;
      }
    }

    public HashSet<string> ReadGenes(string fileName)
    {
      var result = new HashSet<string>();

      using (StreamReader sr = new StreamReader(fileName))
      {
        string line;
        while ((line = sr.ReadLine()) != null)
        {
          if (line.StartsWith("\"ID_REF\""))
          {
            break;
          }
        }

        while ((line = sr.ReadLine()) != null)
        {
          if (line.StartsWith("!"))
          {
            break;
          }
          result.Add(GetGeneName(line));
        }
      }
      return result;
    }

    public static List<SampleItem> ReadSamples(string fileName)
    {
      var result = new List<SampleItem>();

      using (StreamReader sr = new StreamReader(fileName))
      {
        string line;

        var geo = string.Empty;
        while ((line = sr.ReadLine()) != null)
        {
          if (line.Trim().Length > 0 && !line.StartsWith("!"))
          {
            break;
          }

          if (line.StartsWith(GsmConsts.GseAccession))
          {
            geo = GetValue(line);
            continue;
          }

          if (line.StartsWith(GsmConsts.SampleTitle))
          {
            var parts = line.Split('\t');
            for (int i = 1; i < parts.Length; i++)
            {
              result.Add(new SampleItem()
              {
                Dataset = geo,
                SampleTitle = GetString(parts[i])
              });
            }
            continue;
          }

          if (line.StartsWith(GsmConsts.SampleGeoAccession))
          {
            var gsms = line.Split('\t');
            for (int i = 1; i < gsms.Length; i++)
            {
              result[i - 1].Sample = GetString(gsms[i]);
            }
            continue;
          }

          if (line.StartsWith(GsmConsts.SampleSourceName))
          {
            var parts = line.Split('\t');
            for (int i = 1; i < parts.Length; i++)
            {
              result[i - 1].SourceName = GetString(parts[i]);
            }
            continue;
          }
        }
      }

      return result;
    }

    private static string GetValue(string line)
    {
      return GetString(line.Substring(line.IndexOf("\t")).Trim());
    }

    private static string GetGeneName(string line)
    {
      return GetString(line.Substring(0, line.IndexOf("\t")).Trim());
    }
  }
}
