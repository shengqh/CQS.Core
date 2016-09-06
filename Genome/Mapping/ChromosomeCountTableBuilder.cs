using System;
using System.Collections.Generic;
using System.Linq;
using RCPA;
using System.IO;
using CQS.Genome.Sam;
using CQS.Genome.SmallRNA;

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
      var result = new List<string>();

      var countFiles = options.GetCountFiles();
      countFiles.Sort((m1, m2) => m1.Name.CompareTo(m2.Name));

      var format = new ChromosomeCountSlimItemXmlFormat(outputSample: true);

      var countMap = new Dictionary<string, ChromosomeCountSlimItem>();

      int fileIndex = 0;
      foreach (var file in countFiles)
      {
        fileIndex++;
        Progress.SetMessage("Reading {0}/{1}: {2} ...", fileIndex, countFiles.Count, file.File);

        var curcounts = format.ReadFromFile(file.File);

        if (string.IsNullOrEmpty(curcounts[0].Queries[0].Sequence))
        {
          Console.WriteLine("Didn't read in the sequence of query " + curcounts[0].Queries[0].Qname);
        }
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
          ChromosomeCountSlimItem item;
          if (countMap.TryGetValue(name, out item))
          {
            item.Queries.AddRange(c.Queries);
          }
          else
          {
            countMap[name] = c;
          }
        }
      }

      var counts = countMap.Values.ToList();

      WriteOutput(options.OutputFile, countFiles, format, counts);

      result.Add(options.OutputFile);

      if (File.Exists(options.CategoryMapFile))
      {
        Progress.SetMessage("Reading category map ...");
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

        var catFile = Path.ChangeExtension(options.OutputFile, ".category" + Path.GetExtension(options.OutputFile));
        WriteOutput(catFile, countFiles, format, dic.Values.ToList());
        result.Add(catFile);
      }

      if (options.OutputReadTable || options.OutputReadContigTable)
      {
        Progress.SetMessage("Building sequence map...");
        var reads = SmallRNASequenceUtils.ConvertFrom(counts);

        if (options.OutputReadTable)
        {
          Progress.SetMessage("Saving read file...");
          var readOutput = Path.ChangeExtension(options.OutputFile, ".read" + Path.GetExtension(options.OutputFile));
          new SmallRNASequenceFormat(int.MaxValue, false).WriteToFile(readOutput, reads);
          result.Add(readOutput);
        }

        if (options.OutputReadContigTable)
        {
          Progress.SetMessage("Building sequence contig by similarity ...");
          var contigs = SmallRNASequenceUtils.BuildContigByIdenticalSimilarity(reads, int.MaxValue, progress: Progress);

          Progress.SetMessage("Contig number = {0}", contigs.Count);

          Progress.SetMessage("Saving contig file...");
          var contigOutput = Path.ChangeExtension(options.OutputFile, ".contig" + Path.GetExtension(options.OutputFile));
          new SmallRNASequenceContigFormat().WriteToFile(contigOutput, contigs);
          result.Add(contigOutput);

          Progress.SetMessage("Saving sequence contig details...");
          new SmallRNASequenceContigDetailFormat().WriteToFile(contigOutput + ".details", contigs);
          result.Add(contigOutput + ".details");
        }
      }

      Progress.End();

      return result;
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
