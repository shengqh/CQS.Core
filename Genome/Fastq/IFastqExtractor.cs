using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CQS.Genome.Fastq
{
  public interface IFastqExtractor
  {
    int Extract(string sourceFile, string targetFile, IEnumerable<string> exceptQueryNames);
  }
}
