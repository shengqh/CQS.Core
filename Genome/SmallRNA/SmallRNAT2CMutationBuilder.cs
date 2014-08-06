using CQS.Genome.Mapping;
using CQS.Genome.Statistics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CQS.Genome.SmallRNA
{
  public class SmallRNAT2CMutationBuilder
  {
    private double t2cRate;

    public SmallRNAT2CMutationBuilder(double t2cRate)
    {
      this.t2cRate = t2cRate;
    }

    public List<MappedItemGroup> Build(string countXmlFile)
    {
      var result = new MappedItemGroupXmlFileFormat().ReadFromFile(countXmlFile);

      foreach (var group in result)
      {
        foreach (var smallRNA in group)
        {
          smallRNA.MappedRegions.RemoveAll(m => m.AlignedLocations.Count == 0);

          MappedItemUtils.FilterMappedRegion(smallRNA);

          foreach (var region in smallRNA.MappedRegions)
          {
            region.QueryCountBeforeFilter = region.QueryCount;
            region.AlignedLocations.RemoveAll(q =>
            {
              var snp = q.GetMutation(q.Parent.Sequence);
              if (null == snp)
              {
                return true;
              }

              return !snp.IsMutation('T', 'C');
            });
          }
          smallRNA.MappedRegions.RemoveAll(m => m.AlignedLocations.Count == 0);
        }

        group.RemoveAll(n => n.MappedRegions.Count == 0);
      }

      result.RemoveAll(m => m.Count == 0);

      foreach (var group in result)
      {
        foreach (var smallRNA in group)
        {
          foreach (var region in smallRNA.MappedRegions)
          {
            var fisher = new FisherExactTestResult();
            fisher.Sample1.Succeed = region.QueryCountBeforeFilter - region.QueryCount;
            fisher.Sample1.Failed = region.QueryCount;
            fisher.Sample2.Succeed = (int)(region.QueryCountBeforeFilter * (1 - t2cRate));
            fisher.Sample2.Failed = (int)(region.QueryCountBeforeFilter * t2cRate);

            if (fisher.Sample1.Failed < fisher.Sample2.Failed)
            {
              region.PValue = 1;
            }
            else
            {
              region.PValue = fisher.CalculateTwoTailPValue();
            }
          }
        }
      }

      return result;
    }
  }
}
