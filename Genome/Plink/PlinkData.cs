using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CQS.Genome.Plink
{
  public class PlinkData
  {
    /// <summary>
    /// All locus
    /// </summary>
    public List<PlinkLocus> Locus { get; set; }

    /// <summary>
    /// All individual (patient or sample)
    /// </summary>
    public List<PlinkIndividual> Individual { get; set; }

    /// <summary>
    /// First allele matrix of [locus, individual]
    /// </summary>
    public bool[,] One { get; set; }

    /// <summary>
    /// Second allele matrix of [locus, individual]
    /// </summary>
    public bool[,] Two { get; set; }

    public Dictionary<string, int> LocusMap { get; private set; }

    public Dictionary<string, int> IndividualMap { get; private set; }

    /// <summary>
    /// After read the Locus and Individual inforatation, AllocateData should be called to allocate momory
    /// </summary>
    public void AllocateDataMemory()
    {
      this.One = new bool[Locus.Count, Individual.Count];
      this.Two = new bool[Locus.Count, Individual.Count];
    }

    public void BuildMap()
    {
      this.LocusMap = new Dictionary<string, int>();
      for (int i = 0; i < Locus.Count; i++)
      {
        this.LocusMap[Locus[i].Name] = i;
      }

      this.IndividualMap = new Dictionary<string, int>();
      for (int i = 0; i < Individual.Count; i++)
      {
        this.IndividualMap[Individual[i].Iid] = i;
      }
    }

    /// <summary>
    /// Get the geno type of [locus, individual]
    /// </summary>
    /// <param name="locus">locus (zero based)</param>
    /// <param name="individual">individual (zero based)</param>
    /// <returns>geno type, 0:normal-normal, 1:normal-mutation, 2:mutation-mutation</returns>
    public int GenoType(int locus, int individual)
    {
      int g1 = One[locus, individual] ? 1 : 0;
      int g2 = Two[locus, individual] ? 1 : 0;
      return g1 + g2;
    }

    /// <summary>
    /// Get the first allele of [locus, individual]
    /// </summary>
    /// <param name="locus">locus (zero based)</param>
    /// <param name="individual">individual (zero based)</param>
    /// <returns>first allele</returns>
    public string Allele1(int locus, int individual)
    {
      return One[locus, individual] ? Locus[locus].Allele2 : Locus[locus].Allele1;
    }

    /// <summary>
    /// Get the second allele of [locus, individual]
    /// </summary>
    /// <param name="locus">locus (zero based)</param>
    /// <param name="individual">individual (zero based)</param>
    /// <returns>second allele</returns>
    public string Allele2(int locus, int individual)
    {
      return Two[locus, individual] ? Locus[locus].Allele2 : Locus[locus].Allele1;
    }

    private string DoAllele(int locus, string delimiter, Func<int, int, string> func)
    {
      StringBuilder sb = new StringBuilder();
      for (var j = 0; j < Individual.Count; j++)
      {
        if (j != 0)
        {
          sb.Append(delimiter);
        }
        sb.Append(func(locus, j));
      }
      return sb.ToString();
    }

    /// <summary>
    /// Get all first allele one for locus
    /// </summary>
    /// <param name="locus">locus (zero based)</param>
    /// <param name="delimiter">delimiter</param>
    /// <returns>string of first alleles</returns>
    public string LocusAllele1(int locus, string delimiter = "")
    {
      return DoAllele(locus, delimiter, Allele1);
    }

    /// <summary>
    /// Get all first alleles for locus
    /// </summary>
    /// <param name="locus">locus name</param>
    /// <param name="delimiter">delimiter</param>
    /// <returns>string of first alleles</returns>
    public string LocusAllele1(string locusName, string delimiter = "")
    {
      return LocusAllele1(this.LocusMap[locusName], delimiter);
    }

    /// <summary>
    /// Get all second alleles for locus
    /// </summary>
    /// <param name="locus">locus (zero based)</param>
    /// <param name="delimiter">delimiter</param>
    /// <returns>string of second alleles</returns>
    public string LocusAllele2(int locus, string delimiter = "")
    {
      return DoAllele(locus, delimiter, Allele2);
    }


    /// <summary>
    /// Get all second alleles for locus
    /// </summary>
    /// <param name="locus">locus name</param>
    /// <param name="delimiter">delimiter</param>
    /// <returns>string of second alleles</returns>
    public string LocusAllele2(string locusName, string delimiter = "")
    {
      return LocusAllele2(this.LocusMap[locusName], delimiter);
    }

    public static int GetGenoType(bool snp1, bool snp2)
    {
      if (snp1)
      {
        return 2;
      }

      if (snp2)
      {
        return 1;
      }

      return 0;
    }

    /// <summary>
    /// Get all geno type for locus
    /// </summary>
    /// <param name="locus">locus (zero based)</param>
    /// <param name="delimiter">delimiter</param>
    /// <returns>geno types</returns>
    public string LocusGenoType(int locus, string delimiter = "")
    {
      StringBuilder sb = new StringBuilder();
      for (var j = 0; j < Individual.Count; j++)
      {
        if (j != 0)
        {
          sb.Append(delimiter);
        }
        sb.Append(GetGenoType(One[locus, j], Two[locus, j]));
      }
      return sb.ToString();
    }
    /// <summary>
    /// Get all geno type for locus
    /// </summary>
    /// <param name="locusName">locusName</param>
    /// <param name="delimiter">delimiter</param>
    /// <returns>geno types</returns>
    public string LocusGenoType(string locusName, string delimiter = "")
    {
      return LocusGenoType(LocusMap[locusName], delimiter);
    }
  }
}
