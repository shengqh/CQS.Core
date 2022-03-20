using RCPA;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CQS.Genome.Mapping
{
  public class CountMappingSummaryBuilder : AbstractThreadProcessor
  {
    private CountMappingSummaryBuilderOptions options;

    public CountMappingSummaryBuilder(CountMappingSummaryBuilderOptions options)
    {
      this.options = options;
    }

    public override IEnumerable<string> Process()
    {
      var conf = CountMappingSummaryBuilderConfiguration.Read(options.InputFile);

      var data = (from a in conf.SampleFiles
                  select new
                  {
                    Sample = a.Item1,
                    Data = (from b in a.Item2
                            let map = File.ReadAllLines(b).Where(m => !m.StartsWith("#")).ToDictionary(m => m.StringBefore("\t"), m => m.StringAfter("\t"))
                            let totalreads = map.ContainsKey("TotalReads") ? map["TotalReads"] : map["Total reads"]
                            let mappedreads = map.ContainsKey("MappedReads") ? map["MappedReads"] : map["Mapped reads"]
                            let featurereads = map.ContainsKey("FeatureReads") ? map["FeatureReads"] : map["Feature reads"]
                            select new { TotalReads = totalreads, MappedReads = mappedreads, FeatureReads = featurereads }).ToList()
                  }).ToList();

      using (var sw = new StreamWriter(options.OutputFile))
      {
        sw.WriteLine("Sample\t" + (from s in conf.SearchTypes
                                   let ss = new[] { s + "_TotalReads", s + "_MappedReads", s + "_FeatureReads" }
                                   from sss in ss
                                   select sss).Merge("\t"));
        foreach (var d in data)
        {
          sw.Write(d.Sample);
          foreach (var dd in d.Data)
          {
            sw.Write("\t{0}\t{1}\t{2}", dd.TotalReads, dd.MappedReads, dd.FeatureReads);
          }
          sw.WriteLine();
        }
      }

      return new[] { options.OutputFile };
    }
  }
}
