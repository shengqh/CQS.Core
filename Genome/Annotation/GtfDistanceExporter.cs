using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CQS.Genome.Bed;
using CQS.Genome.Tophat;
using RCPA;
using CQS.Genome.Gtf;

namespace CQS.Genome.Annotation
{
  public class GtfDistanceExporter : IAnnotationTsvExporter
  {
    /// <summary>
    /// Dictionary of Chromosome/GTFItems
    /// </summary>
    private Dictionary<string, List<GtfItem>> maps;

    private string header = null;

    private string emptyStr = null;

    public GtfDistanceExporter(string gtfFile, string gtfKey = "exon")
    {
      Console.WriteLine("reading gtf file " + gtfFile + " ...");
      this.maps = CollectionUtils.ToGroupDictionary(GtfItemFile.ReadFromFile(gtfFile, gtfKey), m => m.Seqname.StringAfter("chr"));
      Console.WriteLine("reading gtf file " + gtfFile + " done");

      this.header = string.Format("distance_{0}\tdistance_{0}_position\tdistance_gene\tdistance_in_gene\tdistance_in_gene_range", gtfKey);
      this.emptyStr = "\t";

      //sort the gtf items by locus
      foreach (var lst in maps.Values)
      {
        lst.Sort((m1, m2) => m1.Start.CompareTo(m2.Start));
      }
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

      long minAbsoluteDistance = int.MaxValue;
      long minDistance = int.MaxValue;
      GtfItem distItem = null;
      bool bStart = true;
      foreach (var item in items)
      {
        var disStart = start - item.Start;
        var absDisStart = Math.Abs(disStart);
        if (absDisStart < minAbsoluteDistance)
        {
          minAbsoluteDistance = absDisStart;
          minDistance = disStart;
          distItem = item;
          bStart = true;
        }

        var disEnd = start - item.End;
        var absDisEnd = Math.Abs(disEnd);
        if (absDisEnd < minAbsoluteDistance)
        {
          minAbsoluteDistance = absDisEnd;
          minDistance = disEnd;
          distItem = item;
          bStart = false;
        }

        if (disStart < 0 && disEnd < 0)
        {
          break;
        }
      }

      var position = distItem.GetNameExon() + ":" + (bStart ? "start" : "end");

      string gene = string.Empty;
      string gene_func = string.Empty;
      if (bStart && distItem.ExonNumber <= 1 && minDistance < 0)
      {
      }
      else
      {
        var maxExon = items.Where(m => m.Name.Equals(distItem.Name)).Select(m => m.ExonNumber).Max();
        if (!bStart && distItem.ExonNumber == maxExon && minDistance > 0)
        {
        }
        else
        {
          gene = distItem.Name;
          if ((bStart && minDistance < 0) || (!bStart && minDistance > 0))
          {
            gene_func = "intron";
          }
          else
          {
            gene_func = "exon";
          }
        }
      }
      return string.Format("{0}\t{1}\t{2}\t{3}", minDistance, position, gene, gene_func);
    }
  }
}
