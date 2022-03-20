using RCPA;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace CQS
{
  public class FileDefinitionBuilder : AbstractThreadProcessor
  {
    class FileItem
    {
      public string SampleName { get; set; }
      public List<string> FileNames { get; set; }
      public string GroupName { get; set; }

      public FileItem()
      {
        SampleName = string.Empty;
        FileNames = new List<string>();
        GroupName = string.Empty;
      }
    }

    private readonly FileDefinitionBuilderOptions _options;

    public FileDefinitionBuilder(FileDefinitionBuilderOptions options)
    {
      this._options = options;
    }

    private List<FileItem> GetFileItems()
    {
      var files = GetFiles(_options.InputDir);
      if (files.Length == 0)
      {
        throw new Exception("No file found in folder " + _options.InputDir);
      }

      if (_options.Verbose)
      {
        foreach (var file in files)
        {
          Progress.SetMessage("{0}", file);
        }
      }

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

      var result = files.GroupBy(m =>
      {
        if (_options.InputDir.StartsWith("gs://"))
        {
          m = m.Replace("gs:/", "");
        }

        if (_options.Recursion && _options.UseDirName)
        {
          return nameFunc(Path.GetFileName(Path.GetDirectoryName(m)));
        }
        else
        {
          return nameFunc(Path.GetFileName(m));
        }
      }).ToList().ConvertAll(n => new FileItem()
      {
        SampleName = n.Key,
        FileNames = n.ToList(),
        GroupName = string.Empty
      });

      if (_options.Verbose)
      {
        foreach (var file in result)
        {
          Progress.SetMessage("{0} => {1}", file.SampleName, file.FileNames.Merge(","));
        }
      }

      if (_options.AutoFill)
      {
        var nameMap = result.ToDictionary(l => l.SampleName, l => l);
        Regex number = new Regex(@"(.+?)(\d+)$");

        var numbers = (from n in nameMap.Keys
                       let m = number.Match(n)
                       where m.Success
                       select new { OldName = n, Prefix = m.Groups[1].Value, Value = m.Groups[2].Value }).ToList();

        var numberMax = numbers.Max(l => l.Value.Length);
        foreach (var num in numbers)
        {
          if (num.Value.Length != numberMax)
          {
            nameMap[num.OldName].SampleName = num.Prefix + new string('0', numberMax - num.Value.Length) + num.Value;
          }
        }
      }

      MapData namemap = null;
      if (File.Exists(_options.MapFile))
      {
        namemap = new MapDataReader(0, 1).ReadFromFile(_options.MapFile);
        if (_options.Verbose)
        {
          Progress.SetMessage("Reading name map from {0} ...", _options.MapFile);
          Progress.SetMessage("Current sample name ...");
          foreach (var file in result)
          {
            Progress.SetMessage("{0}", file.SampleName);
          }

          Progress.SetMessage("New map ...");
          foreach (var name in namemap.Data)
          {
            Progress.SetMessage("{0} => {1}", name.Key, name.Value.Value);
          }
        }

        var nameMap = result.ToDictionary(l => l.SampleName, l => l);

        var groupIndex = namemap.InfoNames.IndexOf("Group");
        foreach (var name in nameMap.Keys)
        {
          if (namemap.Data.ContainsKey(name))
          {
            nameMap[name].SampleName = namemap.Data[name].Value;
            if (groupIndex != -1)
            {
              nameMap[name].GroupName = namemap.Data[name].Informations[groupIndex].ToString();
            }
          }
          else
          {
            throw new Exception(string.Format("Cannot find key {0} in name map file {1}", name, _options.MapFile));
          }
        }
      }

      if (!string.IsNullOrEmpty(_options.GroupPattern))
      {
        foreach (var file in result)
        {
          var match = Regex.Match(file.SampleName, _options.GroupPattern);
          if (!match.Success)
          {
            throw new Exception(string.Format("Cannot find pattern {0} in file {1}", _options.NamePattern, file.SampleName));
          }

          var values = new List<string>();
          for (var i = 1; i < match.Groups.Count; i++)
          {
            values.Add(match.Groups[i].Value);
          }
          file.GroupName = values.Merge("");
        }
      }

      foreach (var file in result)
      {
        if (_options.InputDir.StartsWith("gs://"))
        {
          file.FileNames = (from f in file.FileNames
                            select f).ToList();
        }
        else
        {
          file.FileNames = (from f in file.FileNames
                            select Path.GetFullPath(f)).ToList();
        }
      }

      result.Sort((m1, m2) => m1.SampleName.CompareTo(m2.SampleName));
      return result;
    }

    public override IEnumerable<string> Process()
    {
      var fileItems = GetFileItems();

      var hasGroup = fileItems.Any(l => !string.IsNullOrEmpty(l.GroupName));

      if (!string.IsNullOrEmpty(_options.OutputFile))
      {
        Progress.SetMessage("Output to file {0}", _options.OutputFile);
        using (var sw = new StreamWriter(_options.OutputFile))
        {
          sw.Write("Name\tFile");
          if (hasGroup)
          {
            sw.Write("\tGroup");
          }
          sw.WriteLine();

          foreach (var file in fileItems)
          {
            sw.Write("{0}\t{1}", file.SampleName, file.FileNames.Merge(","));
            if (hasGroup)
            {
              sw.Write("\t{0}", file.GroupName);
            }
            sw.WriteLine();
          }
        }
        return new[] { _options.OutputFile };
      }
      else
      {
        var result = new List<string> { "files => {" };

        foreach (var file in fileItems)
        {
          result.Add(string.Format("  \"{0}\" => [{1}],", file.SampleName, (from l in file.FileNames select '"' + l + '"').Merge(", ")));
        }
        result.Add("},");

        if (hasGroup)
        {
          var groupList = fileItems.GroupBy(n => n.GroupName).OrderBy(l => l.Key).ToList();

          result.Add("groups => {");
          foreach (var sgroup in groupList)
          {
            result.Add(string.Format("  \"{0}\" => [{1}],", sgroup.Key, (from l in sgroup
                                                                         orderby l.SampleName
                                                                         select '"' + l.SampleName + '"').Merge(", ")));
          }
          result.Add("},");
        }

        return result;
      }
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
      if (directory.StartsWith("gs://"))
      {
        var proc = new Process
        {
          StartInfo = new ProcessStartInfo
          {
            FileName = "gsutil",
            Arguments = "ls " + directory,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            CreateNoWindow = true
          }
        };

        Console.Out.WriteLine("running command : " + proc.StartInfo.FileName + " " + proc.StartInfo.Arguments);
        try
        {
          if (!proc.Start())
          {
            Console.Out.WriteLine("Cannot start {0} command!", proc);
            return null;
          }
        }
        catch (Exception ex)
        {
          Console.Out.WriteLine("Cannot start {0} command : {1}", proc, ex.Message);
          return null;
        }

        string line;
        var result = new List<string>();
        while ((line = proc.StandardOutput.ReadLine()) != null)
        {
          result.Add(line);
        }
        return result.ToArray();
      }
      else
      {
        var result = new List<string>();
        DoGetFiles(directory, result);
        return result.ToArray();
      }
    }
  }
}
