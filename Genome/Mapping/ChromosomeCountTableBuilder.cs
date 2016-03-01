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

      var format = new ChromosomeCountSlimItemXmlFormat();

      var countMap = new Dictionary<string, ChromosomeCountSlimItem>();

      int fileIndex = 0;
      foreach (var file in countFiles)
      {
        fileIndex++;
        Progress.SetMessage("Reading {0}/{1}: {2} ...", fileIndex, countFiles.Count, file.File);

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
            countMap[name].Queries.AddRange(c.Queries);
          }
          else
          {
            countMap[name] = c;
          }
        }
      }

      var counts = countMap.Values.ToList();

      Progress.SetMessage("Sorting queries in each chromosome for merging ...");
      counts.ForEach(l => l.Queries.Sort((m1, m2) => m1.Qname.CompareTo(m2.Qname)));
      //counts.Sort((m1,m2) => m2.Queries.Count.CompareTo(m1.Queries.Count));

      Progress.SetMessage("Merging, calculating and sorting ...");
      counts.MergeCalculateSortByEstimatedCount();

      Progress.SetMessage("Writing xml file {0} ...", options.OutputFile + ".xml");
      format.WriteToFile(options.OutputFile + ".xml", counts);

      Progress.SetMessage("Writing count file {0} ...", options.OutputFile);
      using (var sw = new StreamWriter(options.OutputFile))
      {
        sw.WriteLine("Name\t{0}", countFiles.ConvertAll(m => m.Name).Merge("\t"));
        foreach (var count in counts)
        {
          var individualCounts = (from f in countFiles
                                  let estimatedCount = count.Queries.Where(m => m.Sample.Equals(f.Name)).Sum(m => m.GetEstimatedCount()) * count.Names.Count
                                  select string.Format("{0:0.##}", estimatedCount)).Merge("\t");

          sw.WriteLine("{0}\t{1}", (from m in count.Names orderby m select m).Merge(";"), individualCounts);
        }
      }

      Progress.End();

      return new string[] { Path.GetFullPath(options.OutputFile) };
    }
  }
}
