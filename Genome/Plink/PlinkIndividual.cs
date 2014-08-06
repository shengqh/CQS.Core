using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CQS.Genome.Plink
{
  public class PlinkIndividual
  {
    public string Fid { get; set; }
    public string Iid { get; set; }
    public string Pat { get; set; }
    public string Mat { get; set; }
    public string Sexcode { get; set; }
    public double Phenotype { get; set; }

    public static List<PlinkIndividual> ReadFromFile(string fileName)
    {
      var result = new List<PlinkIndividual>();

      using (var sr = new StreamReader(fileName))
      {
        string line;
        var comms = new[] { '\t', ' ' };
        while ((line = sr.ReadLine()) != null)
        {
          line = line.Trim();
          if (string.IsNullOrEmpty(line))
          {
            continue;
          }

          var parts = line.Split(comms);
          if (string.IsNullOrEmpty(parts[1]))
          {
            continue;
          }

          var ind = new PlinkIndividual();
          ind.Fid = parts[0];
          ind.Iid = parts[1];
          ind.Pat = parts[2];
          ind.Mat = parts[3];
          ind.Sexcode = parts[4];
          ind.Phenotype = double.Parse(parts[5]);
          result.Add(ind);
        }
      }

      return result;
    }

  }
}
