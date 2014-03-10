using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RCPA;
using CQS.Genome.Mirna;
using CQS.Genome.Mapping;
using System.IO;
using RCPA.Utils;

namespace CQS.Genome.SmallRNA
{
  public class SmallRNACategoryBuilder : AbstractThreadProcessor
  {
    private SmallRNACategoryBuilderOptions options;

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

    public SmallRNACategoryBuilder(SmallRNACategoryBuilderOptions options)
    {
      this.options = options;
    }

    public override IEnumerable<string> Process()
    {
      Progress.SetMessage("Reading smallRNA mapped file " + options.OtherFile + " ...");
      var others = new MappedItemGroupXmlFileFormat().ReadFromFile(options.OtherFile);

      var other_queries = (from g in others
                           from m in g
                           from mr in m.MappedRegions
                           from loc in mr.AlignedLocations
                           select new QueryRecord(loc.Parent.Qname,
                             m.Name.StringBefore(":"),
                             m.Name.StringAfter(":").StringBefore(":"),
                             m.Name.StringAfter(":").StringAfter(":"),
                             loc.Parent.QueryCount)).ToGroupDictionary(m => m.Query);
      Progress.SetMessage("Reading other smallRNA mapped file finished, {0} queries mapped.", other_queries.Count);

      if (File.Exists(options.MiRNAFile))
      {
        Progress.SetMessage("Reading miRNA mapped file " + options.MiRNAFile + " ...");
        var mirnas = new MappedMirnaGroupXmlFileFormat().ReadFromFile(options.MiRNAFile);
        var mirna_queries = (from g in mirnas
                             from m in g
                             from mr in m.MappedRegions
                             from mapped in mr.Mapped.Values
                             from loc in mapped.AlignedLocations
                             select new QueryRecord(loc.Parent.Qname,
                               "miRNA",
                               "miRNA",
                               m.Name,
                               loc.Parent.QueryCount)).ToGroupDictionary(m => m.Query);
        Progress.SetMessage("Reading miRNA mapped file finished, {0} queries mapped.", mirna_queries.Count);

        foreach (var q in mirna_queries)
        {
          List<QueryRecord> rec;
          if (!other_queries.TryGetValue(q.Key, out rec))
          {
            rec = q.Value;
            other_queries[q.Key] = q.Value;
          }
          else
          {
            rec.AddRange(q.Value);
          }
        }
        Progress.SetMessage("Total {0} queries mapped.", other_queries.Count);
      }

      List<CategoryCount> counts = new List<CategoryCount>();
      FillCounts(counts, options.Categories, other_queries);

      var othercategories = (from v in other_queries.Values
                             from item in v
                             select item.Biotype).Distinct().OrderBy(m => m).ToList();

      FillCounts(counts, othercategories, other_queries);

      using (StreamWriter sw = new StreamWriter(options.OutputFile))
      {
        sw.WriteLine("Category\tLevel\tCount");
        //2570-KCV-01-19.bam.count.mapped.xml => 2570-KCV-01-19.bam.info
        var infofile = Path.Combine(Path.GetDirectoryName(options.OtherFile), Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(options.OtherFile))) + ".info");
        Console.WriteLine(infofile);
        if (File.Exists(infofile))
        {
          var lines = File.ReadAllLines(infofile);
          foreach (var line in lines)
          {
            if (line.StartsWith("TotalReads"))
            {
              sw.WriteLine("Total Reads\t0\t{0}", line.StringAfter("\t"));
            }
            else if (line.StartsWith("MappedReads"))
            {
              sw.WriteLine("Mapped Reads\t0\t{0}", line.StringAfter("\t"));
            }
          }
          sw.WriteLine("small RNA\t0\t{0}", counts.Sum(m => m.Count));
        }

        foreach (var rec in counts)
        {
          sw.WriteLine("{0}\t{1}\t{2}", rec.Biotype, 1, rec.Count);
        }
      }


      var rfile = new FileInfo(FileUtils.GetTemplateDir() + "/smallrna_category.r").FullName;
      if (File.Exists(rfile))
      {
        var graphfile = options.PdfGraph ? options.OutputFile + ".pdf" : options.OutputFile + ".png";
        var graphargs = options.PdfGraph ? "1" : "0";
        SystemUtils.Execute("R", "--vanilla --slave -f \"" + rfile + "\" --args \"" + options.OutputFile + "\" \"" + graphfile + "\" " + graphargs);
      }

      return new string[] { Path.GetFullPath(options.OutputFile) };
    }

    private static void FillCounts(List<CategoryCount> counts, IList<string> cats, Dictionary<string, List<QueryRecord>> other_queries)
    {
      foreach (var category in cats)
      {
        int count = 0;
        var keys = new List<string>(other_queries.Keys);
        foreach (var key in keys)
        {
          var lst = other_queries[key];
          if (lst.Any(m => m.Biotype.Equals(category)))
          {
            count += lst[0].QueryCount;
            other_queries.Remove(key);
          }
        }

        counts.Add(new CategoryCount(category, string.Empty, count));
      }
    }
  }
}
