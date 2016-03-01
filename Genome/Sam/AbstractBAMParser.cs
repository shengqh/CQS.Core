using Bio;
using Bio.IO.BAM;
using Bio.IO.SAM;
using Bio.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
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
  public abstract class AbstractBAMParser<T> : IDisposable
  {
    #region Private Fields

    //private BAMParser parser = new BAMParser();

    /// <summary>
    ///   Symbols supported by BAM.
    /// </summary>
    protected const string BAMAlphabet = "=ACMGRSVTWYHKDBN";

    protected const string BAMCigar = "MIDNSHP=X";

    protected static byte[] BAMAlphabetAsBytes = BAMAlphabet.Select(x => (byte)x).ToArray();

    protected string BAMFileName;

    /// <summary>
    ///   Last chunk used to read sequences
    /// </summary>
    protected Chunk LastChunk;

    /// <summary>
    ///   Remained chunks
    /// </summary>
    protected IList<Chunk> LastChunks;

    /// <summary>
    ///   Holds the BAM file stream.
    /// </summary>
    protected Stream ReadStream;

    /// <summary>
    ///   The reference sequence corresponding chunks
    /// </summary>
    protected IList<Chunk> RefSeqChunks;

    /// <summary>
    ///   Holds the acquired reference sequence name.
    /// </summary>
    protected string RefSeqName;

    /// <summary>
    ///   Holds the names of the reference sequence.
    /// </summary>
    protected RegexValidatedStringList RefSeqNames;

    /// <summary>
    ///   Holds the current position of the compressed BAM file stream.
    ///   Used while creating BAMIndex objects from a BAM file and while parsing a BAM file using a BAM index file.
    /// </summary>
    private long _currentCompressedBlockStartPos;

    /// <summary>
    ///   A temporary file stream to hold uncompressed blocks.
    /// </summary>
    private Stream _deCompressedStream;

    /// <summary>
    ///   Flag to indicate whether the BAM file is compressed or not.
    /// </summary>
    private bool _isCompressed;

    /// <summary>
    ///   Holds the length of the reference sequences.
    /// </summary>
    private List<int> _refSeqLengths;

    /// <summary>
    ///   The alphabet to use for sequences in parsed SequenceAlignmentMap objects.
    ///   Always returns singleton instance of SAMDnaAlphabet.
    /// </summary>
    protected IAlphabet Alphabet
    {
      get { return SAMDnaAlphabet.Instance; }
    }

    #endregion

    #region Public Fields

    /// <summary>
    ///   Holds the BAM header
    /// </summary>
    public SAMAlignmentHeader Header { get; private set; }

    public IBAMFilter Filter { get; set; }

    #endregion

    #region Constructors

    /// <summary>
    ///   The default constructor which chooses the default encoding based on the alphabet.
    /// </summary>
    protected AbstractBAMParser(string bamFileName)
    {
      BAMFileName = bamFileName;
      RefSeqNames = new RegexValidatedStringList(SAMAlignedSequenceHeader.RNameRegxExprPattern);
      _refSeqLengths = new List<int>();

      ReadStream = new FileStream(bamFileName, FileMode.Open, FileAccess.Read, FileShare.Read);
      ValidateReader();
      Header = GetHeader();

      RefSeqChunks = null;
      LastChunk = null;

      Filter = new BAMAccept();
    }

    /// <summary>
    ///   The default constructor which chooses the default encoding based on the alphabet.
    /// </summary>
    protected AbstractBAMParser(string bamFileName, string refSeqName)
      : this(bamFileName)
    {
      // verify whether there is any reads related to chromosome.
      var refSeqIndex = RefSeqNames.IndexOf(refSeqName);
      if (refSeqIndex < 0)
      {
        var message = string.Format("reference sequence not found : {0}", refSeqName);
        throw new ArgumentException(message, "refSeqName");
      }

      var bamIndexFileName = GetBAMIndexFileName(bamFileName);
      using (var bamIndexFile = new BAMIndexFile(bamIndexFileName, FileMode.Open, FileAccess.Read))
      {
        var bamIndexInfo = bamIndexFile.Read();
        if (bamIndexInfo.RefIndexes.Count < refSeqIndex)
        {
          throw new ArgumentOutOfRangeException("refSeqIndex");
        }

        var refIndex = bamIndexInfo.RefIndexes[refSeqIndex];
        RefSeqChunks = GetChunks(refIndex);
      }

      RefSeqName = refSeqName;

      ResetChunks();
    }

    private void ResetChunks()
    {
      LastChunks = new List<Chunk>(RefSeqChunks);
      LastChunk = null;
    }

    #endregion

    #region Private Static Methods

    /// <summary>
    ///   Returns a boolean value indicating whether a BAM file is compressed or uncompressed.
    /// </summary>
    /// <param name="array">Byte array containing first 4 bytes of a BAM file</param>
    /// <returns>Returns true if the specified byte array indicates that the BAM file is compressed else returns false.</returns>
    private static bool IsCompressedBAMFile(byte[] array)
    {
      return array[0] == 31 && array[1] == 139 && array[2] == 8;
    }

    /// <summary>
    ///   Returns a boolean value indicating whether a BAM file is valid uncompressed BAM file or not.
    /// </summary>
    /// <param name="array">Byte array containing first 4 bytes of a BAM file</param>
    /// <returns>Returns true if the specified byte array indicates a valid uncompressed BAM file else returns false.</returns>
    private static bool IsUnCompressedBAMFile(byte[] array)
    {
      return array[0] == 66 && array[1] == 65 && array[2] == 77 && array[3] == 1;
    }

    /// <summary>
    ///   Gets optional value depending on the valuetype.
    /// </summary>
    /// <param name="valueType">Value Type.</param>
    /// <param name="array">Byte array to read from.</param>
    /// <param name="startIndex">Start index of value in the array.</param>
    protected static object GetOptionalValue(char valueType, byte[] array, ref int startIndex)
    {
      object obj;
      int len;
      switch (valueType)
      {
        case 'A': //  Printable character
          obj = (char)array[startIndex];
          startIndex++;
          break;
        case 'c': //signed 8-bit integer
          var intValue = (array[startIndex] & 0x7F);
          if ((array[startIndex] & 0x80) == 0x80)
          {
            intValue = intValue + sbyte.MinValue;
          }

          obj = intValue;
          startIndex++;
          break;
        case 'C':
          obj = (uint)array[startIndex];
          startIndex++;
          break;
        case 's':
          obj = Helper.GetInt16(array, startIndex);
          startIndex += 2;
          break;
        case 'S':
          obj = Helper.GetUInt16(array, startIndex);
          startIndex += 2;
          break;
        case 'i':
          obj = Helper.GetInt32(array, startIndex);
          startIndex += 4;
          break;
        case 'I':
          obj = Helper.GetUInt32(array, startIndex);
          startIndex += 4;
          break;
        case 'f':
          obj = Helper.GetSingle(array, startIndex);
          startIndex += 4;
          break;
        case 'Z':
          len = GetStringLength(array, startIndex);
          obj = Encoding.ASCII.GetString(array, startIndex, len - 1);
          startIndex += len;
          break;
        case 'H':
          len = GetStringLength(array, startIndex);
          obj = Helper.GetHexString(array, startIndex, len - 1);
          startIndex += len;
          break;
        case 'B':
          var arrayType = (char)array[startIndex];
          startIndex++;
          var arrayLen = Helper.GetInt32(array, startIndex);
          startIndex += 4;
          var strBuilder = new StringBuilder();
          strBuilder.Append(arrayType);
          for (var i = 0; i < arrayLen; i++)
          {
            strBuilder.Append(',');
            var value = GetOptionalValue(arrayType, array, ref startIndex).ToString();
            strBuilder.Append(value);
          }

          obj = strBuilder.ToString();
          break;
        default:
          throw new Exception("Invalid BAM opt value type: " + valueType);
      }

      return obj;
    }

    /// <summary>
    ///   Gets the length of the string in byte array.
    /// </summary>
    /// <param name="array">Byte array which contains string.</param>
    /// <param name="startIndex">Start index of array from which string is stored.</param>
    private static int GetStringLength(byte[] array, int startIndex)
    {
      var i = startIndex;
      while (i < array.Length && array[i] != '\x0')
      {
        i++;
      }

      return i + 1 - startIndex;
    }

    /// <summary>
    ///   Gets equivalent sequence char for the specified encoded value.
    /// </summary>
    /// <param name="encodedValue">Encoded value.</param>
    protected static byte GetSeqCharAsByte(int encodedValue)
    {
      if (encodedValue >= 0 && encodedValue <= BAMAlphabetAsBytes.Length)
      {
        return BAMAlphabetAsBytes[encodedValue];
      }
      throw new Exception("Invalid BAM encoded sequence value!");
    }

    /// <summary>
    ///   Gets equivalent sequence char for the specified encoded value.
    /// </summary>
    /// <param name="encodedValue">Encoded value.</param>
    /// <param name="qname">Query name, for trace only.</param>
    protected static char GetSeqChar(int encodedValue, string qname)
    {
      if (encodedValue >= 0 && encodedValue <= BAMAlphabet.Length)
      {
        return BAMAlphabet[encodedValue];
      }
      throw new Exception(string.Format("Invalid BAM encoded sequence value: {0} in {1}!", encodedValue, qname));
    }

    /// <summary>
    ///   Gets equivalent cigar char for the specified encoded value.
    /// </summary>
    /// <param name="encodedValue">Encoded value.</param>
    protected static char GetCigarChar(int encodedValue, string qname)
    {
      if (encodedValue >= 0 && encodedValue <= BAMCigar.Length)
      {
        return BAMCigar[encodedValue];
      }
      throw new Exception(string.Format("Invalid Cigar encoded sequence value: {0} in {1}!", encodedValue, qname));
    }

    /// <summary>
    ///   Decompresses specified compressed stream to out stream.
    /// </summary>
    /// <param name="compressedStream">Compressed stream to decompress.</param>
    /// <param name="outStream">Out stream.</param>
    private static void Decompress(Stream compressedStream, Stream outStream)
    {
      using (GZipStream decompressionStream = new GZipStream(compressedStream, CompressionMode.Decompress))
      {
        decompressionStream.CopyTo(outStream);
      }
    }

    // Gets list of possible bins for a given start and end reference sequence co-ordinates.
    private static IList<uint> Reg2Bins(uint start, uint end)
    {
      var bins = new List<uint>();
      uint k;
      --end;
      bins.Add(0);
      for (k = 1 + (start >> 26); k <= 1 + (end >> 26); ++k) bins.Add(k);
      for (k = 9 + (start >> 23); k <= 9 + (end >> 23); ++k) bins.Add(k);
      for (k = 73 + (start >> 20); k <= 73 + (end >> 20); ++k) bins.Add(k);
      for (k = 585 + (start >> 17); k <= 585 + (end >> 17); ++k) bins.Add(k);
      for (k = 4681 + (start >> 14); k <= 4681 + (end >> 14); ++k) bins.Add(k);
      return bins;
    }

    // Gets all chunks for the specified ref sequence index.
    private static IList<Chunk> GetChunks(BAMReferenceIndexes refIndex)
    {
      var chunks = new List<Chunk>();
      foreach (var bin in refIndex.Bins)
      {
        chunks.InsertRange(chunks.Count, bin.Chunks);
      }

      return SortAndMergeChunks(chunks);
    }

    // Gets chunks for specified ref seq index, start and end co-ordinate this method considers linear index also.
    private static IList<Chunk> GetChunks(BAMReferenceIndexes refIndex, int start, int end)
    {
      var chunks = new List<Chunk>();
      var binnumbers = Reg2Bins((uint)start, (uint)end);
      var bins = refIndex.Bins.Where(B => binnumbers.Contains(B.BinNumber)).ToList();

      // consider linear indexing only for the bins less than 4681.
      foreach (var bin in bins.Where(B => B.BinNumber < 4681))
      {
        chunks.InsertRange(chunks.Count, bin.Chunks);
      }

      var index = start / (16 * 1024); // Linear indexing window size is 16K

      if (refIndex.LinearOffsets.Count > index)
      {
        var offset = refIndex.LinearOffsets[index];
        chunks =
          chunks.Where(
            C =>
              C.ChunkEnd.CompressedBlockOffset > offset.CompressedBlockOffset ||
              (C.ChunkEnd.CompressedBlockOffset == offset.CompressedBlockOffset &&
               C.ChunkEnd.UncompressedBlockOffset > offset.UncompressedBlockOffset)).ToList();
      }

      // add chunks for the bin numbers greater than 4681.
      foreach (var bin in bins.Where(B => B.BinNumber >= 4681))
      {
        chunks.InsertRange(chunks.Count, bin.Chunks);
      }

      return SortAndMergeChunks(chunks);
    }

    /// <summary>
    ///   Sorts and merges the overlapping chunks.
    /// </summary>
    /// <param name="chunks">Chunks to sort and merge.</param>
    private static List<Chunk> SortAndMergeChunks(IEnumerable<Chunk> chunks)
    {
      var sortedChunks = chunks.OrderBy(C => C, ChunkSorterForMerging.GetInstance()).ToList();

      for (int i = 0; i < sortedChunks.Count - 1; i++)
      {
        Chunk currentChunk = sortedChunks[i];
        Chunk nextChunk = sortedChunks[i + 1];

        if (nextChunk.ChunkStart.CompareTo(currentChunk.ChunkStart) >= 0 &&
            nextChunk.ChunkStart.CompareTo(currentChunk.ChunkEnd) <= 0)
        {
          // merge chunks.
          currentChunk.ChunkEnd = currentChunk.ChunkEnd.CompareTo(nextChunk.ChunkEnd) >= 0
            ? currentChunk.ChunkEnd
            : nextChunk.ChunkEnd;
          sortedChunks.RemoveAt(i + 1);
          i--;
        }
      }

      return sortedChunks;
    }

    #endregion

    #region Private Methods

    /// <summary>
    ///   Attempts to find the name of an index file for the given BAM file name, throws an error if none is found.
    /// </summary>
    /// <param name="bamFileToFindIndexOf">The name of the BAM file.</param>
    /// <returns>The name of the index file for the given BAM file.</returns>
    protected string GetBAMIndexFileName(string bamFileToFindIndexOf)
    {
      //Try Name+".bai"
      string possibleName = bamFileToFindIndexOf + ".bai";
      if (File.Exists(possibleName))
      {
        return possibleName;
      }
      //Try to remove .bam and replace it with .bai
      possibleName = bamFileToFindIndexOf.Replace(".bam", ".bai");
      if (File.Exists(possibleName))
      {
        return possibleName;
      }
      throw new FileNotFoundException("Could not find BAM Index file for: " + bamFileToFindIndexOf +
                                      " you may need to create an index file before parsing it.");
    }

    /// <summary>
    ///   Validates the BAM stream.
    /// </summary>
    private void ValidateReader()
    {
      _isCompressed = true;
      var array = new byte[4];

      if (ReadStream.Read(array, 0, 4) != 4)
      {
        // cannot read file properly.
        throw new Exception("Invalid bam file: " + BAMFileName);
      }

      _isCompressed = IsCompressedBAMFile(array);

      if (!_isCompressed)
      {
        if (!IsUnCompressedBAMFile(array))
        {
          // Neither compressed BAM file nor uncompressed BAM file header.
          throw new Exception("Invalid bam file: " + BAMFileName);
        }
      }

      ReadStream.Seek(0, SeekOrigin.Begin);
    }

    /// <summary>
    ///   Parses the BAM file and returns the Header.
    /// </summary>
    private SAMAlignmentHeader GetHeader()
    {
      var header = new SAMAlignmentHeader();
      RefSeqNames = new RegexValidatedStringList(SAMAlignedSequenceHeader.RNameRegxExprPattern);
      _refSeqLengths = new List<int>();

      ReadStream.Seek(0, SeekOrigin.Begin);
      _deCompressedStream = null;
      var array = new byte[8];
      ReadUnCompressedData(array, 0, 8);
      var lText = Helper.GetInt32(array, 4);
      var samHeaderData = new byte[lText];
      if (lText != 0)
      {
        ReadUnCompressedData(samHeaderData, 0, lText);
      }

      ReadUnCompressedData(array, 0, 4);
      var noofRefSeqs = Helper.GetInt32(array, 0);

      for (var i = 0; i < noofRefSeqs; i++)
      {
        ReadUnCompressedData(array, 0, 4);
        var len = Helper.GetInt32(array, 0);
        var refName = new byte[len];
        ReadUnCompressedData(refName, 0, len);
        ReadUnCompressedData(array, 0, 4);
        var refLen = Helper.GetInt32(array, 0);
        RefSeqNames.Add(Encoding.ASCII.GetString(refName, 0, refName.Length - 1));
        _refSeqLengths.Add(refLen);
      }

      if (samHeaderData.Length != 0)
      {
        var str = Encoding.ASCII.GetString(samHeaderData);
        using (var reader = new StringReader(str))
        {
          header = SAMParser.ParseSAMHeader(reader);
        }
      }

      header.ReferenceSequences.Clear();

      for (var i = 0; i < RefSeqNames.Count; i++)
      {
        var refname = RefSeqNames[i];
        var length = _refSeqLengths[i];
        header.ReferenceSequences.Add(new ReferenceSequenceInfo(refname, length));
      }

      return header;
    }

    /// <summary>
    ///   Reads specified number of uncompressed bytes from BAM file to byte array
    /// </summary>
    /// <param name="array">Byte array to copy.</param>
    /// <param name="offset">Offset of Byte array from which the data has to be copied.</param>
    /// <param name="count">Number of bytes to copy.</param>
    protected void ReadUnCompressedData(byte[] array, int offset, int count)
    {
      if (!_isCompressed)
      {
        ReadStream.Read(array, offset, count);
        return;
      }

      if (_deCompressedStream == null || _deCompressedStream.Length - _deCompressedStream.Position == 0)
      {
        GetNextBlock();
      }

      var remainingBlockSize = _deCompressedStream.Length - _deCompressedStream.Position;
      if (remainingBlockSize == 0)
      {
        return;
      }

      var bytesToRead = remainingBlockSize >= (long)count ? count : (int)remainingBlockSize;
      _deCompressedStream.Read(array, offset, bytesToRead);

      if (bytesToRead < count)
      {
        GetNextBlock();
        ReadUnCompressedData(array, bytesToRead, count - bytesToRead);
      }
    }

    /// <summary>
    ///   Gets next block by reading and decompressing the compressed block from compressed BAM file.
    /// </summary>
    private void GetNextBlock()
    {
      var bsize = 0;
      var arrays = new byte[18];
      _deCompressedStream = null;
      if (ReadStream.Length <= ReadStream.Position)
      {
        return;
      }

      _currentCompressedBlockStartPos = ReadStream.Position;

      ReadStream.Read(arrays, 0, 18);
      var elen = Helper.GetUInt16(arrays, 10);

      if (elen != 0)
      {
        bsize = Helper.GetUInt16(arrays, 12 + elen - 2);
      }

      var size = bsize + 1;

      var block = new byte[size];
      using (var memStream = new MemoryStream(size))
      {
        arrays.CopyTo(block, 0);

        if (ReadStream.Read(block, 18, size - 18) != size - 18)
        {
          throw new Exception("BAM: Unable to read compressed block");
        }

        var unCompressedBlockSize = Helper.GetUInt32(block, size - 4);

        _deCompressedStream = GetTempStream(unCompressedBlockSize);

        memStream.Write(block, 0, size);
        memStream.Seek(0, SeekOrigin.Begin);
        Decompress(memStream, _deCompressedStream);
      }

      _deCompressedStream.Seek(0, SeekOrigin.Begin);
    }

    /// <summary>
    ///   Gets the temp stream to store Decompressed blocks.
    ///   If the specified capacity is with in the Maximum integer (32 bit int) limit then
    ///   a MemoryStream is created else a temp file is created to store Decompressed data.
    /// </summary>
    /// <param name="capacity">Required capacity.</param>
    private Stream GetTempStream(uint capacity)
    {
      if (_deCompressedStream != null)
      {
        _deCompressedStream.Close();
        _deCompressedStream = null;
      }

      if (capacity <= int.MaxValue)
      {
        _deCompressedStream = new MemoryStream((int)capacity);
      }
      else
      {
        var fileName = Path.GetTempFileName();
        _deCompressedStream = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.ReadWrite);
      }

      return _deCompressedStream;
    }

    #endregion

    #region Protected Methods

    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
      if (!m_disposed)
      {
        if (disposing)
        {
          // Release managed resources
        }

        if (ReadStream != null)
        {
          ReadStream.Dispose();
          ReadStream = null;
        }

        if (_deCompressedStream != null)
        {
          _deCompressedStream.Dispose();
          _deCompressedStream = null;
        }

        m_disposed = true;
      }
    }

    ~AbstractBAMParser()
    {
      Dispose(false);
    }

    private bool m_disposed;

    #endregion

    #region Public Methods

    /// <summary>
    ///   Returns a boolean indicating whether the reader is reached end of file or not.
    /// </summary>
    /// <returns>Returns true if the end of the file has been reached.</returns>
    public bool IsEOF()
    {
      // if the BAM file is uncompressed then check the EOF by verifying the BAM file EOF.
      if (!_isCompressed || _deCompressedStream == null)
      {
        return ReadStream.Length <= ReadStream.Position;
      }

      // if the BAM file is compressed then verify uncompressed block.
      if (_deCompressedStream.Length <= _deCompressedStream.Position)
      {
        GetNextBlock();
      }

      return _deCompressedStream == null || _deCompressedStream.Length == 0;
    }

    public void Reset()
    {
      if (RefSeqChunks != null)
      {
        ResetChunks();
      }
      else
      {
        GetHeader();
      }
    }

    // Gets aligned sequence from the specified chunks of the BAM file which overlaps with the specified start and end co-ordinates.
    public T ParseNext()
    {
      if (LastChunks == null)
      {
        while (!IsEOF())
        {
          var alignedSeq = GetAlignedSequence();
          if (alignedSeq != null)
          {
            //get valid alignedSeq;
            return alignedSeq;
          }
        }

        return default(T);
      }

      while (LastChunks.Count > 0 || LastChunk != null)
      {
        if (LastChunk == null)
        {
          LastChunk = LastChunks[0];
          LastChunks.RemoveAt(0);
          ReadStream.Seek((long)LastChunk.ChunkStart.CompressedBlockOffset, SeekOrigin.Begin);
          GetNextBlock();
          if (_deCompressedStream == null)
          {
            //go to next chunk
            LastChunk = null;
          }
          else
          {
            _deCompressedStream.Seek(LastChunk.ChunkStart.UncompressedBlockOffset, SeekOrigin.Begin);
          }
          continue;
        }

        // read until eof or end of the chunck is reached.
        while (!IsEOF() &&
               (_currentCompressedBlockStartPos < (long)LastChunk.ChunkEnd.CompressedBlockOffset ||
                _deCompressedStream.Position < LastChunk.ChunkEnd.UncompressedBlockOffset))
        {
          var alignedSeq = GetAlignedSequence();
          if (alignedSeq != null)
          {
            //get valid alignedSeq;
            return alignedSeq;
          }
        }

        LastChunk = null;
      }

      return default(T);
    }

    protected abstract T GetAlignedSequence();

    public List<T> ParseToEnd()
    {
      var result = new List<T>();
      T seq;
      while ((seq = ParseNext()) != null)
      {
        result.Add(seq);
      }
      return result;
    }

    #endregion
  }
}