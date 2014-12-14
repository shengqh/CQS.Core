using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CQS.Genome.Mapping
{
  public class ReadCountItem
  {
    public ReadCountItem()
    {
      this.Name = string.Empty;
      this.MappedFile = string.Empty;
      this.Sequence = string.Empty;
    }

    public string Name { get; set; }

    public int Count { get; set; }

    public string Sequence { get; set; }

    public string MappedFile { get; set; }

    public static List<ReadCountItem> ReadFromFile(string fileName)
    {
      var result = new List<ReadCountItem>();
      using (var sr = new StreamReader(fileName))
      {
        string line = sr.ReadLine();
        while ((line = sr.ReadLine()) != null)
        {
          var parts = line.Split('\t');
          result.Add(new ReadCountItem()
          {
            Name = parts[0],
            Count = int.Parse(parts[1]),
            Sequence = parts[2]
          });
        }
      }
      return result;
    }

    public static void WriteToFile(string fileName, List<ReadCountItem> items, bool outputMappedFile = false)
    {
      using (var sw = new StreamWriter(fileName))
      {
        sw.WriteLine("Query\tCount\tSequence{0}", outputMappedFile ? "\tFile" : "");
        foreach (var item in items)
        {
          sw.WriteLine("{0}\t{1}\t{2}{3}", item.Name, item.Count, item.Sequence, outputMappedFile ? "\t" + item.MappedFile : "");
        }
      }
    }
  }
}
