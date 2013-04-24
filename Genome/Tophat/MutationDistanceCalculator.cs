using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using CQS.Genome;

namespace CQS.Genome.Tophat
{
  public static class MutationDistanceCalculator
  {
    public static void Calculate(List<MutationItem> mutations)
    {
      mutations.ForEach(m => m.MutationDistance = int.MaxValue);

      for (int i = 0; i < mutations.Count; i++)
      {
        for (int j = i + 1; j < mutations.Count; j++)
        {
          if (mutations[i].Chr != mutations[j].Chr)
          {
            continue;
          }

          var dis = Math.Abs(mutations[i].Position - mutations[j].Position);
          if (dis < mutations[i].MutationDistance)
          {
            mutations[i].MutationDistance = dis;
            mutations[i].NearestMutationItem = mutations[j];
          }

          if (dis < mutations[j].MutationDistance)
          {
            mutations[j].MutationDistance = dis;
            mutations[j].NearestMutationItem = mutations[i];
          }
        }
      }
    }
  }
}
