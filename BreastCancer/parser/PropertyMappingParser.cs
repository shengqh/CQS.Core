using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RCPA.Format;
using RCPA;
using CQS.Converter;
using System.IO;
using CQS.Ncbi.Geo;
using CQS.Sample;

namespace CQS.BreastCancer.parser
{
  public class PropertyMappingParser : IBreastCancerSampleInfoParser2
  {
    private Dictionary<string, IPropertyConverter<BreastCancerSampleItem>> converters = new Dictionary<string, IPropertyConverter<BreastCancerSampleItem>>();

    private Dictionary<IPropertyConverter<BreastCancerSampleItem>, string> defaultConverters = new Dictionary<IPropertyConverter<BreastCancerSampleItem>, string>();

    public PropertyMappingParser(TextFileDefinition maps)
    {
      InitializeByDefinition(maps);
    }

    public PropertyMappingParser(string mappingFile)
    {
      var maps = new TextFileDefinition();
      maps.ReadFromFile(mappingFile);

      InitializeByDefinition(maps);
    }

    private void InitializeByDefinition(TextFileDefinition maps)
    {
      Type t = typeof(BreastCancerSampleItem);
      var properties = t.GetProperties();

      foreach (var item in maps)
      {
        if (!String.IsNullOrEmpty(item.PropertyName))
        {
          converters[item.AnnotationName] = ConverterUtils.GetPropertConverters<BreastCancerSampleItem>(properties, item.PropertyName);
        }
      }

      foreach (var df in maps.DefaultValues)
      {
        var ct = ConverterUtils.GetPropertConverters<BreastCancerSampleItem>(properties, df.PropertyName);
        defaultConverters[ct] = df.Value;
      }
    }

    public void ParseDataset(string datasetDirectory, Dictionary<string, BreastCancerSampleItem> sampleMap)
    {
      var files = GeoUtils.GetGsmNames(datasetDirectory);

      var dirname = Path.GetFileName(datasetDirectory);

      var map = RawSampleInfoReaderFactory.GetReader(datasetDirectory).ReadDescriptionFromDirectory(datasetDirectory);

      foreach (var key in map.Keys)
      {
        if (files.Contains(key.ToLower()))
        {
          if (!sampleMap.ContainsKey(key))
          {
            sampleMap[key] = new BreastCancerSampleItem(dirname, key);
          }

          var sample = sampleMap[key];

          var qsMap = map[key];

          //parse information
          foreach (var question in qsMap.Keys)
          {
            if (converters.ContainsKey(question))
            {
              var converter = converters[question];
              var answer = qsMap[question].First();
              converter.SetProperty(sample, answer);
            }
          }

          //set defaultvalue
          foreach (var dfConverter in defaultConverters)
          {
            dfConverter.Key.SetProperty(sample, dfConverter.Value);
          }
        }
      }
    }
  }
}
