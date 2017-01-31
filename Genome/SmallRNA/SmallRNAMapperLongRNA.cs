using CQS.Genome.Feature;
using CQS.Genome.Sam;
using System;

namespace CQS.Genome.SmallRNA
{
  public class SmallRNAMapperLongRNA : SmallRNAMapper
  {
    public SmallRNAMapperLongRNA(string mapperName, ISmallRNACountProcessorOptions options, Func<FeatureLocation, bool> accept) : base(mapperName, options,  accept)
    { }

    public override AcceptResult AcceptLocationPair(FeatureLocation floc, SAMAlignedLocation sloc)
    {
      if (sloc.Parent.Sequence.Length < options.MinimumReadLengthForLongRNA)
      {
        return new AcceptResult()
        {
          Accepted = false
        };
      }

      if (sloc.NumberOfMismatch > options.MaximumMismatchForLongRNA)
      {
        return new AcceptResult()
        {
          Accepted = false
        };
      }

      return base.AcceptLocationPair(floc, sloc);
    }
  }
}
