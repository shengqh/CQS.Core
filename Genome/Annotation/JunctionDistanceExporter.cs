using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CQS.Genome.Bed;
using CQS.Genome.Tophat;
using RCPA;

namespace CQS.Genome.Annotation
{
  public class JunctionDistanceExporter : IAnnotationTsvExporter
  {
    private Dictionary<string, List<JunctionItem>> maps;
    private string header = null;
    private string emptyStr = null;

    public JunctionDistanceExporter(string junctionBedFile)
    {
      this.maps = CollectionUtils.ToGroupDictionary(new TophatJunctionBedReader().ReadFromFile(junctionBedFile), m => m.Chr);
      this.header = (from h in new string[] { "junction", "junction_name", "junction_position", "terminal", "terminal_name", "terminal_position" }
                     let t = "distance_" + h
                     select t).Merge("\t");
      this.emptyStr = new String('\t', header.Count(m => m == '\t'));
    }

    public string GetHeader()
    {
      return this.header;
    }

    public string GetValue(string chrom, long start, long end)
    {
      if (!maps.ContainsKey(chrom))
      {
        return this.emptyStr;
      }

      var juncs = maps[chrom];

      juncs.ForEach(n => n.DistanceJunction = Math.Min(Math.Abs(n.End1 - start), Math.Abs(n.Start2 - start)));
      juncs.ForEach(n => n.DistanceTerminal = Math.Min(Math.Abs(n.Start1 - start), Math.Abs(n.End2 - start)));

      var minDistance = juncs.Min(n => n.DistanceJunction);
      var minJunction = juncs.Find(n => n.DistanceJunction == minDistance);

      var junctionDistance = minDistance;
      var junctionItem = minJunction;

      minDistance = juncs.Min(n => n.DistanceTerminal);
      minJunction = juncs.Find(n => n.DistanceTerminal == minDistance);
      var terminalDistance = minDistance;
      var terminalItem = minJunction;

      return string.Format("{0}\t{1}\t{2}-{3}:{4}-{5}\t{6}\t{7}\t{8}-{9}:{10}-{11}",
        junctionDistance, junctionItem.Name, junctionItem.Start1, junctionItem.End1, junctionItem.Start2, junctionItem.End2,
          terminalDistance, terminalItem.Name, terminalItem.Start1, terminalItem.End1, terminalItem.Start2, terminalItem.End2);
    }
  }
}
