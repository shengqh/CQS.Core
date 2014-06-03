using CQS.Ncbi.Geo;
using RCPA.Format;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CQS.Sample
{
  public class SampleItemParser
  {
    private TextFileDefinition maps;

    public SampleItemParser(TextFileDefinition maps)
    {
      this.maps = maps;
    }

    public SampleItemParser(string mappingFile)
    {
      this.maps = new TextFileDefinition();
      maps.ReadFromFile(mappingFile);
    }

    public Dictionary<string, SampleItem> ParseDataset(string datasetDirectory)
    {
      var result = new Dictionary<string, SampleItem>();

      var columns = (from m in maps
                     where !string.IsNullOrEmpty(m.PropertyName)
                     select m).ToDictionary(m => m.AnnotationName);
      var dvs = (from m in maps.DefaultValues
                 where !string.IsNullOrEmpty(m.Value)
                 select m).ToList();

      var files = GeoUtils.GetGsmNameFileMap(datasetDirectory);

      var datasetName = Path.GetFileName(datasetDirectory);

      var dataMap = RawSampleInfoReaderFactory.GetReader(datasetDirectory).ReadDescriptionFromDirectory(datasetDirectory);

      foreach (var sampleName in dataMap.Keys)
      {
        string filename;
        if (files.TryGetValue(sampleName.ToLower(), out filename))
        {
          var sample = new SampleItem()
          {
            Dataset = datasetName,
            Sample = sampleName,
            SampleFile = filename
          };

          result[sampleName] = sample;

          var qsMap = dataMap[sampleName];

          foreach (var column in columns)
          {
            List<string> values;
            if (qsMap.TryGetValue(column.Key, out values))
            {
              sample.Annotations[column.Value.PropertyName] = values.FirstOrDefault();
            }
          }

          foreach (var dv in dvs)
          {
            if (!sample.Annotations.ContainsKey(dv.PropertyName))
            {
              sample.Annotations[dv.PropertyName] = dv.Value;
            }
          }
        }
      }

      return result;
    }
  }
}
