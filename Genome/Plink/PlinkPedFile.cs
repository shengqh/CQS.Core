using RCPA;
using RCPA.Gui;
using RCPA.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CQS.Genome.Plink
{
  public class PlinkPedFile : ProgressClass, IFileFormat<PlinkData>
  {
    private bool _withIndel;

    public PlinkPedFile(bool withIndel = false)
    {
      this._withIndel = withIndel;
    }

    public PlinkData ReadFromFile(string fileName)
    {
      Progress.SetMessage("Reading plink data from " + fileName + "...");
      PlinkData result;
      if (_withIndel)
      {
        result = ReadFromFileWithInDel(fileName);
      }
      else
      {
        result = ReadFromFileWithoutIndel(fileName);
      }
      result.BuildMap();
      return result;
    }

    private PlinkData ReadFromFileWithoutIndel(string fileName)
    {
      var result = ReadLocus(fileName);

      result.Individual = PlinkIndividual.ReadFromFile(fileName);
      result.AllocateDataMemory();

      var allele1 = new char[result.Locus.Count, result.Individual.Count];
      var allele2 = new char[result.Locus.Count, result.Individual.Count];

      int individual = -1;
      //reading data
      using (var sr = new StreamReader(fileName))
      {
        string line;
        while ((line = sr.ReadLine()) != null)
        {
          individual++;
          var parts = line.Split(' ');
          for (int snp = 0; snp < result.Locus.Count; snp++)
          {
            var locus = result.Locus[snp];
            var pos = 6 + snp * 2;
            var a1 = parts[pos];
            var a2 = parts[pos + 1];
            allele1[snp, individual] = a1[0];
            allele2[snp, individual] = a2[0];
          }
        }
      }

      bool alleleAssigned = HasAlleleAssigned(result);
      for (int locus = 0; locus < result.Locus.Count; locus++)
      {
        if (!alleleAssigned)
        {
          var count = new Dictionary<char, int>();
          bool bFound = false;
          for (int ind = 0; ind < result.Individual.Count; ind++)
          {
            var a1 = allele1[locus, ind];
            if (a1 == PlinkLocus.MISSING_CHAR)
            {
              continue;
            }
            var a2 = allele2[locus, ind];
            if (a2 == PlinkLocus.MISSING_CHAR)
            {
              continue;
            }

            if (a1 != a2)
            {
              result.Locus[locus].AlleleChar1 = a1;
              result.Locus[locus].AlleleChar2 = a2;
              bFound = true;
              break;
            }

            int v;
            if (count.TryGetValue(a1, out v))
            {
              count[a1] = v + 1;
            }
            else
            {
              count[a1] = 1;
            }
          }

          if (!bFound)
          {
            var orderedCount = count.ToList().OrderByDescending(m => m.Value).ToList();
            if (orderedCount.Count == 0)
            {
              continue;
            }

            if (orderedCount.Count == 1)
            {
              result.Locus[locus].AlleleChar1 = orderedCount[0].Key;
              result.Locus[locus].AlleleChar2 = orderedCount[0].Key;
              continue;
            }

            if (orderedCount.Count == 2)
            {
              result.Locus[locus].AlleleChar1 = orderedCount[0].Key;
              result.Locus[locus].AlleleChar2 = orderedCount[1].Key;
              continue;
            }

            throw new Exception(string.Format("There are more than 3 alleles for locus {0} : {1}", result.Locus[locus].MarkerId, (from c in orderedCount select c.Key.ToString()).Merge(", ")));
          }

          result.Locus.ForEach(m =>
          {
            m.Allele1 = m.AlleleChar1.ToString();
            m.Allele2 = m.AlleleChar2.ToString();
          });
        }
        else
        {
          result.Locus.ForEach(m =>
          {
            m.AlleleChar1 = m.Allele1[0];
            m.AlleleChar2 = m.Allele2[0];
          });
        }

        var l1 = result.Locus[locus].AlleleChar1;
        //assign value
        for (int ind = 0; ind < result.Individual.Count; ind++)
        {
          var a1 = allele1[locus, ind];
          var a2 = allele2[locus, ind];
          if (a1 == PlinkLocus.MISSING_CHAR || a2 == PlinkLocus.MISSING_CHAR)
          {
            result.IsHaplotype1Allele2[locus, ind] = true;
            result.IsHaplotype2Allele2[locus, ind] = false;
            continue;
          }

          result.IsHaplotype1Allele2[locus, ind] = a1 != l1;
          result.IsHaplotype2Allele2[locus, ind] = a2 != l1;
        }
      }

      allele1 = null;
      allele2 = null;

      return result;
    }

    private static bool HasAlleleAssigned(PlinkData result)
    {
      return result.Locus.Any(m => !m.Allele1.Equals(PlinkLocus.MISSING));
    }

    private static PlinkData ReadLocus(string fileName)
    {
      var result = new PlinkData();
      var tmapFile = FileUtils.ChangeExtension(fileName, ".tmap");
      if (File.Exists(tmapFile))
      {
        result.Locus = PlinkLocus.ReadFromMapFile(tmapFile);
        return result;
      }

      var mapFile = FileUtils.ChangeExtension(fileName, ".map");
      if (File.Exists(mapFile))
      {
        result.Locus = PlinkLocus.ReadFromMapFile(mapFile);
        return result;
      }

      throw new FileNotFoundException("File not found: " + mapFile);
    }

    private PlinkData ReadFromFileWithInDel(string fileName)
    {
      var result = ReadLocus(fileName);

      result.Individual = PlinkIndividual.ReadFromFile(fileName);
      result.AllocateDataMemory();

      var namemap = new Dictionary<char, string>();

      var allele1 = new char[result.Locus.Count, result.Individual.Count];
      var allele2 = new char[result.Locus.Count, result.Individual.Count];

      int individual = -1;
      using (var sr = new StreamReader(fileName))
      {
        string line;
        while ((line = sr.ReadLine()) != null)
        {
          individual++;
          var parts = line.Split(' ');
          for (int snp = 0; snp < result.Locus.Count; snp++)
          {
            var locus = result.Locus[snp];
            var pos = 6 + snp * 2;
            var a1 = parts[pos];
            var a2 = parts[pos + 1];
            allele1[snp, individual] = a1[0];
            allele2[snp, individual] = a2[0];

            namemap[a1[0]] = a1;
            namemap[a2[0]] = a2;
          }
        }
      }

      bool alleleAssigned = HasAlleleAssigned(result);
      for (int locus = 0; locus < result.Locus.Count; locus++)
      {
        if (!alleleAssigned)
        {
          var count = new Dictionary<char, int>();
          bool bFound = false;
          for (int ind = 0; ind < result.Individual.Count; ind++)
          {
            var a1 = allele1[locus, ind];
            if (a1 == PlinkLocus.MISSING_CHAR)
            {
              continue;
            }
            var a2 = allele2[locus, ind];
            if (a2 == PlinkLocus.MISSING_CHAR)
            {
              continue;
            }

            if (a1 != a2)
            {
              result.Locus[locus].Allele1 = namemap[a1];
              result.Locus[locus].Allele2 = namemap[a2];
              bFound = true;
              break;
            }

            int v;
            if (count.TryGetValue(a1, out v))
            {
              count[a1] = v + 1;
            }
            else
            {
              count[a1] = 1;
            }
          }

          if (!bFound)
          {
            var orderedCount = count.ToList().OrderByDescending(m => m.Value).ToList();
            if (orderedCount.Count == 0)
            {
              continue;
            }

            if (orderedCount.Count == 1)
            {
              result.Locus[locus].Allele1 = namemap[orderedCount[0].Key];
              result.Locus[locus].Allele2 = namemap[orderedCount[0].Key];
              continue;
            }

            if (orderedCount.Count == 2)
            {
              result.Locus[locus].Allele1 = namemap[orderedCount[0].Key];
              result.Locus[locus].Allele2 = namemap[orderedCount[1].Key];
              continue;
            }

            throw new Exception(string.Format("There are more than 3 alleles for locus {0} : {1}", result.Locus[locus].MarkerId, (from c in orderedCount select namemap[c.Key]).Merge(", ")));
          }
        }

        var l1 = result.Locus[locus].Allele1[0];
        //assign value
        for (int ind = 0; ind < result.Individual.Count; ind++)
        {
          var a1 = allele1[locus, ind];
          var a2 = allele2[locus, ind];
          if (a1 == PlinkLocus.MISSING_CHAR || a2 == PlinkLocus.MISSING_CHAR)
          {
            result.IsHaplotype1Allele2[locus, ind] = true;
            result.IsHaplotype2Allele2[locus, ind] = false;
            continue;
          }

          result.IsHaplotype1Allele2[locus, ind] = a1 != l1;
          result.IsHaplotype2Allele2[locus, ind] = a2 != l1;
        }
      }

      allele1 = null;
      allele2 = null;

      return result;
    }

    public void WriteToFile(string fileName, PlinkData data)
    {
      throw new NotImplementedException();
    }
  }
}
