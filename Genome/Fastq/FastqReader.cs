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
    public FastqReader()
    {
      this.AcceptName = m => true;
    }

    public Func<string, bool> AcceptName {get;set;}
    #region Methods

    /// <summary>
    /// Gets the IEnumerable of FastqSequence from the stream being parsed.
    /// </summary>
    /// <param name="sr">Stream to be parsed.</param>
    /// <returns>Returns a FastqSequence.</returns>
    public FastqSequence Parse(TextReader sr)
    {
      while (true)
      {
        string line;
        while ((line = sr.ReadLine()) != null)
        {
          if (!String.IsNullOrWhiteSpace(line))
          {
            break;
          }
        }

        if (line == null)
        {
          return null;
        }

        if (!line.StartsWith("@"))
        {
          throw new Exception("Unrecognized line, should start with @ for query name: " + line);
        }

        var refer = line.Substring(1);
        FastqSequence result = new FastqSequence(refer, null);

        if (!AcceptName(result.Name))
        {
          sr.ReadLine();
          sr.ReadLine();
          sr.ReadLine();
          continue;
        }

        result.SeqString = sr.ReadLine();
        result.Strand = sr.ReadLine().StartsWith("-") ? '-' : '+';
        result.Score = sr.ReadLine();
        if (result.Score == null)
        {
          throw new Exception("Unrecognized line, cannot find score line of query: " + refer);
        }

        return result;
      }
    }

    #endregion
  }
}
