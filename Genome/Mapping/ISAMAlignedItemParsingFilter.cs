using Bio.IO.SAM;
using CQS.Genome.Sam;

namespace CQS.Genome.Mapping
{
  public interface ISAMAlignedItemParsingFilter
  {
    /// <summary>
    /// Filter read with query name, for example, with NTA_
    /// </summary>
    /// <param name="qname"></param>
    /// <returns></returns>
    bool AcceptQueryName(string qname);

    /// <summary>
    /// Filter read with Flags, for example:
    /// return !flag.HasFlag(SAMFlags.UnmappedQuery));
    /// </summary>
    /// <param name="flags"></param>
    /// <returns></returns>
    bool AcceptFlags(SAMFlags flags);

    /// <summary>
    /// Filter read with length
    /// </summary>
    /// <param name="length"></param>
    /// <returns></returns>
    bool AcceptLength(int length);

    /// <summary>
    /// Filter read with cigar, for example:
    /// return cigar.All(m => m != 'I' && m != 'D')
    /// </summary>
    /// <param name="cigar"></param>
    /// <returns></returns>
    bool AcceptCigar(string cigar);

    /// <summary>
    /// Filter read with mismatch
    /// </summary>
    /// <param name="numberOfMisMatch"></param>
    /// <returns></returns>
    bool AcceptMismatch(int numberOfMisMatch);

    /// <summary>
    /// Filter read with locus
    /// </summary>
    /// <param name="loc"></param>
    /// <returns></returns>
    bool AcceptLocus(SAMAlignedLocation loc);
  }

  /// <summary>
  /// Default filter, only mapped reads will passed the filters
  /// </summary>
  public class SAMAlignedItemParsingFilter : ISAMAlignedItemParsingFilter
  {
    public virtual bool AcceptQueryName(string qname)
    {
      return true;
    }

    public virtual bool AcceptFlags(SAMFlags flags)
    {
      return !flags.HasFlag(SAMFlags.UnmappedQuery);
    }

    public virtual bool AcceptLength(int length)
    {
      return true;
    }

    public virtual bool AcceptCigar(string cigar)
    {
      return true;
    }

    public virtual bool AcceptMismatch(int numberOfMisMatch)
    {
      return true;
    }

    public virtual bool AcceptLocus(SAMAlignedLocation loc)
    {
      return true;
    }
  }
}
