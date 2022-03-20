using System.Collections.Generic;

namespace CQS.Genome.Fastq
{
  public interface IFastqExtractor
  {
    int Extract(string sourceFile, string targetFile, IEnumerable<string> exceptQueryNames, string countFile);
  }
}
