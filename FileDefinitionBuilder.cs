using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RCPA;
using System.IO;
using System.Text.RegularExpressions;

namespace CQS
{
  public class FileDefinitionBuilder : AbstractThreadProcessor
  {
    private readonly FileDefinitionBuilderOptions _options;

    public FileDefinitionBuilder(FileDefinitionBuilderOptions options)
    {
      this._options = options;
    }

    public override IEnumerable<string> Process()
    {
      var files = GetFiles(_options.InputDir);

      Func<string, string> nameFunc;
      if (string.IsNullOrEmpty(_options.NamePattern))
      {
        nameFunc = Path.GetFileNameWithoutExtension;
      }
      else
      {
        nameFunc = n =>
        {
          var match = Regex.Match(n, _options.NamePattern);
          if (match.Success)
          {
            var values = new List<string>();
            for (int i = 1; i < match.Groups.Count; i++)
            {
              values.Add(match.Groups[i].Value);
            }
            return values.Merge("");
          }
          else
          {
            return n;
          }
        };
      };

      var map = files.GroupBy(m =>
      {
        if (_options.Recursion && _options.UseDirName)
        {
          return nameFunc(Path.GetFileName(Path.GetDirectoryName(m)));
        }
        else
        {
          return nameFunc(Path.GetFileName(m));
        }
      }).ToDictionary(n => n.Key);


      var names = (from k in map.Keys
                   orderby k
                   select k).ToList();

      if (_options.AutoFill)
      {
        var nameMap = names.ToDictionary(l => l, l => l);
        Regex number = new Regex(@"(.+?)(\d+)$");

        var numbers = (from n in names
                       let m = number.Match(n)
                       where m.Success
                       select new { OldName = n, Prefix = m.Groups[1].Value, Value = m.Groups[2].Value }).ToList();

        var numberMax = numbers.Max(l => l.Value.Length);
        foreach (var num in numbers)
        {
          if (num.Value.Length != numberMax)
          {
            nameMap[num.OldName] = num.Prefix + new string('0', numberMax - num.Value.Length) + num.Value;
          }
        }

        map = map.ToDictionary(l => nameMap[l.Key], l => l.Value);
        names = (from k in map.Keys
                 orderby k
                 select k).ToList();
      }

      if (File.Exists(_options.MapFile))
      {
        Progress.SetMessage("Reading name map from {0} ...", _options.MapFile);
        var namemap = new MapDataReader(0, 1).ReadFromFile(_options.MapFile);
        map = map.ToDictionary(l => namemap.Data[l.Key].Value, l => l.Value);
        names = (from k in map.Keys
                 orderby k
                 select k).ToList();
      }

      var result = new List<string> { "files => {" };
      foreach (var name in names)
      {
        result.Add(string.Format("  \"{0}\" => [{1}],", name, (from l in map[name] select '"' + Path.GetFullPath(l) + '"').Merge(", ")));
      }
      result.Add("},");

      if (string.IsNullOrEmpty(_options.GroupPattern))
        return result;

      var groupmap = names.GroupBy(n =>
      {
        var match = Regex.Match(n, _options.GroupPattern);
        if (!match.Success)
        {
          throw new Exception(string.Format("Cannot find pattern {0} in file {1}", _options.NamePattern, n));
        }


        var values = new List<string>();
        for (var i = 1; i < match.Groups.Count; i++)
        {
          values.Add(match.Groups[i].Value);
        }
        return values.Merge("");

      });

      var gnames = (from k in groupmap
                    orderby k.Key
                    select k).ToList();

      result.Add("groups => {");
      foreach (var name in gnames)
      {
        result.Add(string.Format("  \"{0}\" => [{1}],", name.Key, (from l in name
                                                                   orderby l
                                                                   select '"' + l + '"').Merge(", ")));
      }
      result.Add("},");

      return result;
    }

    private void DoGetFiles(string directory, List<string> files)
    {
      var curFiles = Directory.GetFiles(directory).ToList();
      if (!string.IsNullOrEmpty(_options.FilePattern))
      {
        var reg = new Regex(_options.FilePattern);
        curFiles.RemoveAll(m => !reg.Match(Path.GetFileName(m)).Success);
      }
      files.AddRange(curFiles);

      if (_options.Recursion)
      {
        var dirs = Directory.GetDirectories(directory);
        foreach (var dir in dirs)
        {
          DoGetFiles(dir, files);
        }
      }
    }

    private string[] GetFiles(string directory)
    {
      var result = new List<string>();
      DoGetFiles(directory, result);
      return result.ToArray();
    }
  }
}
