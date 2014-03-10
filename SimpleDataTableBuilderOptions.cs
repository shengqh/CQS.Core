using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CQS.Commandline;
using CommandLine;
using System.IO;

namespace CQS
{
  public class SimpleDataTableBuilderOptions : AbstractOptions
  {
    private const string DEFAULT_FILE_PATTERN = "*.count";

    public SimpleDataTableBuilderOptions()
    {
      FilePattern = DEFAULT_FILE_PATTERN;
    }

    [Option('r', "root", Required = false, MetaValue = "DIRECTORY", HelpText = "Directory whose sub directory contains data file")]
    public string RootDirectory { get; set; }

    [Option('l', "list", Required = false, MetaValue = "FILE", HelpText = "File contains count files, one file per line")]
    public string ListFile { get; set; }

    [Option('d', "directoryPattern", DefaultValue = "", MetaValue = "PATTERN", HelpText = "MutectPattern of data directory")]
    public string DirectoryPattern { get; set; }

    [Option('f', "filePattern", DefaultValue = DEFAULT_FILE_PATTERN, MetaValue = "PATTERN", HelpText = "MutectPattern of data file")]
    public string FilePattern { get; set; }

    [Option('o', "output", Required = true, MetaValue = "FILE", HelpText = "Sample mapped file")]
    public string OutputFile { get; set; }

    public override bool PrepareOptions()
    {
      if (string.IsNullOrEmpty(this.RootDirectory) && string.IsNullOrEmpty(this.ListFile))
      {
        ParsingErrors.Add(string.Format("Either root directory or list file should be defined."));
        return false;
      }

      if (!string.IsNullOrEmpty(this.ListFile))
      {
        if (!File.Exists(this.ListFile))
        {
          ParsingErrors.Add(string.Format("List file not exists {0}.", this.ListFile));
          return false;
        }
        else
        {
          var files = (from l in File.ReadAllLines(this.ListFile)
                       where l.Trim().Length > 0
                       select l).ToList();
          foreach (var file in files)
          {
            var parts = file.Split('\t');
            var curfile = parts.Length == 1 ? parts[0] : parts[1];
            if (!File.Exists(curfile))
            {
              ParsingErrors.Add(string.Format("Count file not exists {0}.", curfile));
              return false;
            }
          }
        }
      }
      else
      {
        if (!Directory.Exists(this.RootDirectory))
        {
          ParsingErrors.Add(string.Format("Root directory not exists {0}.", this.RootDirectory));
          return false;
        }
      }

      return true;
    }

    public List<FileItem> GetCountFiles()
    {
      List<FileItem> result;

      if (File.Exists(this.ListFile))
      {
        result = (from file in File.ReadAllLines(this.ListFile)
                  where file.Trim().Length > 0
                  let parts = file.Split('\t')
                  let namefile = parts.Length == 1
                  ? new FileItem()
                  {
                    Name = Path.GetFileName(file).StringBefore("."),
                    File = file
                  } : new FileItem()
                  {
                    Name = parts[0],
                    File = parts[1]
                  }
                  select namefile).ToList();
      }
      else
      {
        var subdirs = string.IsNullOrEmpty(this.DirectoryPattern) ? Directory.GetDirectories(this.RootDirectory) : Directory.GetDirectories(this.RootDirectory, this.DirectoryPattern);

        foreach (var dir in subdirs)
        {
          Console.WriteLine(dir);
        }

        result = (from dir in subdirs
                  let files = Directory.GetFiles(dir, this.FilePattern)
                  where files.Length > 0
                  select new FileItem() { Name = Path.GetFileName(dir), File = files[0] }).OrderBy(m => m.Name).ToList();
      }
      return result;
    }
  }
}
