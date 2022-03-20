using CQS.Genome.Bed;
using RCPA;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CQS.Genome.Gtf
{
  public class Gtf2BedGeneIdBuilder : AbstractThreadProcessor
  {
    private Gtf2BedGeneIdBuilderOptions options;

    public Gtf2BedGeneIdBuilder(Gtf2BedGeneIdBuilderOptions options)
    {
      this.options = options;
    }

    public override IEnumerable<string> Process()
    {
      Dictionary<string, BedItem> map = ReadBedItems();

      var keys = (from key in map.Keys
                  orderby key
                  select key).ToList();

      using (StreamWriter sw = new StreamWriter(options.OutputFile))
      {
        foreach (var key in keys)
        {
          var bed = map[key];
          sw.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}\t{5}",
            bed.Seqname,
            bed.Start,
            bed.End,
            bed.Name,
            0,
            bed.Strand);
        }
      }

      return new string[] { options.OutputFile };
    }

    public Dictionary<string, BedItem> ReadBedItems()
    {
      Dictionary<string, BedItem> map = new Dictionary<string, BedItem>();

      using (var gtf = new GtfItemFile(options.InputFile))
      {
        GtfItem item;
        int count = 0;
        while ((item = gtf.Next()) != null)
        {
          count++;
          if ((count % 100000) == 0)
          {
            Progress.SetMessage("{0} gtf item processed", count);
          }

          BedItem loc;
          string name;
          if (options.ByName)
          {
            name = item.Name;
            if (string.IsNullOrWhiteSpace(name)) //maybe in gff3 format
            {
              //Console.WriteLine(item.Attributes);
              name = item.Attributes.StringAfter("Name=").StringBefore(";");
            }
          }
          else
          {
            name = item.GeneId;
            if (string.IsNullOrWhiteSpace(name)) //maybe in gff3 format
            {
              name = item.Attributes.StringAfter("ID=").StringBefore(";");
            }
          }

          if (!map.TryGetValue(name, out loc))
          {
            loc = new BedItem();
            loc.Name = name;
            loc.Seqname = item.Seqname;
            loc.Start = item.Start;
            loc.End = item.End;
            loc.Strand = item.Strand;
            map[name] = loc;
            continue;
          }

          map[name].UnionWith(item);
        }
      }

      map.Values.ToList().ForEach(m => m.Start--);
      return map;
    }
  }
}
