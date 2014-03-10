using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
  }
}
