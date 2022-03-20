using System.Collections.Generic;

namespace CQS.BreastCancer
{
  public interface IBreastCancerSampleInfoParser
  {
    List<BreastCancerSampleItem> ParseDataset(string datasetDirectory);
  }

  public interface IBreastCancerSampleInfoParser2
  {
    void ParseDataset(string datasetDirectory, Dictionary<string, BreastCancerSampleItem> sampleMap);
  }

  public class CompositeParser2 : List<IBreastCancerSampleInfoParser2>, IBreastCancerSampleInfoParser2
  {
    public void ParseDataset(string datasetDirectory, Dictionary<string, BreastCancerSampleItem> sampleMap)
    {
      foreach (var parser in this)
      {
        parser.ParseDataset(datasetDirectory, sampleMap);
      }
    }
  }
}
