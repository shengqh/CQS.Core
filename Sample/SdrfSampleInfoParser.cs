using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using RCPA;
using CQS.Sample;
using CQS.Ncbi.Geo;

namespace CQS.Sample
{
  public class SdrfSampleInfoParser : IRawSampleInfoReader
  {
    public Dictionary<string, Dictionary<string, List<string>>> ReadDescriptionFromDirectory(string dir)
    {
      var sdrfFile = GetSdrfFiles(dir);
      if (sdrfFile.Length == 0)
      {
        throw new ArgumentException("Cannot find sdrf file in directory " + dir);
      }

      var ann = new AnnotationFormat("^#").ReadFromFile(sdrfFile[0]);

      Dictionary<string, Dictionary<string, List<string>>> result = new Dictionary<string, Dictionary<string, List<string>>>();

      foreach (var a in ann)
      {
        string filename;
        if (a.Annotations.ContainsKey("Array Data File"))
        {
          filename = a.Annotations["Array Data File"] as string;
        }
        else if (a.Annotations.ContainsKey("GEO asscession number"))
        {
          filename = a.Annotations["GEO asscession number"] as string;
        }
        else
        {
          throw new Exception("I don't know how to get filename from " + sdrfFile[0]);
        }

        filename = Path.GetFileNameWithoutExtension(filename);
        var dic = new Dictionary<string, List<string>>();
        foreach (var kv in a.Annotations)
        {
          dic[kv.Key] = new string[] { kv.Value as string }.ToList();
        }

        result[filename] = dic;
      }

      return result;
    }

    public bool IsReaderFor(string directory)
    {
      return GetSdrfFiles(directory).Length > 0;
    }

    private static string[] GetSdrfFiles(string directory)
    {
      return Directory.GetFiles(directory, "*.sdrf.txt");
    }

    public string SupportFor
    {
      get { return "Dataset Sdrf File"; }
    }
  }
}
