using Bio.IO.SAM;
using Bio.Util;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;

namespace CQS.Genome.Sam
{
  /// <summary>
  ///   A BAMParser reads from a source of binary data that is formatted according to the BAM
  ///   file specification, and converts the data to in-memory SequenceAlignmentMap object.
  ///   Documentation for the latest BAM file format can be found at
  ///   http://samtools.sourceforge.net/SAM1.pdf
  ///   The file was modified version of BAMParser in Bio.NET
  /// </summary>
  public class SAMAlignedItemBAMParser : AbstractBAMParser<SAMAlignedItem>, ISAMAlignedItemParser
  {
    /// <summary>
    ///   The default constructor which chooses the default encoding based on the alphabet.
    /// </summary>
    public SAMAlignedItemBAMParser(ISAMFormat samformat, string bamFileName)
      : base(bamFileName)
    {
      Format = samformat;
    }

    /// <summary>
    ///   The default constructor which chooses the default encoding based on the alphabet.
    /// </summary>
    public SAMAlignedItemBAMParser(ISAMFormat samformat, string bamFileName, string refSeqName)
      : base(bamFileName, refSeqName)
    {
      Format = samformat;
    }

    public ISAMFormat Format { get; private set; }

    /// <summary>
    ///   Returns an aligned sequence by parses the BAM file.
    /// </summary>
    [SuppressMessage("Microsoft.Usage", "CA2208:InstantiateArgumentExceptionsCorrectly")]
    protected override SAMAlignedItem GetAlignedSequence()
    {
      var array = new byte[4];

      ReadUnCompressedData(array, 0, 4);
      int blockLen = Helper.GetInt32(array, 0);
      var alignmentBlock = new byte[blockLen];
      ReadUnCompressedData(alignmentBlock, 0, blockLen);

      if (!Filter.Accept(alignmentBlock))
      {
        return null;
      }

      var result = new SAMAlignedItem();
      var loc = new SAMAlignedLocation(result);

      int value;
      // 0-4 bytes
      var refSeqIndex = Helper.GetInt32(alignmentBlock, 0);

      loc.Seqname = refSeqIndex == -1 ? "*" : RefSeqNames[refSeqIndex];

      // 4-8 bytes
      loc.Start = Helper.GetInt32(alignmentBlock, 4) + 1;

      // 8 - 12 bytes "bin<<16|mapQual<<8|read_name_len"
      var unsignedValue = Helper.GetUInt32(alignmentBlock, 8);

      // 10 -12 bytes
      //alignedSeq.Bin = (int)(UnsignedValue & 0xFFFF0000) >> 16;
      // 9th bytes
      loc.MapQ = (int)(unsignedValue & 0x0000FF00) >> 8;

      // 8th bytes
      var queryNameLen = (int)(unsignedValue & 0x000000FF);

      // 12 - 16 bytes
      unsignedValue = Helper.GetUInt32(alignmentBlock, 12);
      // 14-16 bytes
      var flagValue = (int)(unsignedValue & 0xFFFF0000) >> 16;
      loc.Flag = (SAMFlags)flagValue;

      // 12-14 bytes
      var cigarLen = (int)(unsignedValue & 0x0000FFFF);

      // 16-20 bytes
      var readLen = Helper.GetInt32(alignmentBlock, 16);

      // 32-(32+readLen) bytes
      result.Qname = Encoding.ASCII.GetString(alignmentBlock, 32, queryNameLen - 1);
      var strbuilder = new StringBuilder();
      var startIndex = 32 + queryNameLen;

      for (var i = startIndex; i < (startIndex + cigarLen * 4); i += 4)
      {
        // Get the CIGAR operation length stored in first 28 bits.
        var cigarValue = Helper.GetUInt32(alignmentBlock, i);
        strbuilder.Append(((cigarValue & 0xFFFFFFF0) >> 4).ToString(CultureInfo.InvariantCulture));

        // Get the CIGAR operation stored in last 4 bits.
        value = (int)cigarValue & 0x0000000F;
        // MIDNSHP=>0123456
        strbuilder.Append(GetCigarChar(value, result.Qname));
      }

      var cigar = strbuilder.ToString();
      loc.Cigar = string.IsNullOrWhiteSpace(cigar) ? "*" : cigar;

      startIndex += cigarLen * 4;
      var sequence = new StringBuilder();
      var index = startIndex;
      for (; index < (startIndex + (readLen + 1) / 2) - 1; index++)
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

      if (readLen % 2 == 0)
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
          qualValues.Append((char)(alignmentBlock[i] + 33));
        }
      }
      else
      {
        qualValues.Append(SAMParser.AsteriskAsByte);
      }

      loc.Sequence = sequence.ToString();
      loc.Qual = qualValues.ToString();
      loc.Strand = loc.Flag.HasFlag(SAMFlags.QueryOnReverseStrand) ? '-' : '+';

      if (!loc.Flag.HasFlag(SAMFlags.UnmappedQuery))
      {
        startIndex += readLen;
        if (alignmentBlock.Length > startIndex + 4 && alignmentBlock[startIndex] != 0x0 &&
            alignmentBlock[startIndex + 1] != 0x0)
        {
          var options = new List<string>();
          for (int i = 0; i < SAMFormat.OptionStartIndex; i++)
          {
            options.Add(string.Empty);
          }

          for (index = startIndex; index < alignmentBlock.Length;)
          {
            var tag = Encoding.ASCII.GetString(alignmentBlock, index, 2);
            index += 2;
            var vType = (char)alignmentBlock[index++];

            // SAM format supports [AifZH] for value type.
            // In BAM, an integer may be stored as a signed 8-bit integer (c), unsigned 8-bit integer (C), signed short (s), unsigned
            // short (S), signed 32-bit (i) or unsigned 32-bit integer (I), depending on the signed magnitude of the integer. However,
            // in SAM, all types of integers are presented as type ʻiʼ. 

            //NOTE: Code previously here checked for valid value and threw an exception here, but this exception/validation is checked for in this method below, as while as when the value is set.

            var tValue = GetOptionalValue(vType, alignmentBlock, ref index);

            // Convert to SAM format, where all integers are represented the same way
            if ("cCsSI".IndexOf(vType) >= 0)
            {
              vType = 'i';
            }
            options.Add(string.Format("{0}:{1}:{2}", tag, vType, tValue));
          }

          var optionarrays = options.ToArray();

          loc.AlignmentScore = Format.GetAlignmentScore(optionarrays);
          loc.NumberOfMismatch = Format.GetNumberOfMismatch(optionarrays);
          loc.MismatchPositions = Format.GetMismatchPositions(optionarrays);
        }
      }

      return result;
    }
  }
}