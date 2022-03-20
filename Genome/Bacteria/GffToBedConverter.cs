using CQS.Genome.Gtf;
using RCPA;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CQS.Genome.Bacteria
{
  public class GffToBedConverter : AbstractThreadProcessor
  {
    private GffToBedConverterOptions options;

    public GffToBedConverter(GffToBedConverterOptions options)
    {
      this.options = options;
    }

    public override IEnumerable<string> Process()
    {
      var items = ReadGtfItems();
      items.RemoveAll(m => m.Feature.Equals("region"));
      for (int i = items.Count - 1; i > 0; i--)
      {
        for (int j = i - 1; j >= 0; j--)
        {
          var res = items[i].Contains(items[j]);
          if (res == -1)
          {
            items.RemoveAt(i);
            break;
          }
        }
      }

      var groups = items.ToGroupDictionary(m => m.GetLocation());
      foreach (var g in groups.Values)
      {
        if (g.Any(l => !IsCDS(l) && !IsExon(l) && !IsGene(l)))
        {
          g.RemoveAll(l => IsCDS(l) || IsExon(l) || IsGene(l));
          g.ForEach(l =>
          {
            l.Name = l.Feature + ":" + l.Attributes.StringAfter("ID=").StringBefore(";");
            if (l.Attributes.Contains("product="))
            {
              var product = l.Attributes.StringAfter("product=").StringBefore(";");
              if (!product.Contains(" "))
              {
                l.Name = l.Name + ":" + product;
              }
            }
          });
        }
        else
        {
          if (g.Any(l => IsCDS(l)))
          {
            g.RemoveAll(l => IsGene(l));
          }
          g.ForEach(l => l.Name = l.Feature + ":" + l.Attributes.StringAfter("Name=").StringBefore(";"));
        }

        if (g.Count > 1)
        {
          Console.WriteLine(g[0].GetLocation() + " : " + (from l in g select l.Feature).Merge("/"));
        }
      }

      var values = groups.Values.ToList();
      GenomeUtils.SortChromosome(values, m => m[0].Seqname, m => m[0].Start);
      using (StreamWriter sw = new StreamWriter(options.OutputFile))
      {
        foreach (var value in values)
        {
          foreach (var gtf in value)
          {
            sw.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}\t{5}",
              gtf.Seqname,
              gtf.Start - 1,
              gtf.End,
              gtf.Name,
              0,
              gtf.Strand);
          }
        }
      }

      return new string[] { options.OutputFile };
    }

    private static bool IsGene(GtfItem l)
    {
      return l.Feature.Equals("gene");
    }

    private static bool IsExon(GtfItem l)
    {
      return l.Feature.Equals("exon");
    }

    private static bool IsCDS(GtfItem l)
    {
      return l.Feature.Equals("CDS");
    }

    public List<GtfItem> ReadGtfItems()
    {
      List<GtfItem> result = new List<GtfItem>();

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

          item.Name = item.Attributes.StringAfter("locus_tag=").StringBefore(";");
          result.Add(item);
        }
      }

      return result;
    }
  }
}
