using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using CQS.Genome.Feature;
using CQS.Genome.Sam;
using RCPA;

namespace CQS.Genome.SmallRNA
{
  public class SmallRNAT2CMutationSummaryBuilder : AbstractThreadProcessor
  {
    private SmallRNAT2CMutationSummaryBuilderOptions options;

    public SmallRNAT2CMutationSummaryBuilder(SmallRNAT2CMutationSummaryBuilderOptions options)
    {
      this.options = options;
    }

    public override IEnumerable<string> Process()
    {
      using (var sw = new StreamWriter(options.OutputFile))
      {
        sw.WriteLine("File\tCategory\tName\tUniqueRead\tUniqueT2CRead\tUniqueT2CRate\tAvergeT2CIn10BasesOfUniqueRead\tAvergeT2COfUniqueRead\tTotalRead\tTotalT2CRead\tTotalT2CRate\tAverageT2CIn10BasesOfTotalRead\tAverageT2COfTotalRead");
        var inputFiles = options.GetCountXmlFiles();
        foreach (var file in inputFiles)
        {
          var subjects = new FeatureItemGroupXmlFormat().ReadFromFile(file.File);
          var group = subjects.GroupBy(m => m[0].Name.StringBefore(":")).ToList();
          foreach (var g in group)
          {
            var items = g.ToList();
            foreach (var item in items)
            {
              var queries = new HashSet<string>(item.GetAlignedLocations().ConvertAll(l => l.Parent.Qname));
              List<FeatureSamLocation> locs = new List<FeatureSamLocation>();
              foreach (var l in item)
              {
                foreach (var loc in l.Locations)
                {
                  foreach (var sl in loc.SamLocations)
                  {
                    if (queries.Contains(sl.SamLocation.Parent.Qname))
                    {
                      locs.Add(sl);
                      queries.Remove(sl.SamLocation.Parent.Qname);
                    }
                  }
                }
              }

              var t2c = locs.Where(m => m.NumberOfNoPenaltyMutation > 0).ToList();
              var ave_t2c_uniquereads = (t2c.Count > 0) ? t2c.ConvertAll(m => m.NumberOfNoPenaltyMutation * 10.0 / m.SamLocation.Parent.Sequence.Length).Average() : 0.0;
              var ave_t2c_perread_uniquereads = (t2c.Count > 0) ? t2c.ConvertAll(m => m.NumberOfNoPenaltyMutation).Average() : 0.0;

              double ave_t2c_allreads = 0.0;
              double ave_t2c_perread_allreads = 0.0;
              if (t2c.Count > 0)
              {
                List<double> values = new List<double>();
                List<double> perread_values = new List<double>();
                foreach (var t2citem in t2c)
                {
                  var v = t2citem.NumberOfNoPenaltyMutation * 10.0 / t2citem.SamLocation.Parent.Sequence.Length;
                  for (int i = 0; i < t2citem.SamLocation.Parent.QueryCount; i++)
                  {
                    values.Add(v);
                    perread_values.Add(t2citem.NumberOfNoPenaltyMutation);
                  }
                }
                ave_t2c_allreads = values.Average();
                ave_t2c_perread_allreads = perread_values.Average();
              }

              sw.WriteLine("{0}\t{1}\t{2}\t{3:0.###}\t{4:0.###}\t{5:0.###}\t{6:0.###}\t{7:0.###}\t{8:0.###}\t{9:0.###}\t{10:0.###}\t{11:0.###}\t{12:0.###}",
                file.Name,
                g.Key,
                item.Name,
                locs.Count,
                t2c.Count,
                t2c.Count * 1.0 / locs.Count,
                ave_t2c_uniquereads,
                ave_t2c_perread_uniquereads,
                locs.Sum(l => l.SamLocation.Parent.QueryCount),
                t2c.Sum(l => l.SamLocation.Parent.QueryCount),
                t2c.Sum(l => l.SamLocation.Parent.QueryCount) * 1.0 / locs.Sum(l => l.SamLocation.Parent.QueryCount),
                ave_t2c_allreads,
                ave_t2c_perread_allreads);
            }
          }
        }
      }

      return new[] { Path.GetFullPath(options.OutputFile) };
    }
  }
}
