using CQS.Genome.Feature;
using CQS.Genome.Parclip;
using RCPA;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CQS.Genome.SmallRNA
{
  public class SmallRNAT2CMutationSummaryBuilder : AbstractThreadProcessor
  {
    private SmallRNAT2CMutationSummaryBuilderOptions options;

    public SmallRNAT2CMutationSummaryBuilder(SmallRNAT2CMutationSummaryBuilderOptions options)
    {
      this.options = options;
    }

    class SampleCount
    {
      public string Name { get; set; }
      public int GoodReadCount { get; set; }
      public int GoodT2CReadCount { get; set; }
      public double GoodT2CRate { get { return GoodT2CReadCount * 1.0 / GoodReadCount; } }
      public int MiRNACount { get; set; }
      public int TRNACount { get; set; }
      public int OtherSmallRNACount { get; set; }
      public int SmallRNACount { get { return MiRNACount + TRNACount + OtherSmallRNACount; } }
    }

    public override IEnumerable<string> Process()
    {
      var sampleInfos = new List<SampleCount>();
      using (var sw = new StreamWriter(options.OutputFile))
      using (var swUnfiltered = new StreamWriter(Path.ChangeExtension(options.OutputFile, ".unfiltered.tsv")))
      {
        var header = "File\tCategory\tName\tUniqueRead\tUniqueT2CRead\tUniqueT2CRate\tAvergeT2CIn10BasesOfUniqueRead\tAvergeT2COfUniqueRead\tTotalRead\tTotalT2CRead\tTotalT2CRate\tT2C_pvalue\tAverageT2CIn10BasesOfTotalRead\tAverageT2COfTotalRead";
        swUnfiltered.WriteLine(header);
        sw.WriteLine(header);

        var inputFiles = options.GetCountXmlFiles();

        foreach (var file in inputFiles)
        {
          var sc = new SampleCount();
          sc.Name = file.Name;
          sampleInfos.Add(sc);

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

              var totalCount = locs.Sum(l => l.SamLocation.Parent.QueryCount);
              var totalT2CCount = t2c.Sum(l => l.SamLocation.Parent.QueryCount);
              var pvalue = SmallRNAT2CMutationBuilder.CalculateT2CPvalue(totalCount, totalT2CCount, options.ExpectRate);
              var t2crate = totalT2CCount == 0 ? 0 : totalT2CCount * 1.0 / totalCount;
              var value = string.Format("{0}\t{1}\t{2}\t{3:0.###}\t{4:0.###}\t{5:0.###}\t{6:0.###}\t{7:0.###}\t{8:0.###}\t{9:0.###}\t{10:0.###}\t{11:0.###E+0}\t{12:0.###}\t{13:0.###}",
                file.Name,
                g.Key,
                item.Name,
                locs.Count,
                t2c.Count,
                t2c.Count * 1.0 / locs.Count,
                ave_t2c_uniquereads,
                ave_t2c_perread_uniquereads,
                totalCount,
                totalT2CCount,
                t2crate,
                pvalue,
                ave_t2c_allreads,
                ave_t2c_perread_allreads);

              swUnfiltered.WriteLine(value);
              if (!ParclipSmallRNAT2CBuilder.Accept(pvalue, totalCount, totalT2CCount, options.Pvalue, options.MinimumCount, options.ExpectRate))
              {
                continue;
              }

              sw.WriteLine(value);

              sc.GoodReadCount += totalCount;
              sc.GoodT2CReadCount += totalT2CCount;
              if (g.Key.Equals(SmallRNAConsts.miRNA))
              {
                sc.MiRNACount++;
              }
              else if (g.Key.Equals(SmallRNAConsts.tRNA))
              {
                sc.TRNACount++;
              }
              else
              {
                sc.OtherSmallRNACount++;
              }
            }
          }
        }
      }
      using (var sw = new StreamWriter(options.OutputFile + ".summary"))
      {
        sw.WriteLine("File\tTotalRead\tT2CRead\tT2CRate\tSmallRNA\tMicroRNA\ttRNA\tOtherSmallRNA");
        foreach (var si in sampleInfos)
        {
          sw.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}\t{7}",
            si.Name,
            si.GoodReadCount,
            si.GoodT2CReadCount,
            si.GoodT2CRate,
            si.SmallRNACount,
            si.MiRNACount,
            si.TRNACount,
            si.OtherSmallRNACount);
        }
      }

      return new[] { Path.GetFullPath(options.OutputFile), Path.GetFullPath(options.OutputFile + ".summary") };
    }
  }
}
