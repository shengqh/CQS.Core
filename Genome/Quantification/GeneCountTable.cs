using RCPA;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CQS.Genome.Quantification
{
  public class GeneCountTable
  {
    public string[] GeneHeaders { get; set; }

    public string[] Samples { get; set; }

    public List<string[]> GeneValues { get; set; }

    public double[,] Count { get; set; }
  }

  public class GeneCountTableFormat : IFileFormat<GeneCountTable>
  {
    public GeneCountTable ReadFromFile(string fileName)
    {
      var result = new GeneCountTable();
      double value;
      int geneCount = 1;
      int startIndex = -1;
      using (var sr = new StreamReader(fileName))
      {
        var header = sr.ReadLine();
        var data = sr.ReadLine();
        if (header == null || data == null)
        {
          throw new Exception("No data found in " + fileName);
        }

        var parts = data.Split('\t');
        startIndex = header.Split('\t').ToList().FindIndex(m => !m.StartsWith("Feature"));
        if (startIndex == -1)
        {
          startIndex = 1;
        }

        for (; startIndex < parts.Length; startIndex++)
        {
          if (parts.Skip(startIndex).All(m => double.TryParse(m, out value)))
          {
            break;
          }
        }

        result.GeneHeaders = header.Split('\t').Take(startIndex).ToArray();
        result.Samples = header.Split('\t').Skip(startIndex).ToArray();

        while ((data = sr.ReadLine()) != null)
        {
          if (String.IsNullOrWhiteSpace(data))
          {
            break;
          }

          geneCount++;
        }
      }

      result.GeneValues = new List<string[]>();
      result.Count = new double[geneCount, result.Samples.Length];

      using (var sr = new StreamReader(fileName))
      {
        var header = sr.ReadLine();
        string data;
        int geneIndex = -1;
        while ((data = sr.ReadLine()) != null)
        {
          if (String.IsNullOrWhiteSpace(data))
          {
            break;
          }

          geneIndex++;
          var parts = data.Split('\t');
          result.GeneValues.Add(parts.Take(startIndex).ToArray());
          for (int sIndex = startIndex; sIndex < parts.Length; sIndex++)
          {
            result.Count[geneIndex, sIndex - startIndex] = double.Parse(parts[sIndex]);
          }
        }
      }

      return result;
    }

    public void WriteToFile(string fileName, GeneCountTable table)
    {
      using (var sw = new StreamWriter(fileName))
      {
        sw.WriteLine("{0}\t{1}", table.GeneHeaders.Merge("\t"), table.Samples.Merge("\t"));
        for (int i = 0; i < table.GeneValues.Count; i++)
        {
          sw.Write("{0}", table.GeneValues[i].Merge("\t"));
          for (int j = 0; j < table.Samples.Length; j++)
          {
            sw.Write("\t{0:0.###}", table.Count[i, j]);
          }
          sw.WriteLine();
        }
      }
    }
  }
}
