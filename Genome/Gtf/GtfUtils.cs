using System;
using System.Collections.Generic;
using System.IO;

namespace CQS.Genome.Gtf
{
  public static class GtfUtils
  {
    public static Dictionary<string, string> GetGeneIdNameMap(string gtfFile)
    {
      var result = new Dictionary<string, string>();

      using (StreamReader sr = new StreamReader(gtfFile))
      {
        string line;
        while ((line = sr.ReadLine()) != null)
        {
          var parts = line.Split('\t');
          if (parts.Length < 9)
          {
            continue;
          }
          var attributes = parts[8];
          var geneid = attributes.StringAfter("gene_id");
          geneid = geneid.StringBefore(";").Trim();
          geneid = geneid.Substring(1, geneid.Length - 2);

          if (result.ContainsKey(geneid))
          {
            continue;
          }

          var genename = attributes.StringAfter("gene_name");
          genename = genename.StringBefore(";").Trim();
          genename = genename.Substring(1, genename.Length - 2);

          result[geneid] = genename;
        }
      }

      return result;
    }

    public static void CombineCoordinates(this List<GtfItem> gtfs)
    {
      for (int i = gtfs.Count - 1; i > 0; i--)
      {
        var gtfi = gtfs[i];
        for (int j = i - 1; j >= 0; j--)
        {
          var gtfj = gtfs[j];
          if (gtfi.Overlap(gtfj, 0))
          {
            gtfj.UnionWith(gtfi);
            gtfs.RemoveAt(i);
            break;
          }
        }
      }
    }
  }
}
