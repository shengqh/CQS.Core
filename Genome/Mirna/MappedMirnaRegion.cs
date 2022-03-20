﻿using CQS.Genome.Gtf;
using System.Collections.Generic;
using System.Linq;

namespace CQS.Genome.Mirna
{
  /// <summary>
  /// One region of the feature and the corresponding mapped reads.
  /// </summary>
  public class MappedMirnaRegion
  {
    public MappedMirnaRegion()
    {
      this.Region = new GtfItem();
      this.Mapped = new Dictionary<int, SequenceRegionMapped>();
    }

    /// <summary>
    /// The region of feature
    /// </summary>
    public ISequenceRegion Region { get; set; }

    /// <summary>
    /// The offset/region map
    /// </summary>
    public Dictionary<int, SequenceRegionMapped> Mapped { get; set; }

    public double GetEstimatedCount()
    {
      return this.Mapped.Sum(n => n.Value.AlignedLocations.Sum(l => l.Parent.GetEstimatedCount()));
    }
  }
}
