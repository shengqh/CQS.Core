using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RCPA.Commandline;
using CommandLine;
using System.IO;
using System.Text.RegularExpressions;

namespace CQS
{
  public class FileDefinitionBuilderOptions : AbstractOptions
  {
    public FileDefinitionBuilderOptions()
    {
      UseDirName = false;
    }

    [Option('i', "inputDir", Required = true, MetaValue = "DIR", HelpText = "Input directory")]
    public string InputDir { get; set; }

    [Option('f', "filePattern", MetaValue = "STRING", HelpText = "Regex pattern of file, example: .gz$")]
    public string FilePattern { get; set; }

    [Option('n', "namePattern", MetaValue = "STRING", HelpText = "Regex pattern of name in filename, example: \\(2280-RDB-\\\\d+\\)")]
    public string NamePattern { get; set; }

    [Option('r', "recursion", DefaultValue = false, HelpText = "Including sub directories")]
    public bool Recursion { get; set; }

    [Option('d', "useDirName", HelpText = "Use sub directory name as name, must use with -r option")]
    public bool UseDirName { get; set; }

    [Option('a', "autoFill", HelpText = "Auto fill lastest number to longest number, for example, CMS-1 and CMS-100, CMS-1 will be filled as CMS-001")]
    public bool AutoFill { get; set; }

    [Option('g', "groupPattern", MetaValue = "STRING", HelpText = "Regex pattern of group name in filename, example: \\(2280-RDB-\\\\d+\\)")]
    public string GroupPattern { get; set; }

    [Option('m', "mapFile", Required = false, MetaValue = "FILE", HelpText = "key/name map file, the first two columns are key and name. The key should be equals to the name extract from file")]
    public string MapFile { get; set; }

    public override bool PrepareOptions()
    {
      if (!Directory.Exists(this.InputDir))
      {
        ParsingErrors.Add(string.Format("Directory not exists {0}.", this.InputDir));
      }

      CheckPattern(this.FilePattern, "File pattern");
      CheckPattern(this.NamePattern, "Name pattern");
      CheckPattern(this.GroupPattern, "Group pattern");

      return ParsingErrors.Count == 0;
    }
  }
}
