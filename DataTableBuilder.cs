using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using RCPA;

namespace CQS
{
  public class DataTableBuilder : AbstractThreadProcessor
  {
    private readonly DataTableBuilderOptions _options;

    public DataTableBuilder(DataTableBuilderOptions options)
    {
      _options = options;
    }

    public override IEnumerable<string> Process()
    {
      var countfiles = _options.GetCountFiles();

      var reader = new MapItemReader(_options.KeyIndex, _options.ValueIndex, informationIndex: _options.InformationIndex,
                        hasHeader: !_options.HasNoHeader);
      reader.CheckEnd = m => m.StartsWith("no_feature");

      var counts = (from file in countfiles
                    let data = reader.ReadFromFile(file.File)
                    select new { Dir = file.Name, Data = data }).ToList();

      var namemap = new Dictionary<string, string>();
      if (File.Exists(_options.MapFile))
      {
        namemap = new MapReader(0, 1).ReadFromFile(_options.MapFile);
      }

      if (!string.IsNullOrEmpty(_options.KeyRegex))
      {
        var reg = new Regex(_options.KeyRegex);
        counts.ForEach(m =>
        {
          var keys = m.Data.Keys.ToList();
          foreach (var key in keys)
          {
            if (!reg.Match(key).Success)
            {
              m.Data.Remove(key);
            }
          }
        });
      }

      var features = (from c in counts
                      from k in c.Data.Keys
                      select k).Distinct().OrderBy(m => m).ToList();

      using (var sw = new StreamWriter(_options.OutputFile))
      {
        sw.Write("Feature");

        if (namemap.Count > 0)
        {
          sw.Write("\tName");
        }

        if (_options.InformationIndex != -1)
        {
          sw.Write("\tInformation");
        }

        sw.WriteLine("\t" + (from c in counts select c.Dir).Merge("\t"));

        foreach (var feature in features)
        {
          if ((from count in counts
               where count.Data.ContainsKey(feature)
               select count.Data[feature]).All(m => string.IsNullOrEmpty(m.Value) || m.Value.Equals("0")))
          {
            continue;
          }

          sw.Write(feature);
          if (namemap.Count > 0)
          {

            if (namemap.ContainsKey(feature))
            {
              sw.Write("\t" + namemap[feature]);
            }
            else
            {
              var fea = feature.StringBefore(":");
              var suffix = feature.Contains(":") ? ":" + feature.StringAfter(":") : string.Empty;
              var feas = fea.Split('+');
              var values = (from f in feas
                            select namemap.ContainsKey(f) ? namemap[f] : f).Merge("+") + suffix;
              sw.Write("\t" + values);
            }
          }

          if (_options.InformationIndex != -1)
          {
            var c = counts.First(m => m.Data.ContainsKey(feature));
            sw.Write("\t" + c.Data[feature].Information);
          }

          foreach (var count in counts)
          {
            if (count.Data.ContainsKey(feature))
            {
              sw.Write("\t" + count.Data[feature].Value);
            }
            else
            {
              sw.Write("\tNA");
            }
          }
          sw.WriteLine();
        }
      }
      return new[] { Path.GetFullPath(_options.OutputFile) };
    }
  }
}