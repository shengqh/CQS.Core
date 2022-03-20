using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CQS
{
  public class ExpressionDataFastFormat<T> : ExpressionDataFormat<T> where T : ExpressionData, new()
  {
    private bool byGene = true;

    public ExpressionDataFastFormat(bool byGene = true)
    {
      this.byGene = byGene;
    }

    public override void WriteToFile(string fileName, List<T> t)
    {
      if (byGene)
      {
        DoWriteToFileByGene(fileName, t);
      }
      else
      {
        DoWriteToFileBySample(fileName, t);
      }
    }

    private void DoWriteToFileBySample(string fileName, List<T> t)
    {
      var keys = (from data in t
                  from gene in data.Values
                  select gene.Name).Distinct().OrderBy(m => m).ToList();

      //make sure data was filled and sorted by gene names
      foreach (var data in t)
      {
        if (data.Values.Count != keys.Count)
        {
          throw new ArgumentException("Gene names should be fill and sorted before save to file!");
        }

        for (int i = 0; i < data.Values.Count; i++)
        {
          if (!data.Values[i].Name.Equals(keys[i]))
          {
            throw new ArgumentException("Gene names should be fill and sorted before save to file!");
          }
        }
      }

      using (StreamWriter sw = new StreamWriter(fileName))
      {
        sw.Write("Sample");
        keys.ForEach(m => sw.Write("\t" + m));
        sw.WriteLine();

        for (int i = 0; i < t.Count; i++)
        {
          sw.Write(t[i].SampleBarcode);

          t[i].Values.ForEach(m =>
          {
            var value = m.Value;
            if (double.IsNaN(value))
            {
              sw.Write("\tNA");
            }
            else
            {
              sw.Write("\t{0:0.0000}", value);
            }
          });
          sw.WriteLine();
        }
      }

      DoWriteStat(fileName, t, keys);
    }

    private void DoWriteStat(string fileName, List<T> t, List<string> keys)
    {
      using (StreamWriter sw = new StreamWriter(fileName + ".stat"))
      {
        sw.WriteLine("Sample\t{0}", t.Count);
        t.ForEach(m => sw.WriteLine("\t{0}", m.SampleBarcode));

        sw.WriteLine();
        sw.WriteLine("Gene\t{0}", keys.Count);
        keys.ForEach(m => sw.WriteLine("\t{0}", m));
      }
    }


    private void DoWriteToFileByGene(string fileName, List<T> t)
    {
      var keys = (from data in t
                  from gene in data.Values
                  select gene.Name).Distinct().OrderBy(m => m).ToList();

      //make sure data was filled and sorted by gene names
      foreach (var data in t)
      {
        if (data.Values.Count != keys.Count)
        {
          throw new ArgumentException("Gene names should be fill and sorted before save to file!");
        }

        for (int i = 0; i < data.Values.Count; i++)
        {
          if (!data.Values[i].Name.Equals(keys[i]))
          {
            throw new ArgumentException("Gene names should be fill and sorted before save to file!");
          }
        }
      }

      using (StreamWriter sw = new StreamWriter(fileName))
      {
        sw.Write("Gene");
        t.ForEach(m => sw.Write("\t" + m.SampleBarcode));
        sw.WriteLine();
        for (int i = 0; i < keys.Count; i++)
        {
          sw.Write(keys[i]);

          t.ForEach(m =>
          {
            var value = m.Values[i].Value;

            if (double.IsNaN(value))
            {
              sw.Write("\tNA");
            }
            else
            {
              sw.Write("\t{0:0.0000}", value);
            }
          });
          sw.WriteLine();
        }
      }

      DoWriteStat(fileName, t, keys);
    }
  }
}
