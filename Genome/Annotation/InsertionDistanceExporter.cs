using CQS.Genome.Tophat;
using System;

namespace CQS.Genome.Annotation
{
  public class InsertionDistanceExporter : AbstractInsertionDeletionDistanceExporter
  {
    public InsertionDistanceExporter(string insdelBedFile)
      : base(insdelBedFile, "insertion")
    { }

    protected override long DoGetDistance(InsertionDeletionItem insDel, long position)
    {
      return Math.Abs(insDel.Start - position);
    }

    protected override string DoGetPosition(InsertionDeletionItem minInsDel)
    {
      return minInsDel.Start.ToString();
    }
  }
}
