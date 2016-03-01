using CQS.Genome;
using CQS.Genome.Bed;
using CQS.Genome.Gtf;
using CQS.Genome.Mapping;
using CQS.Genome.Mirna;
using CQS.Genome.SmallRNA;
using CQS.Genome.Feature;
using RCPA;
using RCPA.Seq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace CQS.Genome.Parclip
{
  public class CoverageRegion : SequenceRegion
  {
    public CoverageRegion()
    {
      this.Coverages = new List<int>();
    }

    public string GeneSymbol { get; set; }

    public List<int> Coverages { get; private set; }

    public string ReverseComplementedSequence { get; set; }
  }

  public class SeedItem : SequenceRegion
  {
    public double Coverage { get; set; }

    public string GeneSymbol { get; set; }

    public string FullSequence { get; set; }

    public CoverageRegion Source { get; set; }

    public int SourceOffset { get; set; }
  }

  public abstract class AbstractTargetBuilder : AbstractThreadProcessor
  {
    private AbstractTargetBuilderOptions options;

    public AbstractTargetBuilder(AbstractTargetBuilderOptions options)
    {
      this.options = options;
    }

    protected int[] GetPossibleOffsets(string seedName)
    {
      return new[] { options.SeedOffset };
    }
  }
}