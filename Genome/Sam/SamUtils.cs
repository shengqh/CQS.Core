using RCPA.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace CQS.Genome.Sam
{
  public static class SAMUtils
  {
    public static bool IsSortedByCoordinate(string filename)
    {
      return new BAMWindowReader(filename).ReadHeaders()[0].Contains("SO:coordinate");
    }

    public static List<string> GetChromosomes(string filename)
    {
      var headers = new BAMWindowReader(filename).ReadHeaders();
      return (from h in headers
              where h.StartsWith("@SQ")
              select h.StringAfter("SN:").StringBefore("\t")).ToList();
    }

    public static bool IsBAMFile(string filename)
    {
      return filename.ToLower().EndsWith(".bam");
    }

    public static void WriteFastq(this ISAMItem sam, System.IO.StreamWriter sw, bool posAsPaired = false)
    {
      if (posAsPaired)
      {
        sw.WriteLine(string.Format("@{0}/{1}", sam.Qname, sam.Pos));
      }
      else
      {
        sw.WriteLine("@" + sam.Qname);
      }
      sw.WriteLine(sam.Sequence);
      sw.WriteLine("+");
      sw.WriteLine(sam.Qual);
    }

    public static T Parse<T>(string[] parts) where T : SAMItemSlim, new()
    {
      var result = new T
      {
        Qname = parts[SAMFormatConst.QNAME_INDEX],
        Sequence = parts[SAMFormatConst.SEQ_INDEX],
        Qual = parts[SAMFormatConst.QUAL_INDEX]
      };

      return result;
    }

    public static T Parse<T>(string line) where T : SAMItemSlim, new()
    {
      var parts = line.Split('\t');
      return Parse<T>(parts);
    }

    public static void SortByName<T>(this List<T> list) where T : ISAMItem
    {
      list.Sort((m1, m2) => m1.Qname.CompareTo(m2.Qname));
    }

    public static void SortByNameAndScore<T>(this List<T> list, ISAMFormat format) where T : ISAMItem
    {
      list.Sort((m1, m2) =>
      {
        var result = m1.Qname.CompareTo(m2.Qname);
        if (result == 0)
        {
          result = format.CompareScore(m1.AlignmentScore, m2.AlignmentScore);
        }
        return result;
      });
    }

    public static void SortByNameAndScore<T>(this List<T> list, bool isLowerBetter) where T : ISAMItem
    {
      if (isLowerBetter)
      {
        list.Sort((m1, m2) =>
        {
          var result = m1.Qname.CompareTo(m2.Qname);
          if (result == 0)
          {
            result = m1.AlignmentScore.CompareTo(m2.AlignmentScore);
          }
          return result;
        });
      }
      else
      {
        list.Sort((m1, m2) =>
        {
          var result = m1.Qname.CompareTo(m2.Qname);
          if (result == 0)
          {
            result = -m1.AlignmentScore.CompareTo(m2.AlignmentScore);
          }
          return result;
        });
      }
    }

    private static Regex cigarReg = new Regex(@"(\d+)(\S)");

    /// <summary>
    /// Parsing cigar string to get the end point of the read
    /// </summary>
    /// <param name="start">start position in reference</param>
    /// <param name="cigar">cigar string</param>
    /// <returns>end position (included in the mapping region) in reference</returns>
    public static int ParseEnd(int start, string cigar)
    {
      var m = cigarReg.Match(cigar);
      string lastType = string.Empty;
      int lastDis = 0;
      while (m.Success)
      {
        lastDis = int.Parse(m.Groups[1].Value);
        lastType = m.Groups[2].Value;
        if (!lastType.Equals("I"))
        {
          start += lastDis;
        }

        m = m.NextMatch();
      }

      if (lastType.Equals("D"))
      {
        start -= lastDis;
      }

      return start - 1;
    }
  }
}
