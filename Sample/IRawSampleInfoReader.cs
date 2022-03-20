using System.Collections.Generic;

namespace CQS.Sample
{
  public interface IRawSampleInfoReader
  {
    string SupportFor { get; }

    bool IsReaderFor(string directory);

    /// <summary>
    /// Read information from file
    /// </summary>
    /// <param name="dir">Directory contains information</param>
    /// <returns>File => { Key => {Values}}</returns>
    Dictionary<string, Dictionary<string, List<string>>> ReadDescriptionFromDirectory(string dir);
  }
}
