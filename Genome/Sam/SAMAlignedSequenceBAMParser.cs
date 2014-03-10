using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Bio.Algorithms.Alignment;
using Bio.IO.SAM;
using Bio.Util;
using Bio.IO.BAM;
using Bio;

namespace CQS.Genome.Sam
{
  /// <summary>
  /// A BAMParser reads from a source of binary data that is formatted according to the BAM
  /// file specification, and converts the data to in-memory SequenceAlignmentMap object.
  /// Documentation for the latest BAM file format can be found at
  /// http://samtools.sourceforge.net/SAM1.pdf
  /// The file was modified version of BAMParser in Bio.NET
  /// </summary>
  public class SAMAlignedSequenceBAMParser : AbstractBAMParser<SAMAlignedSequence>, IDisposable
  {
    #region Constructors

    /// <summary>
    /// The default constructor which chooses the default encoding based on the alphabet.
    /// </summary>
    public SAMAlignedSequenceBAMParser(string bamFileName)
      : base(bamFileName)
    { }

    /// <summary>
    /// The default constructor which chooses the default encoding based on the alphabet.
    /// </summary>
    public SAMAlignedSequenceBAMParser(string bamFileName, string refSeqName)
      : base(bamFileName, refSeqName)
    { }

    #endregion

    /// <summary>
    /// Returns an aligned sequence by parses the BAM file.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2208:InstantiateArgumentExceptionsCorrectly")]
    protected override SAMAlignedSequence GetAlignedSequence()
    {
      byte[] array = new byte[4];

      ReadUnCompressedData(array, 0, 4);
      int blockLen = Helper.GetInt32(array, 0);
      byte[] alignmentBlock = new byte[blockLen];
      ReadUnCompressedData(alignmentBlock, 0, blockLen);

      if (!Filter.Accept(alignmentBlock))
      {
        return null;
      }

      SAMAlignedSequence alignedSeq = new SAMAlignedSequence();
      int value;
      UInt32 UnsignedValue;
      // 0-4 bytes
      int refSeqIndex = Helper.GetInt32(alignmentBlock, 0);

      if (refSeqIndex == -1)
        alignedSeq.SetPreValidatedRName("*");
      else
        alignedSeq.SetPreValidatedRName(RefSeqNames[refSeqIndex]);

      // 4-8 bytes
      alignedSeq.Pos = Helper.GetInt32(alignmentBlock, 4) + 1;

      // 8 - 12 bytes "bin<<16|mapQual<<8|read_name_len"
      UnsignedValue = Helper.GetUInt32(alignmentBlock, 8);

      // 10 -12 bytes
      //alignedSeq.Bin = (int)(UnsignedValue & 0xFFFF0000) >> 16;
      // 9th bytes
      alignedSeq.MapQ = (int)(UnsignedValue & 0x0000FF00) >> 8;

      // 8th bytes
      int queryNameLen = (int)(UnsignedValue & 0x000000FF);

      // 12 - 16 bytes
      UnsignedValue = Helper.GetUInt32(alignmentBlock, 12);
      // 14-16 bytes
      int flagValue = (int)(UnsignedValue & 0xFFFF0000) >> 16;
      alignedSeq.Flag = (SAMFlags)flagValue;

      // 12-14 bytes
      int cigarLen = (int)(UnsignedValue & 0x0000FFFF);

      // 16-20 bytes
      int readLen = Helper.GetInt32(alignmentBlock, 16);

      // 20-24 bytes
      int mateRefSeqIndex = Helper.GetInt32(alignmentBlock, 20);
      if (mateRefSeqIndex != -1)
      {
        alignedSeq.SetPreValidatedMRNM(RefSeqNames[mateRefSeqIndex]);
      }
      else
      {
        alignedSeq.SetPreValidatedMRNM("*");
      }

      // 24-28 bytes
      alignedSeq.MPos = Helper.GetInt32(alignmentBlock, 24) + 1;

      // 28-32 bytes
      alignedSeq.ISize = Helper.GetInt32(alignmentBlock, 28);

      // 32-(32+readLen) bytes
      alignedSeq.QName = System.Text.ASCIIEncoding.ASCII.GetString(alignmentBlock, 32, queryNameLen - 1);
      StringBuilder strbuilder = new StringBuilder();
      int startIndex = 32 + queryNameLen;

      for (int i = startIndex; i < (startIndex + cigarLen * 4); i += 4)
      {
        // Get the CIGAR operation length stored in first 28 bits.
        UInt32 cigarValue = Helper.GetUInt32(alignmentBlock, i);
        strbuilder.Append(((cigarValue & 0xFFFFFFF0) >> 4).ToString(CultureInfo.InvariantCulture));

        // Get the CIGAR operation stored in last 4 bits.
        value = (int)cigarValue & 0x0000000F;

        // MIDNSHP=>0123456
        switch (value)
        {
          case 0:
            strbuilder.Append("M");
            break;
          case 1:
            strbuilder.Append("I");
            break;
          case 2:
            strbuilder.Append("D");
            break;
          case 3:
            strbuilder.Append("N");
            break;
          case 4:
            strbuilder.Append("S");
            break;
          case 5:
            strbuilder.Append("H");
            break;
          case 6:
            strbuilder.Append("P");
            break;
          case 7:
            strbuilder.Append("=");
            break;
          case 8:
            strbuilder.Append("X");
            break;
          default:
            throw new Exception("Invalid CIGAR of query " + alignedSeq.QName);
        }
      }

      string cigar = strbuilder.ToString();
      if (string.IsNullOrWhiteSpace(cigar))
      {
        alignedSeq.SetPreValidatedCIGAR("*");
      }
      else
      {
        alignedSeq.SetPreValidatedCIGAR(cigar);
      }

      startIndex += cigarLen * 4;
      var sequence = new byte[readLen];
      int sequenceIndex = 0;
      int index = startIndex;
      for (; index < (startIndex + (readLen + 1) / 2) - 1; index++)
      {
        // Get first 4 bit value
        value = (alignmentBlock[index] & 0xF0) >> 4;
        sequence[sequenceIndex++] = GetSeqCharAsByte(value);
        // Get last 4 bit value
        value = alignmentBlock[index] & 0x0F;
        sequence[sequenceIndex++] = GetSeqCharAsByte(value);

      }

      value = (alignmentBlock[index] & 0xF0) >> 4;
      sequence[sequenceIndex++] = GetSeqCharAsByte(value);

      if (readLen % 2 == 0)
      {
        value = alignmentBlock[index] & 0x0F;
        sequence[sequenceIndex++] = GetSeqCharAsByte(value);
      }

      startIndex = index + 1;
      byte[] qualValues = new byte[readLen];

      if (alignmentBlock[startIndex] != 0xFF)
      {
        for (int i = startIndex; i < (startIndex + readLen); i++)
        {
          qualValues[i - startIndex] = (byte)(alignmentBlock[i] + 33);
        }
        //validate quality scores here
        byte badVal;
        bool ok = QualitativeSequence.ValidateQualScores(qualValues, SAMParser.QualityFormatType, out badVal);
        if (!ok)
        {
          string message = string.Format("Invalid encoded quality score found: {0}", (char)badVal);
          throw new ArgumentOutOfRangeException("encodedQualityScores", message);
        }
      }
      else
      {
        qualValues = new byte[] { SAMParser.AsteriskAsByte };
      }
      //Values have already been validated when first parsed at this point so no need to again
      SAMParser.ParseQualityNSequence(alignedSeq, Alphabet, sequence, qualValues, false);

      startIndex += readLen;
      if (alignmentBlock.Length > startIndex + 4 && alignmentBlock[startIndex] != 0x0 && alignmentBlock[startIndex + 1] != 0x0)
      {
        for (index = startIndex; index < alignmentBlock.Length; )
        {
          SAMOptionalField optionalField = new SAMOptionalField();
          optionalField.Tag = System.Text.ASCIIEncoding.ASCII.GetString(alignmentBlock, index, 2);
          index += 2;
          char vType = (char)alignmentBlock[index++];

          // SAM format supports [AifZH] for value type.
          // In BAM, an integer may be stored as a signed 8-bit integer (c), unsigned 8-bit integer (C), signed short (s), unsigned
          // short (S), signed 32-bit (i) or unsigned 32-bit integer (I), depending on the signed magnitude of the integer. However,
          // in SAM, all types of integers are presented as type ʻiʼ. 

          //NOTE: Code previously here checked for valid value and threw an exception here, but this exception/validation is checked for in this method below, as while as when the value is set.

          optionalField.Value = GetOptionalValue(vType, alignmentBlock, ref index).ToString();

          // Convert to SAM format, where all integers are represented the same way
          if ("cCsSI".IndexOf(vType) >= 0)
          {
            vType = 'i';
          }
          optionalField.VType = vType.ToString();

          alignedSeq.OptionalFields.Add(optionalField);
        }
      }

      return alignedSeq;
    }
  }
}
