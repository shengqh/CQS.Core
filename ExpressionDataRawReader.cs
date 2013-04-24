using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RCPA;
using System.IO;

namespace CQS
{
  /// <summary>
  /// Read expression dat from raw file, usually like
  ///                 Value
  /// at10392         3.44393
  /// at10830         1.39388
  /// 
  /// Then, the parameters in construction minColumnCount is 2 and the valueColumnIndex is 1 (zero based)
  /// </summary>
  public class ExpressionDataRawReader : IFileReader<ExpressionData>
  {
    private int minColumnCount;
    private int valueColumnIndex;
    private int startRowIndex;

    public ExpressionDataRawReader(int minColumnCount, int valueColumnIndex, int startRowIndex = 1)
    {
      this.minColumnCount = minColumnCount;
      this.valueColumnIndex = valueColumnIndex;
      this.startRowIndex = startRowIndex;
    }

    public ExpressionData ReadFromFile(string fileName)
    {
      ExpressionData result = new ExpressionData();

      Dictionary<string, ExpressionValue> map = new Dictionary<string, ExpressionValue>();

      using (StreamReader sr = new StreamReader(fileName))
      {
        for (int i = 0; i < startRowIndex; i++)
        {
          sr.ReadLine();
        }

        string line;
        while ((line = sr.ReadLine()) != null)
        {
          var parts = line.Split('\t');
          if (parts.Length < this.minColumnCount)
          {
            continue;
          }

          if (parts[0].StartsWith("?|"))
          {
            continue;
          }

          string gene;
          var pos = parts[0].IndexOf('|');
          if (pos != -1)
          {
            gene = parts[0].Substring(0, pos);
          }
          else
          {
            gene = parts[0];
          }

          string valueStr = this.valueColumnIndex == -1 ? parts.Last() : parts[this.valueColumnIndex];
          double value;
          if (!double.TryParse(valueStr, out value))
          {
            value = double.NaN;
          }

          //If same gene have multiple entries, sum them all as gene expression value
          if (map.ContainsKey(gene))
          {
            if (double.IsNaN(map[gene].Value))
            {
              map[gene].Value = value;
            }
            else if (!double.IsNaN(value))
            {
              map[gene].Value = map[gene].Value + value;
            }
          }
          else
          {
            map[gene] = new ExpressionValue()
            {
              Name = gene,
              Value = value
            };
          }
        }
      }

      result.Values.AddRange(map.Values.ToList());

      return result;
    }
  }
}
