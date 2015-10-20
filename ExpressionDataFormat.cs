using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RCPA;
using System.IO;

namespace CQS
{
  public class ExpressionDataFormat<T> : IFileFormat<List<T>> where T : ExpressionData, new()
  {
    private int startColumn;
    public ExpressionDataFormat(int startColumn = 1)
    {
      this.startColumn = startColumn;
    }

    public virtual List<T> ReadFromFile(string fileName)
    {
      List<T> result = new List<T>();

      using (StreamReader sr = new StreamReader(fileName))
      {
        var line = sr.ReadLine();
        var parts = line.Split('\t');
        for (int i = startColumn; i < parts.Length; i++)
        {
          result.Add(new T() { SampleBarcode = parts[i] });
        }

        while ((line = sr.ReadLine()) != null)
        {
          var values = line.Split('\t');
          for (int i = startColumn; i < values.Length; i++)
          {
            result[i - startColumn].Values.Add(new ExpressionValue()
            {
              Name = values[0],
              Value = values[i].Equals("NA") ? double.NaN : double.Parse(values[i])
            });
          }
        }
      }

      return result;
    }

    public virtual void WriteToFile(string fileName, List<T> t)
    {
      var keys = (from data in t
                  from gene in data.Values
                  select gene.Name).Distinct().OrderBy(m => m).ToList();

      var maps = (from data in t
                  let map = data.Values.ToDictionary(m => m.Name)
                  select new { Data = data, Map = map }).OrderBy(m => m.Data.SampleBarcode).ToList();

      using (StreamWriter sw = new StreamWriter(fileName))
      {
        sw.Write("Gene");
        maps.ForEach(m => sw.Write("\t" + m.Data.SampleBarcode));
        sw.WriteLine();
        foreach (var key in keys)
        {
          sw.Write(key);
          maps.ForEach(m =>
          {
            if (m.Map.ContainsKey(key))
            {
              var d = m.Map[key];
              if (double.IsNaN(d.Value))
              {
                sw.Write("\tNA");
              }
              else
              {
                sw.Write("\t{0:0.0000}", d.Value);
              }
            }
            else
            {
              sw.Write("\tNA");
            }
          });
          sw.WriteLine();
        }
      }

      using (StreamWriter sw = new StreamWriter(fileName + ".stat"))
      {
        sw.WriteLine("Sample\t{0}", t.Count);
        t.ForEach(m => sw.WriteLine("\t{0}", m.SampleBarcode));

        sw.WriteLine();
        sw.WriteLine("Gene\t{0}", keys.Count);
        keys.ForEach(m => sw.WriteLine("\t{0}", m));
      }
    }
  }
}
