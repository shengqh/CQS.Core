using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CQS.Microarray
{
  public interface ISummaryFile
  {
    HashSet<string> ReadGenes(string fileName);
  }
}
