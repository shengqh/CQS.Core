using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CQS.Genome.Bed;
using CQS.Genome.Tophat;
using RCPA;

namespace CQS.Genome.Annotation
{
  public abstract class AbstractInsertionDeletionDistanceExporter : IAnnotationTsvExporter
  {
    private Dictionary<string, List<InsertionDeletionItem>> maps;
    private string header = null;
    private string emptyStr = null;

    public AbstractInsertionDeletionDistanceExporter(string insdelBedFile, string name)
    {
      this.maps = CollectionUtils.ToGroupDictionary(new BedItemFile<InsertionDeletionItem>().ReadFromFile(insdelBedFile), m => m.Seqname.StringAfter("chr"));
      this.header = string.Format("distance_{0}\tdistance_{0}_position", name);
      this.emptyStr = "\t";
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

      var values = maps[chrom];

      values.ForEach(n => n.Distance = DoGetDistance(n, start));

      var minDistance = values.Min(n => n.Distance);
      var minInsDel = values.Find(n => n.Distance == minDistance);

      return string.Format("{0},{1}", minDistance, DoGetPosition(minInsDel));
    }

    protected abstract long DoGetDistance(InsertionDeletionItem insDel, long position);

    protected abstract string DoGetPosition(InsertionDeletionItem minInsDel);
  }
}
