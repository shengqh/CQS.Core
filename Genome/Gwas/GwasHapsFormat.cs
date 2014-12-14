using CQS.Genome.Plink;
using RCPA;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CQS.Genome.Gwas
{
  public class GwasHapsFormat : IFileFormat<PlinkData>
  {
    public PlinkData ReadFromFile(string fileName)
    {
      var result = new PlinkData();
      var samplefile = Path.ChangeExtension(fileName, ".sample");
      result.Individual = new GwasSampleFormat().ReadFromFile(samplefile);

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
              var alle1 = parts[i * 2 + 5];
              var alle2 = parts[i * 2 + 6];
              if (alle1.Equals("?")) //missing value
              {
                result.IsHaplotype1Allele2[locusIndex, i] = true;
                result.IsHaplotype2Allele2[locusIndex, i] = false;
              }
              else if (alle1.Equals(alle2))
              {
                result.IsHaplotype1Allele2[locusIndex, i] = alle1.Equals("1");
                result.IsHaplotype2Allele2[locusIndex, i] = result.IsHaplotype1Allele2[locusIndex, i];
              }
              else
              {
                result.IsHaplotype1Allele2[locusIndex, i] = false;
                result.IsHaplotype2Allele2[locusIndex, i] = true;
              }
            }
          }
        }
      }

      return result;
    }

    public void WriteToFile(string fileName, PlinkData data)
    {
      var samplefile = Path.ChangeExtension(fileName, ".sample");
      new GwasSampleFormat().WriteToFile(samplefile, data.Individual);

      //save haps file
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
            if (data.IsMissing(i, j))
            {
              sw.Write(" ? ?");
            }
            else
            {
              sw.Write(" {0} {1}",
                data.IsHaplotype1Allele2[i, j] ? 1 : 0,
                data.IsHaplotype2Allele2[i, j] ? 1 : 0);
            }
          }
          sw.WriteLine();
        }
      }
    }
  }
}
