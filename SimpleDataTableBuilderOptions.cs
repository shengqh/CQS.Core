using CommandLine;
using RCPA.Commandline;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CQS
{
  public class SimpleDataTableBuilderOptions : AbstractOptions
  {
    public SimpleDataTableBuilderOptions()
    { }

    [Option('l', "list", Required = true, MetaValue = "FILE", HelpText = "File contains count files, one file per line")]
    public string ListFile { get; set; }

    [Option('o', "output", Required = true, MetaValue = "FILE", HelpText = "Output file")]
    public string OutputFile { get; set; }

    public override bool PrepareOptions()
    {
      ValidateListFile();

      return ParsingErrors.Count == 0;
    }

    protected virtual void ValidateListFile()
    {
      if (!File.Exists(this.ListFile))
      {
        ParsingErrors.Add(string.Format("List file not exists {0}.", this.ListFile));
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
          }
        }
      }
    }

    public List<FileItem> GetCountFiles()
    {
      List<FileItem> result = new List<FileItem>();

      result = (from file in File.ReadAllLines(this.ListFile)
                where file.Trim().Length > 0
                let parts = file.Split('\t')
                where parts.Length > 1
                let additionalFile = parts.Length > 2 ? parts[2] : string.Empty
                select new FileItem()
                {
                  Name = parts[0],
                  File = parts[1],
                  AdditionalFile = additionalFile
                }).ToList();
      return result;
    }
  }
}
