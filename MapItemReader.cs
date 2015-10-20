using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace CQS
{
  public class MapItemReader
  {
    private string key = string.Empty;
    private string information = string.Empty;
    private string value = string.Empty;
    private char delimiter;

    private int keyIndex = -1;
    private int informationIndex = -1;
    private int valueIndex = -1;
    private bool hasHeader = true;

    public Func<string, bool> CheckEnd = m => false;

    /// <summary>
    /// Comment key, default value is '#'
    /// All the line start with comment key will be ignored.
    /// </summary>
    public string CommentKey { get; set; }

    public MapItemReader(string key, string value, char delimiter = '\t', string information = "")
    {
      this.key = key;
      this.value = value;
      this.information = information;
      this.delimiter = delimiter;
      this.CommentKey = "#";
    }

    public MapItemReader(int keyIndex, int valueIndex, char delimiter = '\t', bool hasHeader = true, int informationIndex = -1)
    {
      this.keyIndex = keyIndex;
      this.informationIndex = informationIndex;
      this.valueIndex = valueIndex;
      this.delimiter = delimiter;
      this.hasHeader = hasHeader;
      this.CommentKey = "#";
    }

    public Dictionary<string, MapItem> ReadFromFile(string fileName)
    {
      Func<string, bool> isComment = m => !string.IsNullOrEmpty(CommentKey) && m.StartsWith(CommentKey);

      var result = new Dictionary<string, MapItem>();
      using (StreamReader sr = new StreamReader(fileName))
      {
        string line;
        while ((line = sr.ReadLine()) != null)
        {
          if (!isComment(line))
          {
            break;
          }
        }

        if (keyIndex == -1)
        {
          var parts = line.Split(this.delimiter);

          keyIndex = Array.IndexOf(parts, key);
          if (keyIndex == -1)
          {
            throw new ArgumentException(string.Format("Cannot find key column {0} in file {1}", key, fileName));
          }

          valueIndex = Array.IndexOf(parts, value);
          if (valueIndex == -1)
          {
            throw new ArgumentException(string.Format("Cannot find value column {0} in file {1}", value, fileName));
          }

          if (!string.IsNullOrEmpty(information))
          {
            informationIndex = Array.IndexOf(parts, information);
            if (informationIndex == -1)
            {
              throw new ArgumentException(string.Format("Cannot find information column {0} in file {1}", informationIndex, fileName));
            }
          }

          line = sr.ReadLine();
        }
        else if (hasHeader)
        {
          line = sr.ReadLine();
        }

        while (line != null)
        {
          if (!string.IsNullOrWhiteSpace(line) && !isComment(line))
          {
            if (CheckEnd(line))
            {
              break;
            }

            var curParts = line.Split(this.delimiter);
            var item = new MapItem();
            item.Key = curParts[keyIndex];
            item.Value = curParts[valueIndex];
            if (informationIndex != -1)
            {
              item.Information = curParts[informationIndex];
            }
            result[item.Key] = item;
          }

          line = sr.ReadLine();
        }
      }

      return result;
    }
  }
}
