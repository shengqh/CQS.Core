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

      WriteOutput(options.OutputFile, countFiles, format, counts);

      if (File.Exists(options.CategoryMapFile))
      {
        var categoryMap = new MapItemReader(0, 1).ReadFromFile(options.CategoryMapFile);
        var queries = new HashSet<SAMChromosomeItem>(from c in counts
                                                     from q in c.Queries
                                                     select q);

        var dic = new Dictionary<string, ChromosomeCountSlimItem>();
        foreach (var q in queries)
        {
          q.Chromosomes = (from chrom in q.Chromosomes
                           select categoryMap[chrom].Value).Distinct().OrderBy(m => m).ToList();
          foreach (var chrom in q.Chromosomes)
          {
            ChromosomeCountSlimItem item;
            if (!dic.TryGetValue(chrom, out item))
            {
              item = new ChromosomeCountSlimItem();
              item.Names.Add(chrom);
              dic[chrom] = item;
            }
            item.Queries.Add(q);
          }
        }

        WriteOutput(Path.ChangeExtension(options.OutputFile, ".category" + Path.GetExtension(options.OutputFile)), countFiles, format, dic.Values.ToList());
      }

      Progress.End();

      return new string[] { Path.GetFullPath(options.OutputFile) };
    }

    private void WriteOutput(string outputFile, List<FileItem> countFiles, ChromosomeCountSlimItemXmlFormat format, List<ChromosomeCountSlimItem> counts)
    {
      Progress.SetMessage("Sorting queries in each chromosome for merging ...");
      counts.ForEach(l => l.Queries.Sort((m1, m2) => m1.Qname.CompareTo(m2.Qname)));

      Progress.SetMessage("Merging, calculating and sorting ...");
      counts.MergeCalculateSortByEstimatedCount();

      Progress.SetMessage("Writing xml file {0} ...", outputFile + ".xml");
      format.WriteToFile(outputFile + ".xml", counts);

      Progress.SetMessage("Writing count file {0} ...", outputFile);
      using (var sw = new StreamWriter(outputFile))
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
    }
  }
}
