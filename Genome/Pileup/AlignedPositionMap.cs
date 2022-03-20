using System;
using System.Collections.Generic;
using System.Linq;

namespace CQS.Genome.Pileup
{
  /// <summary>
  /// A class used to store the matched information for pileup position (from multiple reads).
  /// Key is the matched allele
  /// Value is the list of [score, distance]
  /// distance means the distance between the matched allele and the 5' terminal
  /// </summary>
  public class AlignedPositionMap : Dictionary<string, List<AlignedPosition>>
  {
    /// <summary>
    /// Reference chromosome
    /// </summary>
    public string Chromosome { get; set; }

    /// <summary>
    /// Reference position [1-based]
    /// </summary>
    public long Position { get; set; }

    /// <summary>
    /// Reference base
    /// </summary>
    public char ReferenceAllele { get; set; }

    public override string ToString()
    {
      return string.Format("{0}:{1}, {2}, {3}",
        this.Chromosome,
        this.Position,
        this.ReferenceAllele,
        (from r in this orderby r.Key select string.Format("{0}:{1}", r.Key, r.Value)).Merge("; "));
    }
  }
}
