using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using CQS.Genome;
using RCPA;

namespace CQS.Genome.Tophat
{
  public static class JunctionDistanceCalculator
  {
    public static void Calculate(List<MutationItem> mutations, string junctionFile)
    {
      var junctions = CollectionUtils.ToGroupDictionary(new TophatJunctionBedReader().ReadFromFile(junctionFile), m => m.Chr);

      foreach (var m in mutations)
      {
        if (junctions.ContainsKey(m.Chr))
        {
          var juncs = junctions[m.Chr];

          juncs.ForEach(n => n.DistanceJunction = Math.Min(Math.Abs(n.End1 - m.Position), Math.Abs(n.Start2 - m.Position)));
          juncs.ForEach(n => n.DistanceTerminal = Math.Min(Math.Abs(n.Start1 - m.Position), Math.Abs(n.End2 - m.Position)));

          var minDistance = juncs.Min(n => n.DistanceJunction);
          var minJunction = juncs.Find(n => n.DistanceJunction == minDistance);
          m.JunctionDistance = minDistance;
          m.JunctionItem = minJunction;

          minDistance = juncs.Min(n => n.DistanceTerminal);
          minJunction = juncs.Find(n => n.DistanceTerminal == minDistance);
          m.TerminalDistance = minDistance;
          m.TerminalItem = minJunction;
        }
      }
    }
  }
}
