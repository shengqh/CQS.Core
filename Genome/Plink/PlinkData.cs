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

    //From plink/input.cpp
    //// 00 hom
    //if (one==loc->allele1 && two==loc->allele1)
    //  {
    //    person->one[k]=false;
    //    person->two[k]=false;
    //  }
    //// 01 het
    //else if (one!=par::missing_genotype &&
    //         two!=par::missing_genotype &&
    //         one!=two)
    //  {
    //    person->one[k]=false;
    //    person->two[k]=true;
    //  }
    //// 11 hom
    //else if (one==loc->allele2 && two==loc->allele2)
    //  {
    //    person->one[k]=true;
    //    person->two[k]=true;
    //  }
    //// 10 missing
    //else if (one==par::missing_genotype || two==par::missing_genotype)
    //  {
    //    person->one[k]=true;
    //    person->two[k]=false;
    //  }

    /// <summary>
    /// First allele matrix of [locus, individual]
    /// </summary>
    public bool[,] IsOneMinor { get; set; }

    /// <summary>
    /// Second allele matrix of [locus, individual]
    /// </summary>
    public bool[,] IsTwoMinor { get; set; }

    public Dictionary<string, int> LocusMap { get; private set; }

    public Dictionary<string, int> IndividualMap { get; private set; }

    /// <summary>
    /// After read the Locus and Individual inforatation, AllocateData should be called to allocate momory
    /// </summary>
    public void AllocateDataMemory()
    {
      this.IsOneMinor = new bool[Locus.Count, Individual.Count];
      this.IsTwoMinor = new bool[Locus.Count, Individual.Count];
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
    /// <returns>geno type, 0:major-major, 1:major-minor, 2:minor-minor, 3:missing</returns>
    public int GenoType(int locus, int individual)
    {
      return GetGenoType(IsOneMinor[locus, individual], IsTwoMinor[locus, individual]);
    }

    /// <summary>
    /// Get the first allele of [locus, individual]
    /// </summary>
    /// <param name="locus">locus (zero based)</param>
    /// <param name="individual">individual (zero based)</param>
    /// <returns>first allele</returns>
    public string Allele1(int locus, int individual)
    {
      if (IsMissing(locus, individual))
      {
        return "0";
      }

      return IsOneMinor[locus, individual] ? Locus[locus].Allele2 : Locus[locus].Allele1;
    }

    public static bool IsMissing(bool isOneMinor, bool isTwoMinor)
    {
      return isOneMinor && !isTwoMinor;
    }

    public bool IsMissing(int locus, int individual)
    {
      return IsMissing(IsOneMinor[locus, individual], IsTwoMinor[locus, individual]);
    }

    /// <summary>
    /// Get the second allele of [locus, individual]
    /// </summary>
    /// <param name="locus">locus (zero based)</param>
    /// <param name="individual">individual (zero based)</param>
    /// <returns>second allele</returns>
    public string Allele2(int locus, int individual)
    {
      if (IsMissing(locus, individual))
      {
        return "0";
      }

      return IsTwoMinor[locus, individual] ? Locus[locus].Allele2 : Locus[locus].Allele1;
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

    public static int GetGenoType(bool isAllele1Minor, bool isAllele2Minor)
    {
      if (isAllele1Minor)
      {
        if (isAllele2Minor)
        {
          return 2;//minor-minor
        }
        else
        {
          return 3; //missing value
        }
      }
      else
      {
        if (isAllele2Minor)
        {
          return 1;//major-minor
        }
        else
        {
          return 0;//major-major
        }
      }
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
        sb.Append(GetGenoType(IsOneMinor[locus, j], IsTwoMinor[locus, j]));
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
