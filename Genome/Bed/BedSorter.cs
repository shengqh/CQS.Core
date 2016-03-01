using CQS.Genome.Gtf;
using RCPA;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CQS.Genome.Bed
{
  public class BedSorter : AbstractThreadProcessor
  {
    private BedSorterOptions _options;

    public BedSorter(BedSorterOptions options)
    {
      this._options = options;
    }

    private class BedEntry
    {
      public string Seqname { get; set; }
      public int Start { get; set; }
      public int End { get; set; }
      public String Line { get; set; }
    }

    public override IEnumerable<string> Process()
    {
      Progress.SetMessage("Reading " + _options.InputFile + " ...");
      var items = (from line in FileUtils.ReadFileLines(_options.InputFile)
                   where !string.IsNullOrWhiteSpace(line)
                   let parts = line.Split('\t')
                   select new BedEntry() { Seqname = parts[0], Start = int.Parse(parts[1]), End = int.Parse(parts[2]), Line = line }).ToList();

      var chrInBed = new List<string>();
      foreach (var item in items)
      {
        if (chrInBed.Contains(item.Seqname))
        {
          continue;
        }
        chrInBed.Add(item.Seqname);
      }

      Progress.SetMessage("Total {0} bed entries with {1} sequence names: {2}", items.Count, chrInBed.Count, chrInBed.Merge(","));

      var baditems = items.Where(m => m.Start >= m.End).Count();
      Progress.SetMessage("{0} entries whose start position was larger than or equals to end position were removed.", baditems);
      items.RemoveAll(m => m.Start >= m.End);

      var chrs = (from line in File.ReadAllLines(_options.DictFile).Skip(1)
                  where !string.IsNullOrWhiteSpace(line)
                  let parts = line.Split('\t')
                  let chr = parts[1].StringAfter("SN:")
                  select chr).ToList();
      Progress.SetMessage("Target sequence names: {0}", chrs.Merge(","));
      var chrHash = new HashSet<string>(chrs);

      var itemCount = items.Where(m => chrHash.Contains(m.Seqname)).Count();
      if (itemCount == 0)
      {
        if (chrs.All(m => m.StartsWith("chr")) && items.Any(m => !m.Seqname.StartsWith("chr")))
        {
          Progress.SetMessage("Add 'chr' to sequence name of bed entries.");
          items.ForEach(m =>
          {
            if (!m.Seqname.StartsWith("chr"))
            {
              m.Seqname = "chr" + m.Seqname;
            }
          });
        }
        else if (chrs.All(m => !m.StartsWith("chr")) && items.Any(m => m.Seqname.StartsWith("chr")))
        {
          Progress.SetMessage("Remove 'chr' from sequence name of bed entries.");
          items.ForEach(m => m.Seqname = m.Seqname.StringAfter("chr"));
        }
      }

      items.RemoveAll(m => !chrHash.Contains(m.Seqname));
      if (items.Count == 0)
      {
        throw new Exception("All sequence name of bed entries were not found in genome dict file!");
      }

      Progress.SetMessage("Saving {0} entries to {1} ...", items.Count, _options.OutputFile);
      using (var sw = new StreamWriter(_options.OutputFile))
      {
        foreach (var chr in chrs)
        {
          var chrItems = items.Where(m => m.Seqname.Equals(chr)).OrderBy(m => m.Start).ToArray();
          foreach (var item in chrItems)
          {
            sw.WriteLine(item.Line);
          }
        }
      }

      return new string[] { _options.OutputFile };
    }
  }
}

