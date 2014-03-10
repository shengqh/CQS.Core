using System.Collections.Generic;

namespace CQS.Genome.Fastq
{
  public interface IFastqItemParser
  {
    FastqItem ParseNext();
  }

  public static class FastqItemParserExtension
  {
    public static List<FastqItem> ParseToEnd(this IFastqItemParser parser)
    {
      var result = new List<FastqItem>();
      FastqItem seq;
      while ((seq = parser.ParseNext()) != null)
      {
        result.Add(seq);
      }
      return result;
    }
  }
}