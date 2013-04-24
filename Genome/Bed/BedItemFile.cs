﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RCPA;
using System.IO;

namespace CQS.Genome.Bed
{
  public class BedItemFile<T> : AbstractTableFile<T> where T : BedItem, new()
  {
    public BedItemFile()      : base()    { }

    public BedItemFile(string filename)      : base(filename)    { }

    /// <summary>
    /// at least contains three colomns : chromosome, start and end
    /// </summary>
    protected override int MinLength
    {
      get
      {
        return 3;
      }
    }

    public int HeaderColumnCount
    {
      get
      {
        return 6;
      }
    }

    public string GetHeader()
    {
      return "chrom\tstart\tend\tname\tscore\tstrand";
    }

    public string GetValue(BedItem item)
    {
      return string.Format("{0}\t{1}\t{2}\t{3}\t{4}\t{5}",
        item.Chrom,
        item.ChromStart,
        item.ChromEnd,
        item.Name,
        item.Score,
        item.Strand);
    }

    protected override Dictionary<int, Action<string, T>> GetIndexActionMap()
    {
      var result = new Dictionary<int, Action<string, T>>();

      result[0] = (m, n) => n.Chrom = m;
      result[1] = (m, n) => n.ChromStart = long.Parse(m);
      result[2] = (m, n) => n.ChromEnd = long.Parse(m);
      result[3] = (m, n) => n.Name = m;
      result[4] = (m, n) => n.Score = double.Parse(m);
      result[5] = (m, n) => n.Strand = m[0];
      result[6] = (m, n) => n.ThickStart = long.Parse(m);
      result[7] = (m, n) => n.ThickEnd = long.Parse(m);
      result[8] = (m, n) => n.ItemRgb = m;
      result[9] = (m, n) => n.BlockCount = int.Parse(m);
      result[10] = (m, n) => n.BlockSizes = m;
      result[11] = (m, n) => n.BlockStarts = m;

      return result;
    }
  }
}