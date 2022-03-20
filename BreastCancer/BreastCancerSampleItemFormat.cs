using CQS.Converter;
using CQS.Sample;
using RCPA;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CQS.BreastCancer
{
  public class BreastCancerSampleItemFormat : IFileFormat<List<BreastCancerSampleItem>>
  {
    public static string DefaultHeader
    {
      get
      {
        var headers = ConverterUtils.GetItems<BreastCancerSampleItem, SampleInfoAttribute>();

        var propertyNames = (from h in headers
                             orderby h.PropertyName
                             select h.PropertyName).ToList();

        propertyNames.Insert(0, "Sample");
        propertyNames.Insert(0, "Dataset");

        return propertyNames.Merge("\t");
      }
    }

    public string HeaderStartKey { get; set; }

    private string header;
    public BreastCancerSampleItemFormat(string header = "")
    {
      if (string.IsNullOrEmpty(header))
      {
        this.header = DefaultHeader;
      }
      else
      {
        this.header = header;
      }

      this.HeaderStartKey = "";
    }

    private LineFormat<BreastCancerSampleItem> _format { get; set; }

    public LineFormat<BreastCancerSampleItem> Format
    {
      get
      {
        if (_format == null)
        {
          _format = new LineFormat<BreastCancerSampleItem>(new BreastCancerSampleItemPropertyFactory(), header);
        }
        return _format;
      }
      set
      {
        _format = value;
      }
    }

    private void InitializeLineFormat(string header)
    {
      _format = new LineFormat<BreastCancerSampleItem>(new BreastCancerSampleItemPropertyFactory(), header);
    }

    public List<BreastCancerSampleItem> ReadFromFile(string fileName)
    {
      var result = new List<BreastCancerSampleItem>();

      var dataset = Path.GetFileName(fileName);
      while (dataset.Contains('.'))
      {
        dataset = Path.GetFileNameWithoutExtension(dataset);
      }

      using (StreamReader sr = new StreamReader(fileName))
      {
        string line;

        while ((line = sr.ReadLine()) != null)
        {
          if (line.StartsWith(HeaderStartKey))
          {
            break;
          }
        }

        if (line == null)
        {
          throw new ArgumentException("Cannot find start key " + this.HeaderStartKey + " in file " + fileName);
        }

        InitializeLineFormat(line);

        while ((line = sr.ReadLine()) != null)
        {
          if (line.Trim() == string.Empty)
          {
            break;
          }

          var item = Format.ParseString(line);
          item.Dataset = dataset;
          result.Add(item);
        }
      }

      return result;
    }

    public void WriteToFile(string fileName, List<BreastCancerSampleItem> t)
    {
      using (StreamWriter sw = new StreamWriter(fileName))
      {
        sw.WriteLine(this.Format.GetHeader());
        foreach (var item in t)
        {
          sw.WriteLine(this.Format.GetString(item));
        }
      }
    }
  }
}
