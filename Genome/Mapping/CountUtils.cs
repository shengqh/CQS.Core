using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CQS.Genome.Mapping
{
  public static class CountUtils
  {
    private class KeyClass
    {
      public string TotalKey { get; set; }
      public string MappedKey { get; set; }
      public string FeatureKey { get; set; }
    }

    private static KeyClass Key1 = new KeyClass()
    {
      TotalKey = "Total reads",
      MappedKey = "Mapped reads",
      FeatureKey = "Feature reads"
    };

    private static KeyClass Key2 = new KeyClass()
    {
      TotalKey = "TotalReads",
      MappedKey = "MappedReads",
      FeatureKey = "FeatureReads"
    };

    public static bool WriteInfoSummaryFile(string targetFile, Dictionary<string, string> countfiles)
    {
      var infofiles = (from c in countfiles
                       let cfile = c.Value.EndsWith(".xml") ? Path.GetDirectoryName(c.Value) + "/" + Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(c.Value)) : c.Value
                       select new
                       {
                         Name = c.Key,
                         CountFile = cfile,
                         InfoFile = Path.ChangeExtension(cfile, ".info")
                       }).ToList();

      var notexists = infofiles.FirstOrDefault(m => !File.Exists(m.InfoFile));
      if (notexists != null)
      {
        Console.WriteLine("WriteInfoSummaryFile ignored, info file not exists : " + notexists.InfoFile);
        return false;
      }

      notexists = infofiles.FirstOrDefault(m => !File.Exists(m.CountFile));
      if (notexists != null)
      {
        Console.WriteLine("WriteInfoSummaryFile ignored, count file not exists : " + notexists.CountFile);
        return false;
      }

      var infos = (from f in infofiles
                   select new
                   {
                     Name = f.Name,
                     FeatureCount = File.ReadAllLines(f.CountFile).Length - 1,
                     Data = new MapItemReader(0, 1, hasHeader: false).ReadFromFile(f.InfoFile)
                   }).OrderBy(m => m.Name).ToList();

      using (StreamWriter sw = new StreamWriter(targetFile))
      {
        var keys = (from key in infos.First().Data.Keys
                    where !key.StartsWith("#")
                    select key).ToList();

        sw.Write("Name\t" + keys.Merge("\t"));
        KeyClass keyc = keys.Contains(Key1.TotalKey) ? Key1 : keys.Contains(Key2.TotalKey) ? Key2 : null;

        var percentage = keys.Contains(keyc.TotalKey) && keys.Contains(keyc.MappedKey) && keys.Contains(keyc.FeatureKey);
        if (percentage)
        {
          sw.WriteLine("\tMapped/Total\tFeature/Mapped\tFeature/Total\tFeatureCount");
        }
        else
        {
          sw.WriteLine("\tFeatureCount");
        }

        foreach (var info in infos)
        {
          sw.Write("{0}\t{1}", info.Name, (from key in keys
                                           select info.Data[key].Value).Merge("\t"));
          if (percentage)
          {
            sw.WriteLine("\t{0:0.00}%\t{1:0.00}%\t{2:0.00}%\t{3}",
              double.Parse(info.Data[keyc.MappedKey].Value) * 100 / double.Parse(info.Data[keyc.TotalKey].Value),
              double.Parse(info.Data[keyc.FeatureKey].Value) * 100 / double.Parse(info.Data[keyc.MappedKey].Value),
              double.Parse(info.Data[keyc.FeatureKey].Value) * 100 / double.Parse(info.Data[keyc.TotalKey].Value),
              info.FeatureCount
              );
          }
          else
          {
            sw.WriteLine("\t{0}", info.FeatureCount);
          }
        }
      }

      return true;
    }
  }
}
