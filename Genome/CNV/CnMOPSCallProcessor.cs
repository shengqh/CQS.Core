using RCPA;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CQS.Genome.CNV
{
  public class CnMOPSCallProcessor : AbstractThreadProcessor
  {
    public class ItemRange : SequenceRegion
    {
      public ItemRange()
      {
        this.Items = new List<CnMOPsItem>();
      }

      public List<CnMOPsItem> Items { get; private set; }
    }

    private CnMOPSCallProcessorOptions options;

    public CnMOPSCallProcessor(CnMOPSCallProcessorOptions options)
    {
      this.options = options;
    }

    public override IEnumerable<string> Process()
    {
      var data = new CnMOPsItemReader().ReadFromFile(options.InputFile);

      Dictionary<string, List<ItemRange>> result = MergeRange(data);
      var seqnames = result.Keys.OrderBy(m => m).ToList();
      GenomeUtils.SortChromosome(seqnames, l => l, l => 1);

      using (var sw = new StreamWriter(options.OutputFile))
      {
        sw.WriteLine("seqname\tstart\tend\tlocus\tsample\tsample_start\tsample_end\tsample_type");

        foreach (var seqname in seqnames)
        {
          var ranges = result[seqname];
          foreach (var range in ranges)
          {
            foreach (var cn in range.Items)
            {
              if (options.IgnoreCN1CN3 && (cn.CN.Equals("CN1") || cn.CN.Equals("CN3")))
              {
                continue;
              }
              sw.WriteLine("{0}\t{1}\t{2}\t{0}:{1}-{2}\t{3}\t{4}\t{5}\t{6}",
                seqname, range.Start, range.End, cn.FileName, cn.Start, cn.End, cn.CN);
            }
          }
        }
      }

      var filenames = (from d in data select d.FileName).Distinct().OrderBy(l => l).ToArray();

      using (var sw = new StreamWriter(options.OutputFile + ".cnvr"))
      {
        sw.WriteLine("seqname\tstart\tend\tfile\t{0}", filenames.Merge("\t"));

        foreach (var seqname in seqnames)
        {
          var ranges = result[seqname];
          foreach (var range in ranges)
          {
            var cns = (from filename in filenames
                       let cn = range.Items.Where(l => l.FileName.Equals(filename)).FirstOrDefault()
                       select cn == null ? "CN2" : cn.CN).ToArray();
            if (options.IgnoreCN1CN3 && cns.All(l => l.Equals("CN1") || l.Equals("CN2") || l.Equals("CN3")))
            {
              continue;
            }

            sw.WriteLine("{0}\t{1}\t{2}\t{0}_{1}_{2}\t{3}",
              seqname, range.Start, range.End, cns.Merge("\t"));
          }
        }
      }

      return new[] { options.OutputFile, options.OutputFile + ".cnvr" };
    }

    public static Dictionary<string, List<ItemRange>> MergeRange(List<CnMOPsItem> data)
    {
      GenomeUtils.SortChromosome(data, m => m.Seqname, m => m.Start);

      Dictionary<string, List<ItemRange>> result = new Dictionary<string, List<ItemRange>>();
      foreach (var d in data)
      {
        if (!result.ContainsKey(d.Seqname))
        {
          result[d.Seqname] = new List<ItemRange>();
        }

        var ranges = result[d.Seqname];

        bool bFound = false;
        foreach (var range in ranges)
        {
          if (range.Overlap(d, 0))
          {
            range.Items.Add(d);
            range.Start = Math.Min(range.Start, d.Start);
            range.End = Math.Max(range.End, d.End);
            bFound = true;
            break;
          }
        }

        if (!bFound)
        {
          var range = new ItemRange();
          range.Items.Add(d);
          range.Seqname = d.Seqname;
          range.Start = d.Start;
          range.End = d.End;
          ranges.Add(range);
        }
      }

      return result;
    }
  }
}