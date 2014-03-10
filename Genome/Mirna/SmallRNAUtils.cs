using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace CQS.Genome.Mirna
{
  public static class SmallRNAUtils
  {
    private static Regex trnaReg = new Regex(@"(?:chr){0,1}([^.]+)\.tRNA(\d+)");
    private static Regex mirnaReg = new Regex(@"([\d]+)(\w{0,1})([\d-]*)");
    /// <summary>
    /// Sort trna names by chromosome and id
    /// </summary>
    /// <param name="names">Name list</param>
    public static void SortNames(List<string> names)
    {
      if (names.All(m => trnaReg.Match(m).Success))
      {
        Console.WriteLine("sort by chr and name ...");
        int chrname;
        var namesmap = (from n in names
                        let m = trnaReg.Match(n)
                        let chr = m.Groups[1].Value
                        let chrIsNum = int.TryParse(chr, out chrname)
                        let id = int.Parse(m.Groups[2].Value)
                        select new { Name = n, Chr = chr, ChrIsNum = chrIsNum, Id = id }).ToDictionary(m => m.Name);

        names.Sort((m1, m2) =>
        {
          var n1 = namesmap[m1];
          var n2 = namesmap[m2];
          if (n1.ChrIsNum && n2.ChrIsNum)
          {
            var result = int.Parse(n1.Chr).CompareTo(int.Parse(n2.Chr));
            if (result == 0)
            {
              result = n1.Id.CompareTo(n2.Id);
            }
            return result;
          }
          else if (n1.ChrIsNum)
          {
            return -1;
          }
          else if (n2.ChrIsNum)
          {
            return 1;
          }
          else
          {
            var result = n1.Chr.CompareTo(n2.Chr);
            if (result == 0)
            {
              result = n1.Id.CompareTo(n2.Id);
            }
            return result;
          }
        });
      }
      else if (names.All(m => mirnaReg.Match(m).Success))
      {
        Console.WriteLine("sort by name ...");
        var namesmap = (from n in names
                        let m = mirnaReg.Match(n)
                        let v1 = int.Parse(m.Groups[1].Value)
                        let v2 = m.Groups[2].Value
                        let v3 = m.Groups[3].Value
                        select new { Name = n, V1 = v1, V2 = v2, V3 = v3 }).ToDictionary(m => m.Name);

        names.Sort((m1, m2) =>
        {
          var n1 = namesmap[m1];
          var n2 = namesmap[m2];
          var result = n1.V1.CompareTo(n2.V1);
          if (result == 0)
          {
            result = n1.V2.CompareTo(n2.V2);
            if (result == 0)
            {
              result = n1.V3.CompareTo(n2.V3);
            }
          }
          return result;
        });
      }
      else
      {
        names.Sort();
      }
    }
  }
}
