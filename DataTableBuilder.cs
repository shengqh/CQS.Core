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

      var reader = new MapItemReader(_options.KeyIndex, _options.ValueIndex, informationIndex: _options.InformationIndex, hasHeader: !_options.HasNoHeader);
      reader.CheckEnd = m => m.StartsWith("__no_feature") || m.StartsWith("no_feature");

      var counts = new List<SampleData>();
      foreach (var file in countfiles)
      {
        Progress.SetMessage("Reading data from {0} ...", file.File);
        var data = reader.ReadFromFile(file.File);
        if (!string.IsNullOrEmpty(_options.KeyRegex))
        {
          var reg = new Regex(_options.KeyRegex);
          counts.Add(new SampleData() { Name = file.Name, Data = data.ToDictionary(l => reg.Match(l.Key).Groups[1].Value, l => l.Value) });
        }
        else
        {
          counts.Add(new SampleData() { Name = file.Name, Data = data });
        }
      }

      MapData namemap = null;
      if (File.Exists(_options.MapFile))
      {
        Progress.SetMessage("Reading name map from {0} ...", _options.MapFile);
        namemap = new MapDataReader(0, 1).ReadFromFile(_options.MapFile);

        if (!string.IsNullOrEmpty(_options.KeyRegex))
        {
          var reg = new Regex(_options.KeyRegex);
          namemap.Data = namemap.Data.ToDictionary(l => reg.Match(l.Key).Groups[1].Value, l => l.Value);
        }
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

      var missing = _options.FillMissingWithZero ? "0" : "NA";

      var outputExtra = _options.ExportExtra && namemap != null && namemap.InfoNames.Count > 0;

      Progress.SetMessage("Writing {0} features to {1} ...", features.Count, _options.OutputFile);
      using (var sw = new StreamWriter(_options.OutputFile))
      {
        sw.Write("Feature");

        if (namemap != null)
        {
          if (outputExtra)
          {
            sw.Write("\t{0}", (from v in namemap.InfoNames select "Feature_" + v).Merge("\t"));
          }
          sw.Write("\tFeature_{0}", namemap.ValueName);
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
            var feature2 = feature.StringBefore(".");
            if (namemap.Data.ContainsKey(feature))
            {
              if (outputExtra)
              {
                sw.Write("\t{0}", namemap.Data[feature].Informations.Merge("\t"));
              }
              sw.Write("\t{0}", namemap.Data[feature].Value);
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
                if (outputExtra)
                {
                  sw.Write("\t{0}", (from f in namemap.InfoNames select string.Empty).Merge("\t"));
                }
                sw.Write("\t{0}", feature);
              }
              else
              {
                if (outputExtra)
                {
                  for (int i = 0; i < namemap.InfoNames.Count; i++)
                  {
                    sw.Write("\t{0}", (from f in feas
                                       select namemap.Data.ContainsKey(f) ? namemap.Data[f].Informations[i] : string.Empty).Merge(";"));
                  }
                }
                sw.Write("\t{0}", (from f in feas
                                   select namemap.Data.ContainsKey(f) ? namemap.Data[f].Value : f).Merge("+") + suffix);
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
              sw.Write("\t" + missing);
            }
          }
          sw.WriteLine();
        }
      }

      //output proteincoding count table
      bool hasProteinCoding = namemap != null && namemap.InfoNames.Count > 0 && namemap.InfoNames.Contains("gene_biotype");
      if (hasProteinCoding)
      {
        WriteProteincodingFile(_options.OutputFile, ".count");
      }

      if (!_options.NoFPKM)
      {
        bool hasLength = namemap != null && namemap.InfoNames.Count > 0 && namemap.InfoNames.Contains("length");
        if (hasLength)
        {
          Progress.SetMessage("Calculating FPKM values...");
          var outputFile = Path.ChangeExtension(_options.OutputFile, ".fpkm.tsv");
          new HTSeqCountToFPKMCalculator(new HTSeqCountToFPKMCalculatorOptions()
          {
            InputFile = _options.OutputFile,
            GeneLengthFile = _options.MapFile,
            KeyRegex = _options.KeyRegex,
            OutputFile = outputFile
          })
          {
            Progress = this.Progress
          }.Process();

          if (hasProteinCoding)
          {
            WriteProteincodingFile(outputFile, ".tsv");
          }
        }
      }

      return new[] { Path.GetFullPath(_options.OutputFile) };
    }

    private static void WriteProteincodingFile(string inputFile, string extension)
    {
      var proteinCodingFile = Path.ChangeExtension(inputFile, ".proteincoding" + extension);
      using (var sr = new StreamReader(inputFile))
      {
        using (var sw = new StreamWriter(proteinCodingFile))
        {
          string line = sr.ReadLine();
          sw.WriteLine(line);
          while ((line = sr.ReadLine()) != null)
          {
            if (line.Contains("protein_coding"))
            {
              sw.WriteLine(line);
            }
          }
        }
      }
    }
  }
}