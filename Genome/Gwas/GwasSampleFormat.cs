using CQS.Genome.Plink;
using RCPA;
using System.Collections.Generic;
using System.IO;

namespace CQS.Genome.Gwas
{
  public class GwasSampleFormat : IFileFormat<List<PlinkIndividual>>
  {
    public List<PlinkIndividual> ReadFromFile(string fileName)
    {
      var result = new List<PlinkIndividual>();

      using (var sr = new StreamReader(fileName))
      {
        string line = sr.ReadLine();
        sr.ReadLine();
        while ((line = sr.ReadLine()) != null)
        {
          var parts = line.Split(' ');
          if (parts.Length < 6)
          {
            continue;
          }

          var individual = new PlinkIndividual();
          individual.Fid = parts[0];
          individual.Iid = parts[1];
          individual.Sexcode = parts[5];
          result.Add(individual);
        }
      }

      return result;
    }

    public void WriteToFile(string fileName, List<PlinkIndividual> samples)
    {
      using (var sw = new StreamWriter(fileName))
      {
        sw.NewLine = "\n";
        sw.WriteLine("ID_1 ID_2 missing father mother sex plink_pheno");
        sw.WriteLine("0 0 0 D D D B");

        foreach (var sample in samples)
        {
          sw.WriteLine("{0} {1} 0 0 0 {2} -9", sample.Fid, sample.Iid, sample.Sexcode);
        }
      }
    }
  }
}
