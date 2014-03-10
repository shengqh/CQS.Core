using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RCPA;
using Bio.Util;
using Bio.IO.SAM;

namespace CQS.Genome.Sam
{
  public interface IBAMFilter : IFilter<byte[]>
  {  }

  public class BAMAccept : IBAMFilter
  {
    public virtual bool Accept(byte[] alignmentBlock)
    {
      return true;
    }
  }

  public class BAMMappedFilter : IBAMFilter
  {
    public virtual bool Accept(byte[] alignmentBlock)
    {
      // 12-16 bytes
      var UnsignedValue = Helper.GetUInt32(alignmentBlock, 12);

      // 14-16 bytes
      int flagValue = (int)(UnsignedValue & 0xFFFF0000) >> 16;

      var flag = (SAMFlags)flagValue;
      return !flag.HasFlag(SAMFlags.UnmappedQuery);
    }
  }

  public class BAMMapQFilter : IBAMFilter
  {
    private int minMapQ;

    public BAMMapQFilter(int minMapQ)
    {
      this.minMapQ = minMapQ;
    }

    public virtual bool Accept(byte[] alignmentBlock)
    {
      // 8 - 12 bytes "bin<<16|mapQual<<8|read_name_len"
      var UnsignedValue = Helper.GetUInt32(alignmentBlock, 8);

      // 9th bytes
      var mapQ = (int)(UnsignedValue & 0x0000FF00) >> 8;

      //if mapQ is too lower no need to parse further
      return mapQ >= this.minMapQ;
    }
  }
}
