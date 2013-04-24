using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace CQS.TCGA
{
  public class TabMapReader
  {
    private string key;
    private string value;
    public TabMapReader(string key, string value)
    {
      this.key = key;
      this.value = value;
    }

    public Dictionary<string, string> ReadFromFile(string fileName)
    {
      Dictionary<string, string> result = new Dictionary<string, string>();
      using (StreamReader sr = new StreamReader(fileName))
      {
        var line = sr.ReadLine();
        var parts = line.Split('\t');
        var keyIndecies = new List<int>();

        for (int i = 0; i < parts.Length; i++)
        {
          if (parts[i].Equals(key))
          {
            keyIndecies.Add(i);
          }
        }

        if (keyIndecies.Count == 0)
        {
          throw new ArgumentException(string.Format("Cannot find key column {0} in sdrf file {1}", key, fileName));
        }

        var valueIndex = Array.IndexOf(parts, value);
        if (valueIndex == -1)
        {
          throw new ArgumentException(string.Format("Cannot find value column {0} in sdrf file {1}", value, fileName));
        }

        while ((line = sr.ReadLine()) != null)
        {
          if (string.IsNullOrWhiteSpace(line))
          {
            continue;
          }

          var curParts = line.Split('\t');
          var curValue = curParts[valueIndex];

          foreach (var index in keyIndecies)
          {
            result[curParts[index]] = curValue;
          }
        }
      }

      return result;
    }
  }
}
