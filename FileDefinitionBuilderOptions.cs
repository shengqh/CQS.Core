using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CQS.Commandline;
using CommandLine;
using System.IO;
using System.Text.RegularExpressions;

namespace CQS
{
  public class FileDefinitionBuilderOptions : AbstractOptions
  {
    [Option('i', "inputDir", Required = true, MetaValue = "DIR", HelpText = "Input directory")]
    public string InputDir { get; set; }

    [Option('f', "filePattern", MetaValue = "STRING", HelpText = "Regex pattern of file, example: .gz$")]
    public string FilePattern { get; set; }

    [Option('n', "namePattern", MetaValue = "STRING", HelpText = "Regex pattern of name in filename, example: \\(2280-RDB-\\\\d+\\)")]
    public string NamePattern { get; set; }

    [Option('r', "recursion", DefaultValue = false, HelpText = "Including sub directories")]
    public bool Recursion { get; set; }

    [Option('g', "groupPattern", MetaValue = "STRING", HelpText = "Regex pattern of group name in filename, example: \\(2280-RDB-\\\\d+\\)")]
    public string GroupPattern { get; set; }

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
