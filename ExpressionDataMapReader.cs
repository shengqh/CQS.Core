using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RCPA;
using System.IO;

namespace CQS
{
  public class ExpressionDataMapReader : IFileReader<ExpressionData>
  {
    private string key;
    private string value;
    private char delimiter;

    public ExpressionDataMapReader(string key, string value, char delimiter = '\t')
    {
      this.key = key;
      this.value = value;
      this.delimiter = delimiter;
    }

    public ExpressionData ReadFromFile(string fileName)
    {
      ExpressionData result = new ExpressionData();
      using (StreamReader sr = new StreamReader(fileName))
      {
        var line = sr.ReadLine();
        var parts = line.Split(this.delimiter);

        var keyIndex = Array.IndexOf(parts, key);
        if (keyIndex == -1)
        {
          throw new ArgumentException(string.Format("Cannot find key column {0} in file {1}", key, fileName));
        }

        var valueIndex = Array.IndexOf(parts, value);
        if (valueIndex == -1)
        {
          throw new ArgumentException(string.Format("Cannot find value column {0} in file {1}", value, fileName));
        }

        while ((line = sr.ReadLine()) != null)
        {
          if (string.IsNullOrWhiteSpace(line))
          {
            continue;
          }

          var curParts = line.Split(this.delimiter);
          var curKey = curParts[keyIndex];
          var curValue = curParts[valueIndex];
          double dValue;
          if (!double.TryParse(curValue, out dValue))
          {
            dValue = double.NaN;
          }

          result.Values.Add(new ExpressionValue(curKey, dValue));
        }
      }

      return result;
    }
  }
}
