using System;
using System.Collections.Generic;
using System.IO;

namespace CQS
{
  public abstract class AbstractHeaderFile<T> : AbstractTableFile<T> where T : new()
  {
    private Dictionary<string, Action<string, T>> _headerActionMap;

    protected abstract Dictionary<string, Action<string, T>> GetHeaderActionMap();

    protected AbstractHeaderFile()
    {
    }

    protected AbstractHeaderFile(string filename) : base(filename)
    {
    }

    protected override Dictionary<int, Action<string, T>> GetIndexActionMap()
    {
      if (_headerActionMap == null)
      {
        _headerActionMap = GetHeaderActionMap();
      }

      string line = FindHeader(reader);
      string[] headers = line.Split('\t');

      var result = new Dictionary<int, Action<string, T>>();
      for (int i = 0; i < headers.Length; i++)
      {
        string part = headers[i];
        if (_headerActionMap.ContainsKey(part))
        {
          result[i] = _headerActionMap[part];
        }
      }

      return result;
    }

    /// <summary>
    ///   Get header from file. Default is the next line of stream
    /// </summary>
    /// <param name="sr">data stream</param>
    /// <returns>header line</returns>
    protected virtual string FindHeader(TextReader sr)
    {
      return sr.ReadLine();
    }
  }
}
