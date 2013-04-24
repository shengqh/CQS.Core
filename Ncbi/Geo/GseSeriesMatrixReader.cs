using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using CQS.Sample;
using RCPA.Utils;

namespace CQS.Ncbi.Geo
{
  public class GseSeriesMatrixReader : IRawSampleInfoReader
  {
    /// <summary>
    /// Dictionary:key=!Sample_geo_accession
    ///            value=Dictionary:key=rownames(such as !Sample_title,!Sample_geo_accession, and so on)
    ///                             value=list of value(there will be multiple values such as !Sample_description
    /// </summary>
    /// <param name="dir"></param>
    /// <returns></returns>
    public Dictionary<string, Dictionary<string, List<string>>> ReadFromDirectory(string dir)
    {
      Dictionary<string, Dictionary<string, List<string>>> result = new Dictionary<string, Dictionary<string, List<string>>>();

      var files = GetMatrixFiles(dir);

      if (files.Length == 0)
      {
        throw new ArgumentException("Cannot find gse series matrix file in directory " + dir);
      }

      foreach (var file in files)
      {
        Dictionary<int, Dictionary<string, List<string>>> tmp = new Dictionary<int, Dictionary<string, List<string>>>();
        using (StreamReader sr = new StreamReader(file))
        {
          string line;

          while ((line = sr.ReadLine()) != null)
          {
            if (line.StartsWith(GsmConsts.SampleTitle))
            {
              var parts = line.Split('\t');
              for (int i = 1; i < parts.Length; i++)
              {
                if (!tmp.ContainsKey(i))
                {
                  tmp[i] = new Dictionary<string, List<string>>();
                }

                if (!tmp[i].ContainsKey(parts[0]))
                {
                  tmp[i][parts[0]] = new List<string>();
                }

                tmp[i][parts[0]].Add(parts[i]);
              }
              break;
            }
          }

          while ((line = sr.ReadLine()) != null)
          {
            if (line.StartsWith(GsmConsts.SeriesMatrixTableBegin))
            {
              break;
            }

            var parts = line.Split('\t');
            for (int i = 1; i < parts.Length; i++)
            {
              if (!tmp[i].ContainsKey(parts[0]))
              {
                tmp[i][parts[0]] = new List<string>();
              }

              if (parts[i].StartsWith("\"") && parts[i].EndsWith("\""))
              {
                tmp[i][parts[0]].Add(parts[i].Substring(1, parts[i].Length - 2));
              }
              else
              {
                tmp[i][parts[0]].Add(parts[i]);
              }
            }
          }
        }

        foreach (var t in tmp)
        {
          var key = t.Value[GsmConsts.SampleGeoAccession].First();
          result[key] = t.Value;
        }
      }

      return result;
    }

    public Dictionary<string, Dictionary<string, List<string>>> ReadDescriptionFromDirectory(string dir)
    {
      var map = ReadFromDirectory(dir);

      var result = new Dictionary<string, Dictionary<string, List<string>>>();

      foreach (var entry in map)
      {
        var valueMap = entry.Value;

        var lst = new List<string>();
        if (valueMap.ContainsKey("!Sample_description"))
        {
          lst.AddRange(valueMap["!Sample_description"]);
        }

        if (valueMap.ContainsKey("!Sample_characteristics_ch1"))
        {
          lst.AddRange(valueMap["!Sample_characteristics_ch1"]);
        }

        lst.RemoveAll(k => !k.Contains(':'));

        int lstSize = lst.Count;

        List<string> maplines = new List<string>();
        for (int i = 0; i < lstSize; i++)
        {
          if (lst[i].ToLower().Contains("question:"))
          {
            var key = lst[i].StringAfter(":");
            var value = lst[i + 1].StringAfter(":");
            maplines.Add(key + ":" + value);
            i++;
            continue;
          }

          string e, r;
          GetQuestion(lst[i], out e, out r);
          var ec = e.Count(m => m == '/');
          var rc = r.Count(m => m == '/');
          if (ec == rc && ec > 0)
          {
            //example: er/pr/her2 status:neg/neg/neg
            var eps = e.Split('/');
            var rps = r.Split('/');
            for (int j = 0; j < eps.Length; j++)
            {
              maplines.Add(eps[j] + ":" + rps[j]);
            }
            continue;
          }

          var count1 = lst[i].Count(k => k == ':');
          var count2 = lst[i].Count(k => k == ',' || k == ';');
          if (count1 > 1 && ((count2 == count1) || (count2 == count1 - 1)))
          {
            var parts = lst[i].Split(',', ';');
            foreach (var p in parts)
            {
              maplines.Add(p.Trim());
            }
          }
          else
          {
            maplines.Add(lst[i]);
          }
        }

        var curmap = new Dictionary<string, List<string>>();
        foreach (var l in maplines)
        {
          string e, r;
          GetQuestion(l, out e, out r);
          if (!curmap.ContainsKey(e))
          {
            curmap[e] = new List<string>();
          }
          curmap[e].Add(r);
        }

        result[entry.Key] = curmap;
      }

      return result;
    }

    public static void GetQuestion(string l, out string e, out string r)
    {
      int pos = 0;
      int backlet = 0;
      for (int i = 0; i < l.Length; i++)
      {
        if (l[i] == ':' && backlet == 0)
        {
          pos = i;
          break;
        }
        else if (l[i] == '(')
        {
          backlet++;
        }
        else if (l[i] == ')')
        {
          backlet--;
        }
      }

      if (pos == 0)
      {
        e = l.StringBefore(":").Trim();
        r = l.StringAfter(":").Trim();
      }
      else
      {
        e = l.Substring(0, pos).Trim();
        r = l.Substring(pos + 1).Trim();
      }
    }

    public bool IsReaderFor(string directory)
    {
      return GetMatrixFiles(directory).Length > 0;
    }

    private static string[] GetMatrixFiles(string directory)
    {
      return Directory.GetFiles(directory, "GSE*series_matrix*.txt");
    }

    public string SupportFor
    {
      get { return "GEO-GSE Dataset"; }
    }
  }
}
