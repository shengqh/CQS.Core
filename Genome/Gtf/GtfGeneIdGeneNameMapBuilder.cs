using RCPA;
using RCPA.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CQS.Genome.Gtf
{
  public class GtfGeneIdGeneNameMapBuilder : AbstractThreadProcessor
  {
    private GtfGeneIdGeneNameMapBuilderOptions options;

    public GtfGeneIdGeneNameMapBuilder(GtfGeneIdGeneNameMapBuilderOptions options)
    {
      this.options = options;
    }

    public override IEnumerable<string> Process()
    {
      Dictionary<string, List<GtfItem>> map = new Dictionary<string, List<GtfItem>>();

      var namemap = new Dictionary<string, string>();
      if (File.Exists(options.MapFile))
      {
        namemap = new MapReader(0, 1, hasHeader: false).ReadFromFile(options.MapFile);
      }

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
          List<GtfItem> oldItems;
          if (!map.TryGetValue(item.GeneId, out oldItems))
          {
            map[item.GeneId] = new[] { item }.ToList();
          }
          else
          {
            if (IsExon(item))
            {
              oldItems.RemoveAll(m => !IsExon(m));
              oldItems.Add(item);
            }
            else
            {
              if (oldItems.All(m => !IsExon(m)))
              {
                oldItems.Add(item);
              }
            }
          }
        }
      }

      //      map[item.GeneId] = item.Attributes.StringAfter("gene_name \"").StringBefore("\"");
      var keys = (from key in map.Keys
                  orderby key
                  select key).ToList();

      using (StreamWriter sw = new StreamWriter(options.OutputFile))
      using (StreamWriter swBed = new StreamWriter(options.OutputFile + ".bed"))
      {
        bool bHasGeneName = map.Values.Any(l => l.Any(m => m.Attributes.Contains("gene_name")));
        if (!bHasGeneName  && !File.Exists(options.MapFile))
        {
          throw new Exception(string.Format("No gene_name found in {0} and no id/name map file defined.", options.InputFile));
        }

        bool bHasGeneBiotype = map.Values.Any(l => l.Any(m => m.Attributes.Contains("gene_biotype")));
        if (bHasGeneBiotype)
        {
          sw.WriteLine("gene_id\tgene_name\tlength\tchr\tstart\tend\tgene_biotype");
        }
        else
        {
          sw.WriteLine("gene_id\tgene_name\tlength\tchr\tstart\tend");
        }

        foreach (var key in keys)
        {
          var gtfs = map[key];
          string name;
          var gtf = gtfs.FirstOrDefault(m => m.Attributes.Contains("gene_name"));
          gtfs.CombineCoordinates();
          string biotype;
          if (gtf == null)
          {
            biotype = string.Empty;
            if (!namemap.TryGetValue(key, out name))
            {
              name = key;
            }
          }
          else
          {
            biotype = gtf.GetBiotype();
            name = gtf.Attributes.StringAfter("gene_name \"").StringBefore("\"");
          }

          if (bHasGeneBiotype)
          {
            sw.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}", key, name, gtfs.Sum(m => m.Length), gtfs.First().Seqname, gtfs.Min(l => l.Start), gtfs.Max(l => l.End), biotype);
          }
          else
          {
            sw.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}\t{5}", key, name, gtfs.Sum(m => m.Length), gtfs.First().Seqname, gtfs.Min(l => l.Start), gtfs.Max(l => l.End));
          }
          swBed.WriteLine("{0}\t{1}\t{2}\t{3}_{4}", gtfs.First().Seqname, gtfs.Min(l => l.Start), gtfs.Max(l => l.End), key.StringBefore("."), name);
        }
      }

      return new string[] { options.OutputFile };
    }

    private static bool IsExon(GtfItem item)
    {
      return item.Feature.Equals("exon");
    }
  }
}
