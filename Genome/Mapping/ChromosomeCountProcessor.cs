using CQS.Genome.Sam;
using CQS.Genome.SmallRNA;
using RCPA;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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

      var builder = new ChromosomeCountSlimItemBuilder(options.PreferPrefix, options.CategoryMapFile);
      var chroms = builder.Build(options.InputFile);

      Progress.SetMessage("Assigning query count ...");
      var cm = options.GetCountMap();

      var queries = chroms.GetQueries();

      queries.ForEach(m =>
      {
        m.QueryCount = cm.GetCount(m.Qname);
      });

      Progress.SetMessage("Sorting items ...");
      chroms.Sort((m1, m2) => m2.Queries.Count.CompareTo(m1.Queries.Count));
      chroms.ForEach(m => m.Queries.Sort((m1, m2) => m1.Qname.CompareTo(m2.Qname)));

      Progress.SetMessage("Saving xml file ...");
      var mappedfile = options.OutputFile + ".mapped.xml";
      new ChromosomeCountSlimItemXmlFormat().WriteToFile(mappedfile, chroms);

      if (options.MergeChromosomesByReads)
      {
        chroms.MergeCalculateSortByEstimatedCount();
      }
      //new ChromosomeCountItemXmlFormat().WriteToFile(mappedfile + ".merged", chroms);

      Progress.SetMessage("Saving count file ...");
      using (var sw = new StreamWriter(options.OutputFile))
      {
        sw.WriteLine("Name\tUniqueQuery\tQueryCount");
        foreach (var chr in chroms)
        {
          sw.WriteLine("{0}\t{1:0.##}\t{2}", (from m in chr.Names orderby m select m).Merge(";"),
            chr.Queries.Count,
            chr.GetQueryCount());
        }
      }

      Progress.End();

      return result;
    }
  }
}