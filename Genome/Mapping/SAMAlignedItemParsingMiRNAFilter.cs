using CQS.Genome.Sam;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CQS.Genome.Mapping
{
  public class SAMAlignedItemParsingMiRNAFilter : SAMAlignedItemParsingFilter
  {
    private int minLength;

    private int maxLength;

    private int maxMismatch;

    public SAMAlignedItemParsingMiRNAFilter(int minLength, int maxLength, int maxMismatch)
    {
      this.minLength = minLength;
      this.maxLength = maxLength;
      this.maxMismatch = maxMismatch;
    }

    public override bool AcceptLength(int length)
    {
      return this.minLength <= length && length <= this.maxLength;
    }

    /// <summary>
    /// Not insertion and deletion
    /// </summary>
    /// <param name="cigar"></param>
    /// <returns></returns>
    public override bool AcceptCigar(string cigar)
    {
      return cigar.All(m => m != 'I' && m != 'D');
    }

    public override bool AcceptMismatch(int numberOfMisMatch)
    {
      return numberOfMisMatch <= this.maxMismatch;
    }
  }
}
