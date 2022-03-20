using CQS.Genome.Mapping;
using CQS.Genome.Mirna;
using RCPA;
using RCPA.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CQS.Genome.SmallRNA
{
  public class SmallRNACategoryGroupBuilder : AbstractThreadProcessor
  {
    private class QueryRecord
    {
      public QueryRecord(string query, string biotype, string subtype, string name, int queryCount)
      {
        this.Query = query;
        this.Biotype = biotype;
        this.Subtype = subtype;
        this.Name = name;
        this.QueryCount = queryCount;
      }

      public string Query { get; set; }
      public string Biotype { get; set; }
      public string Subtype { get; set; }
      public string Name { get; set; }
      public int QueryCount { get; set; }
    }

    private class CategoryCount
    {
      public CategoryCount(string biotype, string subtype, int count)
      {
        this.Biotype = biotype;
        this.Subtype = subtype;
        this.Count = count;
      }

      public string Biotype { get; set; }
      public string Subtype { get; set; }
      public int Count { get; set; }
    }

    private SmallRNACategoryGroupBuilderOptions options;

    public SmallRNACategoryGroupBuilder(SmallRNACategoryGroupBuilderOptions options)
    {
      this.options = options;
    }

    public override IEnumerable<string> Process()
    {
      var entries = (from line in File.ReadAllLines(options.InputFile)
                     let parts = line.Split('\t')
                     where parts.Length >= 3
                     let mirna = parts.Length == 3 ? string.Empty : parts[3]
                     select new { GroupName = parts[0], SampleName = parts[1], SmallRNAFile = parts[2], MiRNAFile = mirna }).ToList();

      if (entries.All(m => !File.Exists(m.MiRNAFile)))
      {
        return new SmallRNACategoryGroupPlusBuilder(options)
        {
          Progress = this.Progress
        }.Process();
      }

      var groups = entries.GroupBy(m => m.GroupName).ToList();

      var result = new List<string>();

      foreach (var group in groups)
      {
        var catfile = Path.Combine(options.OutputDirectory, group.Key + ".catcount");
        result.Add(catfile);
        using (var sw = new StreamWriter(catfile))
        {
          sw.WriteLine("SampleName\tCategory\tLevel\tCount");

          foreach (var entry in group)
          {
            Progress.SetMessage("Reading smallRNA mapped file " + entry.SmallRNAFile + " ...");
            var others = new MappedItemGroupXmlFileFormat().ReadFromFile(entry.SmallRNAFile);

            var otherQueries = (from g in others
                                from m in g
                                from mr in m.MappedRegions
                                from loc in mr.AlignedLocations
                                select new QueryRecord(loc.Parent.Qname,
                                  m.Name.StringBefore(":"),
                                  m.Name.StringAfter(":").StringBefore(":"),
                                  m.Name.StringAfter(":").StringAfter(":"),
                                  loc.Parent.QueryCount)).ToGroupDictionary(m => m.Query);
            Progress.SetMessage("Reading smallRNA mapped file finished, {0} queries mapped.", otherQueries.Count);

            //2570-KCV-01-19.bam.count.mapped.xml => 2570-KCV-01-19.bam.info
            var infofile = Path.Combine(Path.GetDirectoryName(entry.SmallRNAFile), Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(entry.SmallRNAFile))) + ".info");
            if (File.Exists(entry.MiRNAFile))
            {
              infofile = Path.Combine(Path.GetDirectoryName(entry.MiRNAFile), Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(entry.MiRNAFile))) + ".info");

              Progress.SetMessage("Reading miRNA mapped file " + entry.MiRNAFile + " ...");
              var mirnas = new MappedMirnaGroupXmlFileFormat().ReadFromFile(entry.MiRNAFile);
              var mirnaQueries = (from g in mirnas
                                  from m in g
                                  from mr in m.MappedRegions
                                  from mapped in mr.Mapped.Values
                                  from loc in mapped.AlignedLocations
                                  select new QueryRecord(loc.Parent.Qname.StringBefore(":CLIP_"),
                                    "miRNA",
                                    "miRNA",
                                    m.Name,
                                    loc.Parent.QueryCount)).ToGroupDictionary(m => m.Query);
              Progress.SetMessage("Reading miRNA mapped file finished, {0} queries mapped.", mirnaQueries.Count);

              foreach (var q in mirnaQueries)
              {
                List<QueryRecord> rec;
                if (!otherQueries.TryGetValue(q.Key, out rec))
                {
                  rec = q.Value;
                  otherQueries[q.Key] = q.Value;
                }
                else
                {
                  rec.AddRange(q.Value);
                }
              }
              Progress.SetMessage("Total {0} queries mapped.", otherQueries.Count);
            }

            var counts = new List<CategoryCount>();
            FillCounts(counts, options.Categories, otherQueries);

            var othercategories = (from v in otherQueries.Values
                                   from item in v
                                   select item.Biotype).Distinct().OrderBy(m => m).ToList();

            FillCounts(counts, othercategories, otherQueries);

            if (File.Exists(infofile))
            {
              var lines = File.ReadAllLines(infofile);

              Progress.SetMessage("reading mapping information from " + infofile + " ...");

              int totalReads = 0;
              int mappedReads = 0;
              foreach (var line in lines)
              {
                if (line.StartsWith("TotalReads"))
                {
                  totalReads = int.Parse(line.StringAfter("\t"));
                }
                else if (line.StartsWith("MappedReads"))
                {
                  mappedReads = int.Parse(line.StringAfter("\t"));
                }
              }

              var smallRNAReads = counts.Sum(m => m.Count);

              sw.WriteLine("{0}\tTotal Reads\t0\t{1}", entry.SampleName, totalReads);
              sw.WriteLine("{0}\tMapped Reads\t0\t{1}", entry.SampleName, mappedReads);
              sw.WriteLine("{0}\tsmall RNA\t0\t{1}", entry.SampleName, smallRNAReads);

              sw.WriteLine("{0}\tUnmapped\t1\t{1}", entry.SampleName, totalReads - mappedReads);
              sw.WriteLine("{0}\tOther Mapped\t1\t{1}", entry.SampleName, mappedReads - smallRNAReads);
              sw.WriteLine("{0}\tsmall RNA\t1\t{1}", entry.SampleName, smallRNAReads);
            }

            foreach (var rec in counts)
            {
              sw.WriteLine("{0}\t{1}\t{2}\t{3}", entry.SampleName, rec.Biotype, 2, rec.Count);
            }
          }
        }

        var data = (from line in File.ReadAllLines(catfile).Skip(1)
                    where !string.IsNullOrWhiteSpace(line)
                    let parts = line.Split('\t')
                    let level = double.Parse(parts[2])
                    where !(parts[1].Equals("small RNA") && level == 1)
                    select new
                    {
                      SampleName = parts[0],
                      Category = parts[1],
                      Level = level,
                      Count = int.Parse(parts[3])
                    }).ToList();

        var tablefile = catfile + ".tsv";
        result.Add(tablefile);
        using (var sw = new StreamWriter(tablefile))
        {
          var samples = (from d in data
                         select d.SampleName).Distinct().OrderBy(m => m).ToList();
          sw.WriteLine("Category\t{0}", samples.Merge("\t"));

          var categories = (from d in data
                            where d.Level == 2
                            select d.Category).Distinct().OrderBy(m => m).ToList();
          categories.Insert(0, "small RNA");
          categories.Insert(0, "Other Mapped");
          categories.Insert(0, "Unmapped");
          categories.Insert(0, "Mapped Reads");
          categories.Insert(0, "Total Reads");

          Console.WriteLine(categories.Merge("\n"));

          var map = data.ToDoubleDictionary(m => m.SampleName, m => m.Category);
          foreach (var cat in categories)
          {
            sw.WriteLine("{0}\t{1}", cat,
              (from sample in samples
               let dic = map[sample]
               select dic.ContainsKey(cat) ? dic[cat].Count.ToString() : "").Merge("\t"));
          }
        }

        var rfile = new FileInfo(FileUtils.GetTemplateDir() + "/smallrna_category_group.r").FullName;
        if (File.Exists(rfile))
        {
          var targetrfile = catfile + ".r";
          using (var sw = new StreamWriter(targetrfile))
          {
            sw.WriteLine("catfile<-\"{0}\"", catfile);
            sw.WriteLine("outputdir<-\"{0}\"", options.OutputDirectory);
            sw.WriteLine("ispdf<-{0}", options.PdfGraph ? "1" : "0");
            string line = File.ReadAllText(rfile);
            using (var sr = new StreamReader(rfile))
            {
              if (line.Contains("#predefine_end"))
              {
                while ((line = sr.ReadLine()) != null)
                {
                  if (line.Contains("#predefine_end"))
                  {
                    break;
                  }
                }
              }

              while ((line = sr.ReadLine()) != null)
              {
                sw.WriteLine(line);
              }
            }
          }
          SystemUtils.Execute("R", "--vanilla --slave -f \"" + targetrfile + "\"");
        }
      }
      return result;
    }

    private static void FillCounts(List<CategoryCount> counts, IEnumerable<string> cats, Dictionary<string, List<QueryRecord>> otherQueries)
    {
      foreach (var category in cats)
      {
        var count = 0;
        var keys = new List<string>(otherQueries.Keys);
        foreach (var key in keys)
        {
          var lst = otherQueries[key];
          if (!lst.Any(m => m.Biotype.Equals(category)))
            continue;

          count += lst[0].QueryCount;
          otherQueries.Remove(key);
        }

        counts.Add(new CategoryCount(category, string.Empty, count));
      }
    }
  }
}
