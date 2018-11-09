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
  public class CoverageSite
  {
    public int Coverage { get; set; }
    public HashSet<string> UniqueRead { get; set; }
    public CoverageSite(int coverage, string uniqueRead)
    {
      Coverage = coverage;
      UniqueRead = new HashSet<string>(new[] { uniqueRead });
    }

    public CoverageSite(int coverage, IList<string> uniqueRead)
    {
      Coverage = coverage;
      UniqueRead = new HashSet<string>(uniqueRead);
    }

    public CoverageSite(int coverage)
    {
      Coverage = coverage;
      UniqueRead = new HashSet<string>();
    }
  }

  public class CoverageRegion : SequenceRegion
  {
    public CoverageRegion()
    {
      this.Coverages = new List<CoverageSite>();
    }

    public string GeneSymbol { get; set; }

    public List<CoverageSite> Coverages { get; private set; }

    public string ReverseComplementedSequence { get; set; }
  }

  public class SeedItem : SequenceRegion
  {
    public double Coverage { get; set; }

    public string GeneSymbol { get; set; }

    public string FullSequence { get; set; }

    public CoverageRegion Source { get; set; }

    public int SourceOffset { get; set; }

    public int GetSeedUniqueReadCount(int finalSeedLength)
    {
      return (from c in this.Source.Coverages.Skip(this.SourceOffset).Take(finalSeedLength)
              from q in c.UniqueRead
              select q).Distinct().Count();
    }
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