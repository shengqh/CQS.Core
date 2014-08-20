using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CQS.Genome.Plink
{
  public class PlinkLocus
  {
    public static readonly string MISSING = "0";
    public static readonly char MISSING_CHAR = '0';

    public int Chr { get; set; }

    public string Name { get; set; }

    /// <summary>
    /// cM map positions
    /// </summary>
    public double Pos { get; set; }

    /// <summary>
    /// base-pair position
    /// </summary>
    public int Bp { get; set; }

    public char AlleleChar1 { get; set; }

    public char AlleleChar2 { get; set; }

    public string Allele1 { get; set; }

    public string Allele2 { get; set; }

    /// <summary>
    /// The number of platform containing this locus, whose alleles are not '0：0'
    /// </summary>
    public int ValidPlatformCount { get; set; }

    public string Platform { get; set; }

    /// <summary>
    /// Read locus from bim file of bed format
    /// </summary>
    /// <param name="fileName">bim file</param>
    /// <returns>list of PlinkLocus</returns>
    public static List<PlinkLocus> ReadFromBimFile(string fileName)
    {
      var result = new List<PlinkLocus>();

      using (var sr = new StreamReader(fileName))
      {
        string line;
        while ((line = sr.ReadLine()) != null)
        {
          line = line.Trim();
          if (string.IsNullOrEmpty(line))
          {
            continue;
          }

          var parts = line.Split('\t');
          if (string.IsNullOrEmpty(parts[1]))
          {
            continue;
          }

          var locus = new PlinkLocus();
          locus.Chr = int.Parse(parts[0]);
          locus.Name = parts[1];
          locus.Pos = double.Parse(parts[2]);
          locus.Bp = int.Parse(parts[3]);
          locus.Allele1 = parts[4];
          locus.Allele2 = parts[5];

          if (parts.Length > 7)
          {
            locus.Platform = parts[6];
            locus.ValidPlatformCount = int.Parse(parts[7]);
          }

          result.Add(locus);
        }
      }

      return result;
    }

    /// <summary>
    /// Read locus from map file of ped format
    /// </summary>
    /// <param name="fileName">map file</param>
    /// <returns>list of PlinkLocus</returns>
    public static List<PlinkLocus> ReadFromMapFile(string fileName)
    {
      var result = new List<PlinkLocus>();

      using (var sr = new StreamReader(fileName))
      {
        string line;
        while ((line = sr.ReadLine()) != null)
        {
          line = line.Trim();
          if (string.IsNullOrEmpty(line))
          {
            continue;
          }

          var parts = line.Split('\t');
          if (string.IsNullOrEmpty(parts[1]))
          {
            continue;
          }

          var locus = new PlinkLocus();
          locus.Chr = int.Parse(parts[0]);
          locus.Name = parts[1];
          locus.Pos = double.Parse(parts[3]);
          locus.Allele1 = MISSING;
          locus.Allele2 = MISSING;
          result.Add(locus);
        }
      }

      return result;
    }

    public static void WriteToFile(string fileName, List<PlinkLocus> items, bool exportPlatform = false)
    {
      using (var sw = new StreamWriter(fileName))
      {
        foreach (var locus in items)
        {
          sw.Write("{0}\t{1}\t{2}\t{3}\t{4}\t{5}",
                     locus.Chr,
                     locus.Name,
                     locus.Pos,
                     locus.Bp,
                     locus.Allele1,
                     locus.Allele2);
          if (exportPlatform)
          {
            sw.Write("\t{0}\t{1}", locus.Platform, locus.ValidPlatformCount);
          }
          sw.WriteLine();
        }
      }
    }

    public override string ToString()
    {
      return string.Format("{0}:{1}:{2}", Name, Allele1, Allele2);
    }
  }
}
