using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CQS.Sample
{
  public interface IRawSampleInfoReader
  {
    string SupportFor { get; }

    bool IsReaderFor(string directory);

    Dictionary<string, Dictionary<string, List<string>>> ReadDescriptionFromDirectory(string dir);
  }
}
