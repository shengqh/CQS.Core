using RCPA;
using System;
using System.Collections.Generic;
using System.IO;

namespace CQS.Genome.Annotation
{
  public class GenebankFormat : IFileReader<List<GenebankItem>>
  {
    public List<GenebankItem> ReadFromFile(string fileName)
    {
      List<GenebankItem> result = new List<GenebankItem>();

      using (StreamReader sr = new StreamReader(fileName))
      {
        string line;
        while ((line = sr.ReadLine()) != null)
        {
          if (line.StartsWith("LOCUS"))
          {
            GenebankItem item = new GenebankItem();
            result.Add(item);
            item.Accession = line.StringAfter("LOCUS").Trim().StringBefore(" ");

            while ((line = sr.ReadLine()) != null)
            {
              if (line.StartsWith("FEATURES"))
              {
                GenebankFeature feature = null;
                while ((line = sr.ReadLine()) != null)
                {
                  if (line[0] != ' ')
                  {
                    break;
                  }

                  if (line[5] != ' ')
                  {
                    feature = new GenebankFeature();
                    item.Features.Add(feature);
                    feature.FeatureName = line.Substring(5).StringBefore(" ");
                    feature.Location = line;
                  }
                  else
                  {
                    feature.Context.Add(line.Substring(3).Trim());
                  }
                }
              }

              if (line.Equals("//"))
              {
                break;
              }
            }
          }
        }
      }

      return result;
    }
  }
}
