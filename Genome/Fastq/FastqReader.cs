using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace CQS.Genome.Fastq
{
  /// <summary>
  /// A FastqParser reads from a source of text that is formatted according to the FASTQ 
  /// file specification and converts the data to in-memory FastqSequence objects.
  /// </summary>
  public sealed class FastqReader
  {
    #region Methods

    /// <summary>
    /// Gets the IEnumerable of QualitativeSequences from the steam being parsed.
    /// </summary>
    /// <param name="reader">Stream to be parsed.</param>
    /// <returns>Returns the QualitativeSequences.</returns>
    public IEnumerable<FastqSequence> Parse(TextReader reader)
    {
      FastqSequence result;
      while ((result = ParseOne(reader)) != null)
      {
        yield return result;
      }
    }

    /// <summary>
    /// Gets the IEnumerable of FastqSequence from the stream being parsed.
    /// </summary>
    /// <param name="sr">Stream to be parsed.</param>
    /// <returns>Returns a FastqSequence.</returns>
    public FastqSequence ParseOne(TextReader sr)
    {
      string line;
      while ((line = sr.ReadLine()) != null)
      {
        if (line.StartsWith("@"))
        {
          break;
        }
      }

      if (line == null)
      {
        return null;
      }

      var refer = line.Substring(1);
      var seq = sr.ReadLine();
      var strand = sr.ReadLine();
      var score = sr.ReadLine();
      if (score == null)
      {
        return null;
      }

      FastqSequence result = new FastqSequence(refer, seq);
      result.Strand = strand.StartsWith("-") ? '-' : '+';
      result.Score = score;

      return result;
    }

    #endregion
  }
}
