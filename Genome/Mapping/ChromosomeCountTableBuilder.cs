using System;
using System.Collections.Generic;
using System.Linq;
using RCPA;
using System.IO;
using CQS.Genome.Sam;

namespace CQS.Genome.Mapping
{
  public class ChromosomeCountTableBuilder : AbstractThreadProcessor
  {
    private ChromosomeCountTableBuilderOptions options;

    public ChromosomeCountTableBuilder(ChromosomeCountTableBuilderOptions options)
    {
      this.options = options;
    }

    public override IEnumerable<string> Process()
    {
      var countFiles = options.GetCountFiles();
      countFiles.Sort((m1, m2) => m1.Name.CompareTo(m2.Name));

      var format = new ChromosomeCountXmlFormat();

      var countMap = new Dictionary<string, ChromosomeCountItem>();

      foreach (var file in options.GetCountFiles())
      {
        var curcounts = format.ReadFromFile(file.File);
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
            countMap[name].UnionQueryWith(c.Queries);
          }
          else
          {
            countMap[name] = c;
          }
        }
      }

      var counts = countMap.Values.ToList();

      counts.CalculateAndSortByEstimatedCount();

      //if (options.ExportXml)
      //{
      //  Progress.SetMessage("Writing mapped details to " + options.OutputFile + ".mapped.xml ...");
      //  format.WriteToFile(options.OutputFile + ".mapped.xml", counts);
      //}

      Progress.SetMessage("Writing count to " + options.OutputFile + " ...");
      using (var sw = new StreamWriter(options.OutputFile))
      {
        sw.WriteLine("Name\t{0}", countFiles.ConvertAll(m => m.Name).Merge("\t"));
        foreach (var count in counts)
        {
          //if (mirna.Names.Any(m => m.StartsWith("hsa")))
          //{
          //  mirna.Names.RemoveWhere(m => !m.StartsWith("hsa"));
          //}

          var individualCounts = (from f in countFiles
                                  let queries = count.Queries.Where(m => m.Sample.Equals(f.Name))
                                  let cr = new ChromosomeCountItem()
                                  {
                                    Names = count.Names,
                                    Queries = queries.ToList()
                                  }
                                  let ec = cr.CalculateEstimatedCount()
                                  select string.Format("{0:0.##}", ec)).Merge("\t");

          sw.WriteLine("{0}\t{1}", (from m in count.Names orderby m select m).Merge(";"), individualCounts);
        }
      }

      Progress.End();

      return new string[] { Path.GetFullPath(options.OutputFile) };
    }
  }
}
