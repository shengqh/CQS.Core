using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace CQS.Genome
{
  public class CountMap
  {
    private string countFile;

    private Dictionary<string, int> countMap = new Dictionary<string, int>();

    public bool HasCountFile { get; private set; }

    public CountMap() : this(null) { }

    public CountMap(string countFile)
    {
      this.countFile = countFile;
      this.HasCountFile = File.Exists(countFile);

      if (this.HasCountFile)
      {
        Dictionary<string, string> counts = new MapReader(0, 1).ReadFromFile(countFile);
        foreach (var c in counts)
        {
          countMap[c.Key] = int.Parse(c.Value);
        }
        func = DoGetIdenticalCount;
      }
      else
      {
        func = DoGetCountOne;
      }
    }

    private int DoGetCountOne(string qName)
    {
      return 1;
    }

    private int DoGetIdenticalCount(string qName)
    {
      int value;
      if (countMap.TryGetValue(qName, out value))
      {
        return value;
      }
      else
      {
        throw new Exception("Cannot find query " + qName + " in count file " + this.countFile);
      }
    }

    delegate int GetCountFunc(string qName);

    private GetCountFunc func;

    public int GetCount(string qName)
    {
      return func(qName);
    }
  }
}
