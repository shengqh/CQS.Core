using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CQS.Genome.Sam;
using RCPA;
using CQS.Genome.Mirna;
using CQS.Genome.SmallRNA;

namespace CQS.Genome.Mapping
{
  public class ChromosomeCountProcessor : AbstractThreadProcessor
  {
    private ChromosomeCountProcessorOptions options;

    public ChromosomeCountProcessor(ChromosomeCountProcessorOptions options)
    {
      this.options = options;
    }

    public override IEnumerable<string> Process()
    {
      var result = new List<string>();

      var builder = options.GetCandidateBuilder();
      var queries = new HashSet<string>();
      var samitems = builder.Build<SAMAlignedItem>(options.InputFile, out queries);
      if (options.Offsets.Count > 0 && options.Offsets[0] != 0)
      {
        Progress.SetMessage("Filter mapped reads with offsets {0} ...", (from offset in options.Offsets select offset.ToString()).Merge(","));
        var offsets = new HashSet<long>(options.Offsets);
        samitems.ForEach(m =>
        {
          m.RemoveLocation(l => !offsets.Contains(l.Start));
        });
        samitems.RemoveAll(m => m.Locations.Count == 0);
        Progress.SetMessage("Total {0} mapped reads kept.");
      }

      var cm = options.GetCountMap();

      //Assume that the sample is from human tissue, the prefered prefix will be hsa.
      //If one read mapped to multiple features belongs to multiple species including human, only the features from human will be kept.
      if (!string.IsNullOrEmpty(options.PreferPrefix))
      {
        samitems.ForEach(m =>
        {
          if (m.Locations.Any(l => IsPreferPrefix(l)))
          {
            m.RemoveLocation(l => !IsPreferPrefix(l));
          }
        });
      }

      //If one read perfectly mapped to human genome and it is not mapped to human miRNA, it will be discarded.
      if (File.Exists(options.PerfectMappedNameFile))
      {
        Progress.SetMessage("Filter mapped reads with query perfectly mapped to genome ...");

        var removedReads = new HashSet<string>();
        var names = File.ReadAllLines(options.PerfectMappedNameFile);
        if (names.Any(l => l.Contains(SmallRNAConsts.NTA_TAG)))
        {
          foreach (var name in names)
          {
            //only the original read perfect mapped to genome will be considered
            if (string.IsNullOrEmpty(name.StringAfter(SmallRNAConsts.NTA_TAG)))
            {
              removedReads.Add(name.StringBefore(SmallRNAConsts.NTA_TAG));
            }
          }
        }
        else
        {
          removedReads = new HashSet<string>(names);
        }

        if (!string.IsNullOrEmpty(options.PreferPrefix))
        {
          //keep all reads with perferName
          samitems.RemoveAll(m =>
          {
            if (!m.Locations.Any(l => IsPreferPrefix(l)))
            {
              if (removedReads.Contains(m.Qname))
              {
                return true;
              }
            }

            return false;
          });
        }
        else
        {
          //remove all perfect mapped 
          samitems.RemoveAll(m => removedReads.Contains(m.Qname));
        }

        Progress.SetMessage("Total {0} mapped reads kept.", samitems.Count);
      }

      samitems.ForEach(m =>
      {
        m.QueryCount = cm.GetCount(m.Qname);
        m.SortLocations();
      });

      var locs = (from s in samitems
                  from l in s.Locations
                  select l).ToList();

      var chroms = (from g in locs.GroupBy(m => m.Seqname)
                    select new ChromosomeCountItem() { Names = new string[] { g.Key }.ToList(), Queries = (from l in g select l.Parent).ToList() }).ToList();
      chroms.Sort((m1, m2) => m2.Queries.Count.CompareTo(m1.Queries.Count));

      var mappedfile = options.OutputFile + ".mapped.xml";
      new ChromosomeCountXmlFormat().WriteToFile(mappedfile, chroms);

      chroms.CalculateAndSortByEstimatedCount();

      using (var sw = new StreamWriter(options.OutputFile))
      {
        if (!string.IsNullOrEmpty(options.PreferPrefix))
        {
          foreach (var chr in chroms)
          {
            if (chr.Names.Any(m => m.StartsWith(options.PreferPrefix)))
            {
              chr.Names.RemoveAll(m => !m.StartsWith(options.PreferPrefix));
            }
          }
        }

        sw.WriteLine("Name\tEstimatedCount\tQueryCount");
        foreach (var chr in chroms)
        {
          sw.WriteLine("{0}\t{1:0.##}\t{2}", (from m in chr.Names orderby m select m).Merge(";"),
            chr.EstimatedCount,
            chr.QueryCount);
        }
      }

      Progress.End();

      return result;
    }


    private bool IsPreferPrefix(SamAlignedLocation l)
    {
      return l.Seqname.StartsWith(options.PreferPrefix);
    }
  }
}