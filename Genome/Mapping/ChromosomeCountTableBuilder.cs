using System;
using System.Collections.Generic;
using System.Linq;
using RCPA;
using System.IO;

namespace CQS.Genome.Mapping
{
  public class ChromosomeCountTableBuilder : AbstractThreadProcessor
  {
    private SimpleDataTableBuilderOptions options;

    public ChromosomeCountTableBuilder(SimpleDataTableBuilderOptions options)
    {
      this.options = options;
    }

    public override IEnumerable<string> Process()
    {
      var countFiles = options.GetCountFiles();
      countFiles.Sort((m1, m2) => m1.Name.CompareTo(m2.Name));

      var countMap = new Dictionary<string, ChromosomeCountItem>();

      foreach (var file in options.GetCountFiles())
      {
        var curcounts = new ChromosomeCountXmFormat().ReadFromFile(file.File);
        curcounts.ForEach(m =>
        {
          foreach (var q in m.Queries)
          {
            q.Sample = file.Name;
          }
        });

        foreach (var c in curcounts)
        {
          var name = c.Names.First();
          if (countMap.ContainsKey(name))
          {
            foreach (var q in c.Queries)
            {
              countMap[name].Queries.Add(q);
            }
          }
          else
          {
            countMap[name] = c;
          }
        }
      }

      var counts = countMap.Values.ToList();
      counts.Sort((m1, m2) => m2.QueryCount.CompareTo(m1.QueryCount));
      counts.MergeItems();

      using (var sw = new StreamWriter(options.OutputFile))
      {
        sw.WriteLine("Name\t{0}", countFiles.ConvertAll(m => m.Name).Merge("\t"));
        foreach (var mirna in counts)
        {
          if (mirna.Names.Any(m => m.StartsWith("hsa")))
          {
            mirna.Names.RemoveWhere(m => !m.StartsWith("hsa"));
          }

          var individualCounts = (from f in countFiles
                                  select mirna.Queries.Where(m => m.Sample.Equals(f.Name)).Sum(m => m.QueryCount).ToString()).Merge("\t");
          sw.WriteLine("{0}\t{1}", (from m in mirna.Names orderby m select m).Merge(";"), individualCounts);
        }
      }

      Progress.End();

      return new string[] { Path.GetFullPath(options.OutputFile) };
    }
  }
}
