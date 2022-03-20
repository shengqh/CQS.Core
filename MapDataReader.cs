using System;
using System.Collections.Generic;
using System.IO;

namespace CQS
{
  public class MapDataItem
  {
    public MapDataItem()
    {
      this.Informations = new List<string>();
    }
    public string Key { get; set; }
    public string Value { get; set; }
    public List<string> Informations { get; private set; }
  }

  public class MapData
  {
    public MapData()
    {
      this.InfoNames = new List<string>();
      this.Data = new Dictionary<string, MapDataItem>();
    }
    public string KeyName { get; set; }
    public string ValueName { get; set; }
    public List<string> InfoNames { get; private set; }
    public Dictionary<string, MapDataItem> Data { get; set; }
  }

  public class MapDataReader
  {
    private string keyName = string.Empty;
    private string valueName = string.Empty;
    private char delimiter;
    private int keyIndex = -1;
    private int valueIndex = -1;

    /// <summary>
    /// Comment key, default value is '#'
    /// All the line start with comment key will be ignored.
    /// </summary>
    private string commentKey { get; set; }

    public Func<string, bool> CheckEnd = m => false;

    public MapDataReader(string keyName, string valueName, char delimiter = '\t', string commentKey = "#")
    {
      this.keyName = keyName;
      this.valueName = valueName;
      this.delimiter = delimiter;
      this.commentKey = commentKey;
    }

    public MapDataReader(int keyIndex, int valueIndex, char delimiter = '\t', string commentKey = "#")
    {
      this.keyIndex = keyIndex;
      this.valueIndex = valueIndex;
      this.delimiter = delimiter;
      this.commentKey = commentKey;
    }

    public MapData ReadFromFile(string fileName)
    {
      Func<string, bool> isComment = m => !string.IsNullOrEmpty(commentKey) && m.StartsWith(commentKey);

      var result = new MapData();
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

        var parts = line.Split(this.delimiter);
        if (this.keyIndex == -1)
        {
          this.keyIndex = Array.IndexOf(parts, this.keyName);
          if (this.keyIndex == -1)
          {
            throw new ArgumentException(string.Format("Cannot find key column {0} in file {1}", this.keyName, fileName));
          }
        }
        else
        {
          this.keyName = parts[this.keyIndex];
        }

        if (this.valueIndex == -1)
        {
          this.valueIndex = Array.IndexOf(parts, this.valueName);
          if (valueIndex == -1)
          {
            throw new ArgumentException(string.Format("Cannot find value column {0} in file {1}", this.valueName, fileName));
          }
        }
        else
        {
          this.valueName = parts[this.valueIndex];
        }

        result.KeyName = this.keyName;
        result.ValueName = this.valueName;
        for (int i = 0; i < parts.Length; i++)
        {
          if (i != keyIndex && i != valueIndex)
          {
            result.InfoNames.Add(parts[i]);
          }
        }

        while ((line = sr.ReadLine()) != null)
        {
          if (!string.IsNullOrWhiteSpace(line) && !isComment(line))
          {
            if (CheckEnd(line))
            {
              break;
            }

            var curParts = line.Split(this.delimiter);
            var item = new MapDataItem();
            item.Key = curParts[keyIndex];
            item.Value = curParts[valueIndex];
            result.Data[item.Key] = item;
            for (int i = 0; i < curParts.Length; i++)
            {
              if (i != keyIndex && i != valueIndex)
              {
                item.Informations.Add(curParts[i]);
              }
            }
          }
        }
      }

      return result;
    }
  }
}
