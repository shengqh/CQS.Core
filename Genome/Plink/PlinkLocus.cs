using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CQS.Genome.Plink
{
  public class PlinkLocus
  {
    public PlinkLocus()
    {
      this.MarkerId = string.Empty;
      this.Allele1 = string.Empty;
      this.Allele2 = string.Empty;
      this.Platform = string.Empty;
      this.FromImputation = false;
    }

    public static readonly string MISSING = "0";
    public static readonly char MISSING_CHAR = '0';

    public int Chromosome { get; set; }

    public string MarkerId { get; set; }

    /// <summary>
    /// base-pair position
    /// </summary>
    public double GeneticDistance { get; set; }

    /// <summary>
    /// cM map positions
    /// </summary>
    public int PhysicalPosition { get; set; }

    public char AlleleChar1 { get; set; }

    public char AlleleChar2 { get; set; }

    public string Allele1 { get; set; }

    public string Allele2 { get; set; }

    /// <summary>
    /// The number of platform containing this locus, whose alleles are not '0：0'
    /// </summary>
    public int ValidPlatformCount { get; set; }

    public string Platform { get; set; }

    public int TotalSample { get; set; }

    public int ValidSample { get; set; }

    public double Allele2Frequency { get; set; }

    public bool FromImputation { get; set; }

    /// <summary>
    /// Read locus from bim file of bed format
    /// </summary>
    /// <param name="fileName">bim file</param>
    /// <returns>list of PlinkLocus</returns>
    public static List<PlinkLocus> ReadFromBimFile(string fileName, bool hasPlatform = false, bool hasAllele2Freqency = false)
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
          locus.Chromosome = int.Parse(parts[0]);
          locus.MarkerId = parts[1];
          locus.GeneticDistance = double.Parse(parts[2]);
          locus.PhysicalPosition = int.Parse(parts[3]);
          locus.Allele1 = parts[4];
          locus.Allele2 = parts[5];

          var index = 6;
          if (hasPlatform)
          {
            locus.Platform = parts[index++];
            locus.ValidPlatformCount = int.Parse(parts[index++]);
          }

          if (hasAllele2Freqency)
          {
            locus.Allele2Frequency = double.Parse(parts[index++]);
            locus.TotalSample = int.Parse(parts[index++]);
            locus.ValidSample = int.Parse(parts[index++]);
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
          locus.Chromosome = int.Parse(parts[0]);
          locus.MarkerId = parts[1];
          locus.GeneticDistance = int.Parse(parts[2]);
          locus.PhysicalPosition = int.Parse(parts[3]);
          if (parts.Length >= 6)
          {
            locus.Allele1 = parts[4];
            locus.Allele2 = parts[5];
          }
          else
          {
            locus.Allele1 = MISSING;
            locus.Allele2 = MISSING;
          }
          result.Add(locus);
        }
      }

      return result;
    }

    public static void WriteToFile(string fileName, List<PlinkLocus> items, bool exportPlatform = false, bool exportAllele2Freqency = false)
    {
      using (var sw = new StreamWriter(fileName))
      {
        foreach (var locus in items)
        {
          sw.Write("{0}\t{1}\t{2}\t{3}\t{4}\t{5}",
                     locus.Chromosome,
                     locus.MarkerId,
                     locus.GeneticDistance,
                     locus.PhysicalPosition,
                     locus.Allele1,
                     locus.Allele2);
          if (exportPlatform)
          {
            sw.Write("\t{0}\t{1}", locus.Platform, locus.ValidPlatformCount);
          }

          if (exportAllele2Freqency)
          {
            sw.Write("\t{0:0.000}\t{1}\t{2}", locus.Allele2Frequency, locus.TotalSample, locus.ValidSample);
          }

          sw.WriteLine();
        }
      }
    }

    public override string ToString()
    {
      return string.Format("{0}:{1}:{2}", MarkerId, Allele1, Allele2);
    }
  }
}
