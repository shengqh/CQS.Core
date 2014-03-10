using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using Bio.IO.SAM;
using Bio.Util;
using CQS.Genome.Sam;
using RCPA.Seq;

namespace CQS.Genome.Fastq
{
  /// <summary>
  ///   A BAMParser reads from a source of binary data that is formatted according to the BAM
  ///   file specification, and converts the data to in-memory SequenceAlignmentMap object.
  ///   Documentation for the latest BAM file format can be found at
  ///   http://samtools.sourceforge.net/SAM1.pdf
  ///   The file was modified version of BAMParser in Bio.NET
  ///   This class is used for extract fastq information only.
  /// </summary>
  public class FastqItemBAMParser : AbstractBAMParser<FastqItem>, IFastqItemParser
  {
    public HashSet<string> IgnoreQuery { get; private set; }

    /// <summary>
    ///   The default constructor which chooses the default encoding based on the alphabet.
    /// </summary>
    public FastqItemBAMParser(string bamFileName)
      : base(bamFileName)
    {
      IgnoreQuery = new HashSet<string>();
    }

    /// <summary>
    ///   The default constructor which chooses the default encoding based on the alphabet.
    /// </summary>
    public FastqItemBAMParser(string bamFileName, string refSeqName)
      : base(bamFileName, refSeqName)
    {
      IgnoreQuery = new HashSet<string>();
    }

    /// <summary>
    ///   Returns an aligned sequence by parses the BAM file.
    /// </summary>
    [SuppressMessage("Microsoft.Usage", "CA2208:InstantiateArgumentExceptionsCorrectly")]
    protected override FastqItem GetAlignedSequence()
    {
      var array = new byte[4];

      ReadUnCompressedData(array, 0, 4);
      var blockLen = Helper.GetInt32(array, 0);
      var alignmentBlock = new byte[blockLen];
      ReadUnCompressedData(alignmentBlock, 0, blockLen);

      if (!Filter.Accept(alignmentBlock))
      {
        return null;
      }

      int value;
      // 8 - 12 bytes "bin<<16|mapQual<<8|read_name_len"
      var unsignedValue = Helper.GetUInt32(alignmentBlock, 8);

      // 8th bytes
      var queryNameLen = (int) (unsignedValue & 0x000000FF);

      // 32-(32+readLen) bytes
      var qname = Encoding.ASCII.GetString(alignmentBlock, 32, queryNameLen - 1);

      if (IgnoreQuery.Contains(qname))
      {
        return null;
      }

      var result = new FastqItem()
      {
        Qname = qname
      };

      // 12 - 16 bytes
      unsignedValue = Helper.GetUInt32(alignmentBlock, 12);
      // 14-16 bytes
      var flagValue = (int) (unsignedValue & 0xFFFF0000) >> 16;
      var flag = (SAMFlags) flagValue;

      // 12-14 bytes
      var cigarLen = (int) (unsignedValue & 0x0000FFFF);

      // 16-20 bytes
      var readLen = Helper.GetInt32(alignmentBlock, 16);

      // 32-(32+readLen) bytes
      var startIndex = 32 + queryNameLen + cigarLen*4;

      var sequence = new StringBuilder();
      var index = startIndex;
      for (; index < (startIndex + (readLen + 1)/2) - 1; index++)
      {
        // Get first 4 bit value
        value = (alignmentBlock[index] & 0xF0) >> 4;
        sequence.Append(GetSeqChar(value, result.Qname));
        // Get last 4 bit value
        value = alignmentBlock[index] & 0x0F;
        sequence.Append(GetSeqChar(value, result.Qname));
      }

      value = (alignmentBlock[index] & 0xF0) >> 4;
      sequence.Append(GetSeqChar(value, result.Qname));

      if (readLen%2 == 0)
      {
        value = alignmentBlock[index] & 0x0F;
        sequence.Append(GetSeqChar(value, result.Qname));
      }

      startIndex = index + 1;
      var qualValues = new StringBuilder();

      if (alignmentBlock[startIndex] != 0xFF)
      {
        for (var i = startIndex; i < (startIndex + readLen); i++)
        {
          qualValues.Append((char) (alignmentBlock[i] + 33));
        }
      }
      else
      {
        qualValues.Append(SAMParser.AsteriskAsByte);
      }

      if (flag.HasFlag(SAMFlags.QueryOnReverseStrand))
      {
        result.Sequence = SequenceUtils.GetReverseComplementedSequence(sequence.ToString());
        result.Qual = qualValues.ToString().Reverse();
      }
      else
      {
        result.Sequence = sequence.ToString();
        result.Qual = qualValues.ToString();
      }

      result.Flags = flag;

      return result;
    }

    public static bool IsPaired(string bamFile)
    {
      using (var sr = new FastqItemBAMParser(bamFile))
      {
        FastqItem item;
        var count = 0;
        while ((item = sr.ParseNext()) != null)
        {
          if (item.Flags.HasFlag(SAMFlags.PairedRead))
          {
            return true;
          }

          if (item.Qname.EndsWith("/1") || item.Qname.EndsWith("/2"))
          {
            return true;
          }

          count++;
          if (count > 100000)
          {
            break;
          }
        }
      }
      return false;
    }

    public static bool IsSortedByName(string bamFile)
    {
      using (var sr = new FastqItemBAMParser(bamFile))
      {
        var hd = sr.Header.RecordFields.FirstOrDefault(m => m.Typecode.Equals("HD"));
        if (hd != null)
        {
          var so = hd.Tags.FirstOrDefault(m => m.Tag.Equals("SO"));
          if (so != null)
          {
            return so.Value.Equals("queryname");
          }
        }
      }
      return false;
    }
  }
}