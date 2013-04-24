using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace CQS.Genome.QC
{
  public class FastQCItem
  {
    public FastQCItem()
    {
      this.Name = string.Empty;
      this.Items = new List<FastQCBasicStatisticItem>();
    }

    public string Name { get; set; }

    public List<FastQCBasicStatisticItem> Items { get; set; }

    public static FastQCItem ParseFromDirectory(string directory)
    {
      FastQCItem result = new FastQCItem();
      result.Name = Path.GetFileName(directory);

      foreach (var dir in Directory.GetDirectories(directory))
      {
        var file = dir + "/fastqc_data.txt";
        if (File.Exists(file))
        {
          result.Items.Add(FastQCBasicStatisticItem.ParseFromFile(file));
        }
      }

      if (result.Items.Count == 0)
      {
        return null;
      }

      return result;
    }

    public string FileNames
    {
      get
      {
        return (from item in Items
                select item.Name).Merge(";");
      }
    }

    public string FileType
    {
      get
      {
        if (Items == null || Items.Count == 0)
        {
          return string.Empty;
        }

        return Items.First().FileType;
      }
    }

    public string Encoding
    {
      get
      {
        if (Items == null || Items.Count == 0)
        {
          return string.Empty;
        }

        return Items.First().Encoding;
      }
    }

    public long TotalSequences
    {
      get
      {
        return this.Items.Sum(m => m.TotalSequences);
      }
    }

    public long FilteredSequences
    {
      get
      {
        return this.Items.Sum(m => m.FilteredSequences);
      }
    }

    public int SequenceLength
    {
      get
      {
        if (Items == null || Items.Count == 0)
        {
          return 0;
        }

        return Items.First().SequenceLength;
      }
    }

    public double GC
    {
      get
      {
        return (from item in Items
                select item.GC).Average();
      }
    }

  }
}
