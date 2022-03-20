using CQS.Genome.Bed;
using RCPA;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CQS.Genome.Tophat
{
  public static class InsertionDeletionDistanceCalculator
  {
    public static void Calculate(List<MutationItem> mutations, string insertionDeletionFile, Action<MutationItem, long> setDistanceValue, Action<MutationItem, InsertionDeletionItem> setItemValue)
    {
      var insdels = CollectionUtils.ToGroupDictionary(new BedItemFile<InsertionDeletionItem>().ReadFromFile(insertionDeletionFile), m => m.Seqname);

      foreach (var m in mutations)
      {
        var values = insdels[m.Chr];

        values.ForEach(n => n.Distance = Math.Abs(n.Start - m.Position));

        var minDistance = values.Min(n => n.Distance);
        var minInsDel = values.Find(n => n.Distance == minDistance);
        setDistanceValue(m, minDistance);
        setItemValue(m, minInsDel);
      }
    }

    public static void CalculateInsertion(List<MutationItem> mutations, string insertionDeletionFile)
    {
      Calculate(mutations, insertionDeletionFile, (m, v) => m.InsertionDistance = v, (m, n) => m.InsertionItem = n);
    }

    public static void CalculateDeletion(List<MutationItem> mutations, string insertionDeletionFile)
    {
      Calculate(mutations, insertionDeletionFile, (m, v) => m.DeletionDistance = v, (m, n) => m.DeletionItem = n);
    }
  }
}
