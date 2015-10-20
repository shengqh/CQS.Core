using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CQS.Genome.Bed;
using CQS.Genome.Tophat;
using RCPA;
using CQS.Genome.Gtf;
using System.IO;

namespace CQS.Genome.Annotation
{
  public class SomaticMutationDistanceExporter : IAnnotationTsvExporter
  {
    private string header = null;

    private string emptyStr = null;

    private class Item
    {
      public string Chr { get; set; }
      public long Position { get; set; }
    }

    private Dictionary<string, List<Item>> maps;

    public SomaticMutationDistanceExporter(string sourceFile)
    {
      this.maps = (from line in File.ReadAllLines(sourceFile)
                   where !line.StartsWith("#")
                   let parts = line.Split('\t')
                   where parts.Length > 1 && !string.IsNullOrWhiteSpace(parts[1]) && parts[1].All(l => Char.IsDigit(l))
                   select new Item() { Chr = parts[0].StringAfter("chr"), Position = long.Parse(parts[1]) }).GroupBy(m => m.Chr).ToDictionary(m => m.Key, m => m.OrderBy(l => l.Position).ToList());

      this.header = string.Format("mutation_density");
      this.emptyStr = "\t";
    }

    public string GetHeader()
    {
      return this.header;
    }

    public string GetValue(string chrom, long start, long end)
    {
      if (!maps.ContainsKey(chrom))
      {
        return this.emptyStr;
      }

      var items = maps[chrom];
      if (items.Count < 2)
      {
        return this.emptyStr;
      }

      var index = items.FindIndex(m => m.Position == start);
      if (index == -1)
      {
        return this.emptyStr;
      }

      var disPrev = index == 0 ? int.MaxValue : items[index].Position - items[index - 1].Position;
      var disNext = index == items.Count - 1 ? int.MaxValue : items[index].Position - items[index + 1].Position;

      if (disPrev < Math.Abs(disNext))
      {
        return string.Format("{0}_{1}:{2}", items[index - 1].Chr, items[index - 1].Position, disPrev);
      }
      else
      {
        return string.Format("{0}_{1}:{2}", items[index + 1].Chr, items[index + 1].Position, disNext);
      }
    }
  }
}
