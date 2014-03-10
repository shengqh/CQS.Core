using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CQS.Genome.Bed;
using CQS.Genome.Tophat;
using RCPA;

namespace CQS.Genome.Annotation
{
  public class DeletionDistanceExporter : AbstractInsertionDeletionDistanceExporter
  {
    public DeletionDistanceExporter(string insdelBedFile)
      : base(insdelBedFile, "deletion")
    { }

    protected override long DoGetDistance(InsertionDeletionItem insDel, long position)
    {
      return Math.Min(Math.Abs(insDel.Start - position), Math.Abs(insDel.End - position));
    }

    protected override string DoGetPosition(InsertionDeletionItem minInsDel)
    {
      return string.Format("{0}-{1}", minInsDel.Start, minInsDel.End);
    }
  }
}
