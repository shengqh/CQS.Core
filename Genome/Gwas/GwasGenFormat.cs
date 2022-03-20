using CQS.Genome.Plink;
using RCPA;
using System.Collections.Generic;
using System.IO;

namespace CQS.Genome.Gwas
{
  public class GwasGenFormat : IFileFormat<PlinkData>
  {
    public PlinkData ReadFromFile(string fileName)
    {
      var result = new PlinkData();
      var samplefile = Path.ChangeExtension(fileName, ".sample");
      if (File.Exists(samplefile))
      {
        result.Individual = new GwasSampleFormat().ReadFromFile(samplefile);
      }
      else
      {
        using (var sr = new StreamReader(fileName))
        {
          string line = sr.ReadLine();
          var parts = line.Split(' ');
          var individualCount = (parts.Length - 5) / 3;
          result.Individual = new List<PlinkIndividual>();
          for (int i = 0; i < individualCount; i++)
          {
            result.Individual.Add(new PlinkIndividual());
          }
        }
      }

      using (var sr = new StreamReader(fileName))
      {
        string line;
        while ((line = sr.ReadLine()) != null)
        {
          if (!string.IsNullOrEmpty(line))
          {
            var locus = new PlinkLocus();
            result.Locus.Add(locus);
          }
        }
      }

      result.AllocateDataMemory();

      using (var sr = new StreamReader(fileName))
      {
        string line;
        int locusIndex = -1;
        while ((line = sr.ReadLine()) != null)
        {
          if (!string.IsNullOrEmpty(line))
          {
            locusIndex++;
            var locus = result.Locus[locusIndex];
            var parts = line.Split(' ');
            locus.Chromosome = int.Parse(parts[0]);
            locus.MarkerId = parts[1];
            locus.PhysicalPosition = int.Parse(parts[2]);
            locus.Allele1 = parts[3];
            locus.Allele2 = parts[4];

            for (int i = 0; i < result.Individual.Count; i++)
            {
              var alle1 = double.Parse(parts[i * 3 + 5]);
              var alle2 = double.Parse(parts[i * 3 + 6]);
              var alle3 = double.Parse(parts[i * 3 + 7]);
              if (alle1 == alle2 && alle1 == alle3)//missing value
              {
                result.IsHaplotype1Allele2[locusIndex, i] = true;
                result.IsHaplotype2Allele2[locusIndex, i] = false;
              }
              else
              {
                if (alle1 >= alle2 && alle1 >= alle3)
                {
                  result.IsHaplotype1Allele2[locusIndex, i] = false;
                  result.IsHaplotype2Allele2[locusIndex, i] = false;
                }
                else if (alle2 >= alle3)
                {
                  result.IsHaplotype1Allele2[locusIndex, i] = false;
                  result.IsHaplotype2Allele2[locusIndex, i] = true;
                }
                else
                {
                  result.IsHaplotype1Allele2[locusIndex, i] = true;
                  result.IsHaplotype2Allele2[locusIndex, i] = true;
                }
              }
            }
          }
        }
      }

      return result;
    }

    public void WriteToFile(string fileName, PlinkData data)
    {
      if (!string.IsNullOrEmpty(data.Individual[0].Fid))
      {
        var samplefile = Path.ChangeExtension(fileName, ".sample");
        new GwasSampleFormat().WriteToFile(samplefile, data.Individual);
      }

      //save gen file
      using (var sw = new StreamWriter(fileName))
      {
        sw.NewLine = "\n";
        for (int i = 0; i < data.Locus.Count; i++)
        {
          var item = data.Locus[i];

          sw.Write("{0} {1} {2} {3} {4}", item.Chromosome, item.MarkerId, item.PhysicalPosition, item.Allele1, item.Allele2);

          for (int j = 0; j < data.Individual.Count; j++)
          {
            var sample = data.Individual[j];
            var genotype = data.GenoType(i, j);
            switch (genotype)
            {
              case 0: sw.Write(" 1 0 0"); break;
              case 1: sw.Write(" 0 1 0"); break;
              case 2: sw.Write(" 0 0 1"); break;
              default: sw.Write(" 0 0 0"); break;
            }
          }
          sw.WriteLine();
        }
      }
    }
  }
}
