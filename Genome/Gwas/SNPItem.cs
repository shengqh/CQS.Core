using RCPA.Seq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CQS.Genome.Gwas
{
  public class Alleles
  {
    public string Allele1 { get; set; }
    public string Allele2 { get; set; }
    public double Allele2Frequency { get; set; }
    public bool NeedFlip { get; set; }

    public void Flip()
    {
      var t = Allele1;
      Allele1 = Allele2;
      Allele2 = t;
      Allele2Frequency = 1 - Allele2Frequency;
    }
  }

  public class SNPItem
  {
    public string Name { get; set; }
    public int Chrom { get; set; }
    public int Position { get; set; }
    public char RefChar { get; set; }
    public char Allele1 { get; set; }
    public string Allele2 { get; set; }

    public string Dataset { get; set; }
    public Dictionary<string, Alleles> Platforms { get; set; }

    public int Genotype0 { get; set; }
    public int Genotype1 { get; set; }
    public int Genotype2 { get; set; }
    public int GenotypeUnknown { get; set; }

    public char DbsnpRefAllele { get; set; }
    public char DbsnpAltAllele { get; set; }
    public bool DbsnpIsReversed { get; set; }

    public char G1000Allele1 { get; set; }
    public char G1000Allele2 { get; set; }
    public double G1000Allele2Frequency { get; set; }

    public bool IsCompementary()
    {
      var comp2 = SequenceUtils.GetComplementAllele(this.G1000Allele2);
      return this.G1000Allele1 == comp2;
    }

    public SNPItem()
    {
      Name = string.Empty;
      Chrom = 0;
      Position = 0;
      Dataset = string.Empty;
      Platforms = new Dictionary<string, Alleles>();
      RefChar = ' ';
      Genotype0 = 0;
      Genotype1 = 0;
      Genotype2 = 0;
      GenotypeUnknown = 0;

      DbsnpRefAllele = ' ';
      DbsnpAltAllele = ' ';
      G1000Allele1 = ' ';
      G1000Allele2 = ' ';
    }

    public static List<SNPItem> ReadFromFile(string fileName)
    {
      var result = new List<SNPItem>();
      using (var sr = new StreamReader(fileName))
      {
        var line = sr.ReadLine();
        var hasFlip = line.Contains("_AlleleNeedFlip");

        var platforms = line.Split('\t').ToList().ConvertAll(m => m.StringBefore("_Allele"));
        var step = hasFlip ? 3 : 2;
        while ((line = sr.ReadLine()) != null)
        {
          var parts = line.Split('\t');
          //sw.WriteLine("Dbsnp_ID\tCHROM\tPOS\tDbsnp_RefAllele\tDbsnp_AltAllele\tDbsnp_IsReversed\t1000G_REF\t1000G_ALT\t1000G_MAF\tDataset\t{0}",

          var item = new SNPItem()
          {
            Name = parts[0],
            Chrom = int.Parse(parts[1]),
            Position = int.Parse(parts[2]),
            Allele1 = parts[3][0],
            Allele2 = parts[4],
            DbsnpIsReversed = bool.Parse(parts[5]),
            G1000Allele1 = parts[6][0],
            G1000Allele2 = parts[7][0],
            G1000Allele2Frequency = double.Parse(parts[8]),
            Dataset = parts[9]
          };

          for (int i = 10; i < parts.Length; i += step)
          {
            if (string.IsNullOrEmpty(parts[i]))
            {
              continue;
            }

            var alleles = parts[i].Split(':');
            var aitem = new Alleles()
            {
              Allele1 = alleles[0],
              Allele2 = alleles[1]
            };
            aitem.Allele2Frequency = double.Parse(parts[i + 1]);
            if (hasFlip)
            {
              aitem.NeedFlip = bool.Parse(parts[i + 2]);
            }
            item.Platforms[platforms[i]] = aitem;
          }

          result.Add(item);
        }
      }
      return result;
    }

    public override string ToString()
    {
      return string.Format("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}\t{7}\t{8}", Chrom, Position, Allele1, Allele2, RefChar, Name, DbsnpIsReversed, Dataset,
            (from pl in Platforms
             let value = string.Format("{0}:{1}\t{2:0.000}\t{3}", pl.Value.Allele1, pl.Value.Allele2, pl.Value.Allele2Frequency, pl.Value.NeedFlip)
             select value).Merge("\t"));
    }

    public static void WriteToFile(string fileName, List<SNPItem> items)
    {
      var platforms = (from item in items
                       from pl in item.Platforms.Keys
                       select pl).Distinct().OrderBy(m => m).ToList();

      using (var sw = new StreamWriter(fileName))
      {
        sw.WriteLine("Dbsnp_ID\tCHROM\tPOS\tDbsnp_RefAllele\tDbsnp_AltAllele\tDbsnp_IsReversed\t1000G_REF\t1000G_ALT\t1000G_MAF\tDataset\t{0}",
          (from p in platforms select string.Format("{0}_Alleles\t{0}_Allele2Frequency\t{0}_AlleleNeedFlip", p)).Merge("\t"));

        foreach (var v in items)
        {
          sw.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}\t{7}\t{8}\t{9}\t{10}", v.Name, v.Chrom, v.Position, v.Allele1, v.Allele2, v.DbsnpIsReversed, v.G1000Allele1, v.G1000Allele2, v.G1000Allele2Frequency, v.Dataset,
            (from pl in platforms
             let value = v.Platforms.ContainsKey(pl) ?
               string.Format("{0}:{1}\t{2:0.000}\t{3}", v.Platforms[pl].Allele1, v.Platforms[pl].Allele2, v.Platforms[pl].Allele2Frequency, v.Platforms[pl].NeedFlip) : "\t\t"
             select value).Merge("\t"));
        }
      }
    }

    public bool IsSourceAllelesMatchedWithG1000()
    {
      if (this.G1000Allele1 == ' ')
      {
        return false;
      }

      var comp1 = SequenceUtils.GetComplementAllele(this.G1000Allele1);
      var comp2 = SequenceUtils.GetComplementAllele(this.G1000Allele2);

      var plat1 = Allele1;
      var plat2 = Allele2[0];

      if (plat1 == this.G1000Allele1 && plat2 == this.G1000Allele2)
      {
        return true;
      }

      if (plat1 == this.G1000Allele2 && plat2 == this.G1000Allele1)
      {
        return true;
      }

      if (plat1 == comp1 && plat2 == comp2)
      {
        return true;
      }

      if (plat1 == comp2 && plat2 == comp1)
      {
        return true;
      }

      return false;
    }

    public bool IsPlatformAllelesMatchedWithDatabase()
    {
      if (this.G1000Allele1 == ' ')
      {
        return false;
      }

      var comp1 = SequenceUtils.GetComplementAllele(this.G1000Allele1);
      var comp2 = SequenceUtils.GetComplementAllele(this.G1000Allele2);

      foreach (var plat in Platforms.Values)
      {
        var plat1 = plat.Allele1.First();
        var plat2 = plat.Allele2.First();

        if (plat1 == this.G1000Allele1 && plat2 == this.G1000Allele2)
        {
          continue;
        }

        if (plat1 == this.G1000Allele2 && plat2 == this.G1000Allele1)
        {
          continue;
        }

        if (plat1 == comp1 && plat2 == comp2)
        {
          continue;
        }

        if (plat1 == comp2 && plat2 == comp1)
        {
          continue;
        }

        return false;
      }

      return true;
    }

    /// <summary>
    /// Comparing allele1 and allele2 with dbsnp reference allele to suggest the action for adjustment.
    /// </summary>
    /// <returns></returns>
    public StrandAction SuggestAction()
    {
      StrandAction result;
      if (this.Allele2[0] == this.DbsnpRefAllele)
      {
        result = StrandAction.None;
      }
      else if (this.Allele1 == this.DbsnpRefAllele)
      {
        result = StrandAction.Switch;
      }
      else if (SequenceUtils.GetComplementAllele(this.Allele2[0]) == this.DbsnpRefAllele)
      {
        result = StrandAction.Flip;
      }
      else if (SequenceUtils.GetComplementAllele(this.Allele1) == this.DbsnpRefAllele)
      {
        result = StrandAction.FlipSwitch;
      }
      else
      {
        result = StrandAction.Unknown;
      }
      return result;
    }
  }
}
