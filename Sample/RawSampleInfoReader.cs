using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RCPA;

namespace CQS.Sample
{
  public class RawSampleInfoReader : IRawSampleInfoReader
  {
    private List<IRawSampleInfoReader> readers = new List<IRawSampleInfoReader>(RawSampleInfoReaderFactory.Readers);

    public string SupportFor
    {
      get
      {
        return StringUtils.Merge(from r in readers
                                 select r.SupportFor, "/");
      }
    }

    public bool IsReaderFor(string directory)
    {
      return readers.Any(m => m.IsReaderFor(directory));
    }

    public Dictionary<string, Dictionary<string, List<string>>> ReadDescriptionFromDirectory(string dir)
    {
      var curReaders = (from r in readers
                        where r.IsReaderFor(dir)
                        select r).ToList();
      if (curReaders.Count == 0)
      {
        throw new Exception("I don't know how to parse information from " + dir);
      }

      Dictionary<string, Dictionary<string, List<string>>> result = new Dictionary<string, Dictionary<string, List<string>>>();
      foreach (var reader in curReaders)
      {
        var curResult = reader.ReadDescriptionFromDirectory(dir);
        foreach (var cr in curResult)
        {
          if (!result.ContainsKey(cr.Key))
          {
            result[cr.Key] = cr.Value;
            continue;
          }

          var map = result[cr.Key];
          foreach (var v in cr.Value)
          {
            if (!map.ContainsKey(v.Key))
            {
              map[v.Key] = v.Value;
            }
            else
            {
              map[v.Key].AddRange(v.Value);
            }
          }
        }
      }
      return result;
    }
  }
}
