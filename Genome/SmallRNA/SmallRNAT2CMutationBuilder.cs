using CQS.Genome.Feature;
using CQS.Genome.Mapping;
using CQS.Genome.Statistics;
using RCPA.Gui;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CQS.Genome.SmallRNA
{
  public class SmallRNAT2CMutationBuilder : ProgressClass
  {
    private double t2cRate;

    public SmallRNAT2CMutationBuilder(double t2cRate)
    {
      this.t2cRate = t2cRate;
    }

    public List<FeatureItemGroup> Build(string countXmlFile)
    {
      var result = new FeatureItemGroupXmlFormat().ReadFromFile(countXmlFile);

      Progress.SetMessage("There are {0} groups in {1}", result.Count, countXmlFile);

      result.ForEach(g => g.ForEach(smallRNA => smallRNA.Locations.ForEach(region => region.QueryCountBeforeFilter = region.QueryCount)));

      //no number of no penalty mutation defined, check the T2C
      if (result.All(m => m.All(l => l.Locations.All(k => k.SamLocations.All(s => s.NumberOfNoPenaltyMutation == 0)))))
      {
        foreach (var group in result)
        {
          foreach (var smallRNA in group)
          {
            smallRNA.Locations.RemoveAll(m => m.SamLocations.Count == 0);
            foreach (var region in smallRNA.Locations)
            {
              region.SamLocations.ForEach(q =>
              {
                var snp = q.SamLocation.GetNotGsnapMismatch(q.SamLocation.Parent.Sequence);
                if (null != snp && snp.IsMutation('T', 'C'))
                {
                  q.NumberOfMismatch = q.SamLocation.NumberOfMismatch - 1;
                  q.NumberOfNoPenaltyMutation = 1;
                }
                else
                {
                  q.NumberOfMismatch = q.SamLocation.NumberOfMismatch;
                  q.NumberOfNoPenaltyMutation = 0;
                }
              });
            }
          }
        }
      }

      result.RemoveAll(m =>
      {
        m.RemoveAll(l =>
        {
          l.Locations.RemoveAll(k =>
          {
            k.SamLocations.RemoveAll(s => s.NumberOfNoPenaltyMutation == 0);
            return k.SamLocations.Count == 0;
          });

          return l.Locations.Count == 0;
        });

        return m.Count == 0;
      });

      Progress.SetMessage("There are {0} groups having T2C mutation", result.Count);

      foreach (var group in result)
      {
        foreach (var smallRNA in group)
        {
          foreach (var region in smallRNA.Locations)
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
