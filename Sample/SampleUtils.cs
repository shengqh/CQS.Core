using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using RCPA;

namespace CQS.Sample
{
  public static class SampleUtils
  {
    public static List<string> ReadPropertiesFromFile(string propertyFile)
    {
      var result = (from l in File.ReadAllLines(propertyFile)
                    let lt = l.Trim()
                    where !string.IsNullOrEmpty(lt)
                    select lt).ToList();
      return result;
    }

    public static string[] GetDatasets(string root)
    {
      var subdirs = Directory.GetDirectories(root);
      Array.Sort(subdirs, delegate(string name1, string name2)
      {
        var n1 = new FileInfo(name1).Name;
        var n2 = new FileInfo(name2).Name;
        if (n1.StartsWith("GSE") && n2.StartsWith("GSE"))
        {
          return int.Parse(n1.Substring(3)).CompareTo(int.Parse(n2.Substring(3)));
        }
        else
        {
          return name1.CompareTo(name2);
        }
      });
      return subdirs;
    }

    public static IPropertyConverter<SampleItem>[] GetConverters(IEnumerable<string> properties)
    {
      var columns = GetColumns(properties);

      var factory = new SampleItemPropertyFactory<SampleItem>();

      return (from header in columns
              select factory.FindConverter(header)).ToArray();

    }

    public static List<string> GetColumns(IEnumerable<string> properties)
    {
      var columns = new List<string>(properties);

      if (!columns.Contains("Sample"))
      {
        columns.Insert(0, "Sample");
      }

      if (!columns.Contains("Dataset"))
      {
        columns.Insert(0, "Dataset");
      }

      if (!columns.Contains("SampleFile"))
      {
        columns.Add("SampleFile");
      }

      return columns;
    }
  }
}
