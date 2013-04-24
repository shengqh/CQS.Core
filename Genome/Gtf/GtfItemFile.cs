using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RCPA;
using System.IO;

namespace CQS.Genome.Gtf
{
  public class GtfItemFile : AbstractFile
  {
    public GtfItemFile()
      : base()
    { }

    public GtfItemFile(string filename)
      : base(filename)
    { }

    public GtfItem Next()
    {
      if (reader == null)
      {
        throw new FileNotFoundException("Open file first.");
      }

      string line;
      while ((line = reader.ReadLine()) != null)
      {
        var parts = line.Split('\t');
        if (parts.Length >= 9)
        {
          return ParseItem(parts);
        }
      }

      return null;
    }

    private static GtfItem ParseItem(string[] parts)
    {
      GtfItem result = new GtfItem();
      result.Seqname = parts[0];
      result.Source = parts[1];
      result.Feature = parts[2];
      result.Start = long.Parse(parts[3]);
      result.End = long.Parse(parts[4]);
      result.Score = parts[5];
      result.Strand = parts[6][0];
      result.Frame = parts[7][0];
      result.Attributes = parts[8];
      return result;
    }

    public GtfItem NextExon()
    {
      string line;
      while ((line = reader.ReadLine()) != null)
      {
        var parts = line.Split('\t');
        if (parts.Length >= 9 && parts[2].Equals("exon"))
        {
          return ParseItem(parts);
        }
      }
      return null;
    }
  }
}
