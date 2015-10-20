using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using RCPA;
using CQS.Genome.Quantification;

namespace CQS
{
  public class DataTableBuilder : AbstractThreadProcessor
  {
    private readonly DataTableBuilderOptions _options;

    public DataTableBuilder(DataTableBuilderOptions options)
    {
      _options = options;
    }

    private class SampleData
    {
      public string Name { get; set; }
      public Dictionary<string, MapItem> Data { get; set; }
    }

    public override IEnumerable<string> Process()
    {
      var countfiles = _options.GetCountFiles();

      var reader = new MapItemReader(_options.KeyIndex, _options.ValueIndex, informationIndex: _options.InformationIndex,
                        hasHeader: !_options.HasNoHeader);
      reader.CheckEnd = m => m.StartsWith("no_feature");

      var counts = new List<SampleData>();
      foreach (var file in countfiles)
      {
        Progress.SetMessage("Reading data from {0} ...", file.File);
        var data = reader.ReadFromFile(file.File);
        counts.Add(new SampleData() { Name = file.Name, Data = data });
      }

      MapData namemap = null;
      if (File.Exists(_options.MapFile))
      {
        Progress.SetMessage("Reading name map from {0} ...", _options.MapFile);
        namemap = new MapDataReader(0, 1).ReadFromFile(_options.MapFile);
      }

      if (!string.IsNullOrEmpty(_options.KeyRegex))
      {
        Progress.SetMessage("Filtering key by pattern {0} ...", _options.KeyRegex);
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

      Progress.SetMessage("Writing {0} features to {1} ...", features.Count, _options.OutputFile);
      using (var sw = new StreamWriter(_options.OutputFile))
      {
        sw.Write("Feature");

        if (namemap != null)
        {
          sw.Write("\tFeature_{0}", namemap.ValueName);
          if (_options.ExportExtra)
          {
            sw.Write("\t{0}", (from v in namemap.InfoNames select "Feature_" + v).Merge("\t"));
          }
        }

        sw.WriteLine("\t" + (from c in counts select c.Name).Merge("\t"));

        foreach (var feature in features)
        {
          if ((from count in counts
               where count.Data.ContainsKey(feature)
               select count.Data[feature]).All(m => string.IsNullOrEmpty(m.Value) || m.Value.Equals("0")))
          {
            continue;
          }

          sw.Write(feature);
          if (namemap != null)
          {
            if (namemap.Data.ContainsKey(feature))
            {
              sw.Write("\t{0}", namemap.Data[feature].Value);
              if (_options.ExportExtra)
              {
                sw.Write("\t{0}", namemap.Data[feature].Informations.Merge("\t"));
              }
            }
            else
            {
              var fea = feature.StringBefore(":");
              var suffix = feature.Contains(":") ? ":" + feature.StringAfter(":") : string.Empty;
              var feas = fea.Split('+');
              var values = new List<string>();

              var findFeature = feas.FirstOrDefault(m => namemap.Data.ContainsKey(m));
              if (findFeature == null)
              {
                sw.Write("\t{0}", feature);
                if (_options.ExportExtra)
                {
                  sw.Write("\t{0}", (from f in namemap.InfoNames select string.Empty).Merge("\t"));
                }
              }
              else
              {
                sw.Write("\t{0}", (from f in feas
                                   select namemap.Data.ContainsKey(f) ? namemap.Data[f].Value : f).Merge("+") + suffix);
                if (_options.ExportExtra)
                {
                  for (int i = 0; i < namemap.InfoNames.Count; i++)
                  {
                    sw.Write("\t{0}", (from f in feas
                                       select namemap.Data.ContainsKey(f) ? namemap.Data[f].Informations[i] : string.Empty).Merge(";"));
                  }
                }
              }
            }
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

      if (File.Exists(_options.MapFile))
      {
        bool hasLength = false;
        using (var sr = new StreamReader(_options.MapFile))
        {
          var line = sr.ReadLine();
          if (line != null)
          {
            hasLength = line.Contains("length");
          }
        }

        if (hasLength)
        {
          Progress.SetMessage("Calculating FPKM values...");
          new HTSeqCountToFPKMCalculator(new HTSeqCountToFPKMCalculatorOptions()
          {
            InputFile = _options.OutputFile,
            GeneLengthFile = _options.MapFile,
            OutputFile = Path.ChangeExtension(_options.OutputFile, ".fpkm.tsv")
          })
          {
            Progress = this.Progress
          }.Process();
        }
      }

      return new[] { Path.GetFullPath(_options.OutputFile) };
    }
  }
}