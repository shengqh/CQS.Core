using RCPA;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CQS.Genome.QC
{
  public class BamSummaryShortBuilder : AbstractThreadProcessor
  {
    private BamSummaryShortBuilderOptions options;

    public BamSummaryShortBuilder(BamSummaryShortBuilderOptions options)
    {
      this.options = options;
    }

    public override IEnumerable<string> Process()
    {
      var reader = new SamToolsStatItemReader();

      var items = (from file in options.GetStatisticFiles()
                   let filename = Path.GetFileNameWithoutExtension(file)
                   let shortname = filename.ToLower().EndsWith(".bam") ? Path.GetFileNameWithoutExtension(filename) : filename
                   select new
                   {
                     FileName = shortname,
                     Data = reader.ReadFromFile(file)
                   }).ToList();

      using (var sw = new StreamWriter(options.OutputFile))
      {
        sw.WriteLine("Sample\tTotalReads\tMappedReads\tMappedReadPercentage\tMappedPairs");
        foreach (var item in items)
        {
          sw.WriteLine("{0}\t{1}\t{2}\t{3:0.00}%\t{4}",
            item.FileName,
            item.Data.Total,
            item.Data.Mapped,
            item.Data.Mapped * 100.0 / item.Data.Total,
            item.Data.WithItselfAndMateMapped);
        }
      }

      return new string[] { options.OutputFile };
    }
  }
}
