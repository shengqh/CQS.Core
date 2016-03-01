using Bio.IO.SAM;
using CQS.Genome.Sam;
using RCPA;
using RCPA.Gui;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CQS.Genome.Mapping
{
  public class ChromosomeCountSlimItemBuilder : ProgressClass
  {
    private string preferPrefix;
    /// <summary>
    /// Constructor of ChromosomeCountSlimItemBuilder
    /// </summary>
    public ChromosomeCountSlimItemBuilder(string preferPrefix)
    {
      this.preferPrefix = preferPrefix;
    }

    public List<ChromosomeCountSlimItem> Build(string fileName)
    {
      var result = new List<ChromosomeCountSlimItem>();

      var queries = new Dictionary<string, SAMChromosomeItem>();
      var chromosomes = new Dictionary<string, ChromosomeCountSlimItem>();

      using (var sr = SAMFactory.GetReader(fileName, true))
      {
        int count = 0;
        int waitingcount = 0;
        string line;
        while ((line = sr.ReadLine()) != null)
        {
          if (count % 1000 == 0)
          {
            if (Progress.IsCancellationPending())
            {
              throw new UserTerminatedException();
            }
          }

          if (count % 100000 == 0 && count > 0)
          {
            Progress.SetMessage("{0} candidates from {1} reads", waitingcount, count);
          }

          count++;

          var parts = line.Split('\t');

          SAMFlags flag = (SAMFlags)int.Parse(parts[SAMFormatConst.FLAG_INDEX]);

          //unmatched
          if (flag.HasFlag(SAMFlags.UnmappedQuery))
          {
            continue;
          }

          var qname = parts[SAMFormatConst.QNAME_INDEX];
          SAMChromosomeItem query;
          if(!queries.TryGetValue(qname, out query))
          {
            query = new SAMChromosomeItem();
            query.Qname = qname;
            queries[qname] = query;
          }

          var seqname = parts[SAMFormatConst.RNAME_INDEX].StartsWith("chr") ? parts[SAMFormatConst.RNAME_INDEX].StringAfter("chr") : parts[SAMFormatConst.RNAME_INDEX];
          query.Chromosomes.Add(seqname);

          ChromosomeCountSlimItem item;
          if(!chromosomes.TryGetValue(seqname, out item))
          {
            item = new ChromosomeCountSlimItem();
            item.Names.Add(seqname);
            chromosomes[seqname] = item;
            result.Add(item);
          }
          item.Queries.Add(query);

          waitingcount++;
        }

        Progress.SetMessage("Finally, there are {0} candidates from {1} reads", waitingcount, count);
      }

      foreach(var query in queries.Values)
      {
        query.Chromosomes = query.Chromosomes.Distinct().OrderBy(m => m).ToList();
      }

      if (!string.IsNullOrEmpty(this.preferPrefix))
      {
        foreach (var query in queries.Values)
        {
          if(query.Chromosomes.Any(l => l.StartsWith(this.preferPrefix)))
          {
            var chroms = query.Chromosomes.Where(l => l.StartsWith(this.preferPrefix)).ToArray();
            foreach(var chrom in chroms)
            {
              chromosomes[chrom].Queries.Remove(query);
              query.Chromosomes.Remove(chrom);
            }
          }
        }

        result.RemoveAll(l => l.Queries.Count == 0);
      }
      return result;
    }
  }
}
