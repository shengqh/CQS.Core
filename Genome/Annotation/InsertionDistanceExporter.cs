using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CQS.Genome.Bed;
using CQS.Genome.Tophat;
using RCPA;

namespace CQS.Genome.Annotation
{
  public class InsertionDistanceExporter : AbstractInsertionDeletionDistanceExporter
  {
    public InsertionDistanceExporter(string insdelBedFile)
      : base(insdelBedFile, "insertion")
    { }

    protected override long DoGetDistance(InsertionDeletionItem insDel, long position)
    {
      return Math.Abs(insDel.ChromStart - position);
    }

    protected override string DoGetPosition(InsertionDeletionItem minInsDel)
    {
      return minInsDel.ChromStart.ToString();
    }
  }
}
