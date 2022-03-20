using System.Collections.Generic;

namespace CQS.Microarray
{
  public interface ISummaryFile
  {
    HashSet<string> ReadGenes(string fileName);
  }
}
