using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RCPA;
using CQS.Genome.Mirna;
using CQS.Genome.Mapping;
using System.IO;
using RCPA.Utils;
using CQS.Genome.Feature;

namespace CQS.Genome.SmallRNA
{
  public class SmallRNACategoryGroupPlusBuilder : AbstractThreadProcessor
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

    public SmallRNACategoryGroupPlusBuilder(SmallRNACategoryGroupBuilderOptions options)
    {
      this.options = options;
    }

    private static readonly string TotalReadsKey = "Total Reads";
    private static readonly string MappedReadsKey = "Mapped Reads";
    private static readonly string smallRNAKey = "small RNA";
    private static readonly string UnmappedKey = "Unmapped";
    private static readonly string OtherMappedKey = "Other Mapped";

    public override IEnumerable<string> Process()
    {
      var entries = (from line in File.ReadAllLines(options.InputFile)
                     let parts = line.Split('\t')
                     where parts.Length >= 3
                     select new { GroupName = parts[0], SampleName = parts[1], SmallRNAFile = parts[2] }).ToList();

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
            Progress.SetMessage("Reading smallRNA mapped info file " + entry.SmallRNAFile + " ...");

            var map = new MapItemReader(0, 1, hasHeader: false).ReadFromFile(entry.SmallRNAFile);

            var totalReads = Math.Round(double.Parse(map["TotalReads"].Value));
            var mappedReads = Math.Round(double.Parse(map["MappedReads"].Value));
            var smallRNAReads = Math.Round(double.Parse(map["FeatureReads"].Value));

            sw.WriteLine("{0}\t{1}\t0\t{2}", entry.SampleName, TotalReadsKey, totalReads);
            sw.WriteLine("{0}\t{1}\t0\t{2}", entry.SampleName, MappedReadsKey, mappedReads);
            sw.WriteLine("{0}\t{1}\t0\t{2}", entry.SampleName, smallRNAKey, smallRNAReads);

            sw.WriteLine("{0}\t{1}\t1\t{2}", entry.SampleName, UnmappedKey, totalReads - mappedReads);
            sw.WriteLine("{0}\t{1}\t1\t{2}", entry.SampleName, OtherMappedKey, mappedReads - smallRNAReads);
            sw.WriteLine("{0}\t{1}\t1\t{2}", entry.SampleName, smallRNAKey, smallRNAReads);

            foreach (var biotype in SmallRNAConsts.Biotypes)
            {
              if (map.ContainsKey(biotype))
              {
                sw.WriteLine("{0}\t{1}\t{2}\t{3}", entry.SampleName, biotype, 2, Math.Round(double.Parse(map[biotype].Value)));
              }
            }
          }
        }

        var data = (from line in File.ReadAllLines(catfile).Skip(1)
                    where !string.IsNullOrWhiteSpace(line)
                    let parts = line.Split('\t')
                    let level = double.Parse(parts[2])
                    where !(parts[1].Equals(smallRNAKey) && level == 1)
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

          var categories = new string[] { TotalReadsKey, MappedReadsKey, UnmappedKey, OtherMappedKey, smallRNAKey }.Union(SmallRNAConsts.Biotypes).ToList();

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
  }
}
