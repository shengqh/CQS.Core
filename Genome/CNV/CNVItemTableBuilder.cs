using CQS.Genome.Bed;
using RCPA;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CQS.Genome.CNV
{
  public class CNVItemTableBuilder : AbstractThreadProcessor
  {
    private CNVItemTableBuilderOptions options;

    public CNVItemTableBuilder(CNVItemTableBuilderOptions options)
    {
      this.options = options;
    }

    public override IEnumerable<string> Process()
    {
      var hasheader = new StreamReader(options.BedFile).ReadLine().Contains("start");
      var beds = new BedItemFile<BedItem>() { HasHeader = hasheader }.ReadFromFile(options.BedFile);
      var items = new CNVItemReader<CNVItem>().ReadFromFile(options.InputFile);
      var itemsgroup = items.GroupBy(m => m.Seqname.StringAfter("chr"));
      var bedgroups = beds.GroupBy(m => m.Seqname).ToDictionary(m => m.Key);

      foreach (var ig in itemsgroup)
      {
        if (!bedgroups.ContainsKey(ig.Key))
        {
          throw new Exception(string.Format("Cannot find chromosome {0} in bed file {1}", ig.Key, options.BedFile));
        }
      }

      foreach (var ig in itemsgroup)
      {
        var bg = bedgroups[ig.Key];
        foreach (var item in ig)
        {
          var bgmax = FindMaxOverlap(bg, item);
          if (bgmax != null)
          {
            item.ItemName = bgmax.Name;
          }
        }
      }

      items.RemoveAll(m => string.IsNullOrWhiteSpace(m.ItemName));
      var itemmap = items.GroupBy(m => m.ItemName).ToDictionary(m => m.Key);
      var genes = itemmap.Keys.OrderBy(m => m).ToList();
      var samples = items.ConvertAll(m => m.FileName).Distinct().OrderBy(m => m).ToList();

      using (var sw = new StreamWriter(options.OutputFile))
      {
        sw.WriteLine("gene,DELETION,DUPLICATION," + samples.Merge(","));
        foreach (var gene in genes)
        {
          var sas = itemmap[gene].GroupBy(m => m.FileName).ToDictionary(m => m.Key);
          sw.Write(gene);
          var deletioncount = samples.Count(m => sas.ContainsKey(m) && sas[m].Any(n => n.ItemType == CNVType.DELETION));
          var duplicationcount = samples.Count(m => sas.ContainsKey(m) && sas[m].Any(n => n.ItemType == CNVType.DUPLICATION));
          sw.Write(",{0:0.0}%,{1:0.0}%", deletioncount * 100.0 / samples.Count, duplicationcount * 100.0 / samples.Count);

          foreach (var sample in samples)
          {
            sw.Write(",");
            if (sas.ContainsKey(sample))
            {
              var sis = sas[sample];
              if (sis.Any(m => !m.ItemType.Equals(sis.First().ItemType)))
              {
                Console.WriteLine("Different type of gene " + gene + " in sample " + sample);
              }
              sw.Write((from si in sis
                        select string.Format("{0}:{1}:{2}-{3}", si.ItemType, si.Seqname, si.Start, si.End)).Merge("/"));
            }
          }
          sw.WriteLine();
        }
      }

      return new string[] { options.OutputFile };
    }

    private long GetOverlap(BedItem bi, CNVItem ci)
    {
      if (bi.End < ci.Start)
      {
        return 0;
      }

      if (ci.End < bi.Start)
      {
        return 0;
      }

      return Math.Min(ci.End, bi.End) - Math.Max(ci.Start, bi.Start);
    }

    private BedItem FindMaxOverlap(IGrouping<string, BedItem> bg, CNVItem ci)
    {
      BedItem result = null;
      long maxoverlap = 0;
      foreach (var bi in bg)
      {
        var overlapped = GetOverlap(bi, ci);
        if (overlapped > maxoverlap)
        {
          result = bi;
          maxoverlap = overlapped;
        }
      }
      return result;
    }
  }
}
