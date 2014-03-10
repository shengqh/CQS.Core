﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CQS.Commandline;
using CommandLine;
using System.IO;

namespace CQS
{
  public class DataTableBuilderOptions : SimpleDataTableBuilderOptions
  {
    private const string DEFAULT_KEY_REGEX = "";
    private const int DEFAULT_KEY_INDEX = 0;
    private const int DEFAULT_INFORMATION_INDEX = -1;
    private const int DEFAULT_VALUE_INDEX = 1;
    private const bool DEFAULT_HasNoHeader = false;

    public DataTableBuilderOptions()
    {
      KeyRegex = DEFAULT_KEY_REGEX;
      KeyIndex = DEFAULT_KEY_INDEX;
      InformationIndex = DEFAULT_INFORMATION_INDEX;
      ValueIndex = DEFAULT_VALUE_INDEX;
      HasNoHeader = DEFAULT_HasNoHeader;
    }

    [Option('p', "keyRegex", DefaultValue = DEFAULT_KEY_REGEX, MetaValue = "REGEX", HelpText = "Regex of key/feature")]
    public string KeyRegex { get; set; }

    [Option('m', "mapFile", Required = false, MetaValue = "FILE", HelpText = "key/name map file")]
    public string MapFile { get; set; }

    [Option('k', "keyIndex", DefaultValue = DEFAULT_KEY_INDEX, MetaValue = "INT", HelpText = "Index of key/feature")]
    public int KeyIndex { get; set; }

    [Option('i', "informationIndex", DefaultValue = DEFAULT_INFORMATION_INDEX, MetaValue = "INT", HelpText = "Index of information")]
    public int InformationIndex { get; set; }

    [Option('v', "valueIndex", DefaultValue = DEFAULT_VALUE_INDEX, MetaValue = "INT", HelpText = "Index of value")]
    public int ValueIndex { get; set; }

    [Option("noheader", DefaultValue = DEFAULT_HasNoHeader, MetaValue = "BOOL", HelpText = "Source file has no header information")]
    public bool HasNoHeader { get; set; }

    public override bool PrepareOptions()
    {
      if (!base.PrepareOptions())
      {
        return false;
      }

      if (!string.IsNullOrEmpty(this.MapFile) && !File.Exists(this.MapFile))
      {
        ParsingErrors.Add(string.Format("Map file not exists {0}.", this.MapFile));
        return false;
      }

      if (!CheckPattern(this.KeyRegex, "keyRegex"))
      {
        return false;
      }

      return true;
    }
  }
}
