using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RCPA;
using System.IO;

namespace CQS
{
  public abstract class AbstractHeaderFile<T> : AbstractTableFile<T> where T : new()
  {
    private Dictionary<string, Action<string, T>> headerActionMap = null;

    protected abstract Dictionary<string, Action<string, T>> GetHeaderActionMap();

    public AbstractHeaderFile() : base() { }

    public AbstractHeaderFile(string filename) : base(filename) { }

    protected override Dictionary<int, Action<string, T>> GetIndexActionMap()
    {
      if (headerActionMap == null)
      {
        headerActionMap = GetHeaderActionMap();
      }

      var line = FindHeader(this.reader);
      var headers = line.Split('\t');

      var result = new Dictionary<int, Action<string, T>>();
      for (int i = 0; i < headers.Length; i++)
      {
        var part = headers[i];
        if (headerActionMap.ContainsKey(part))
        {
          result[i] = headerActionMap[part];
        }
      }

      return result;
    }

    /// <summary>
    /// Get header from file. Default is the next line of stream
    /// </summary>
    /// <param name="sr">data stream</param>
    /// <returns>header line</returns>
    protected virtual string FindHeader(TextReader sr)
    {
      return sr.ReadLine();
    }
  }
}
