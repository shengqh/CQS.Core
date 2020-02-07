using Bio.Util;
using CommandLine;
using RCPA.Commandline;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CQS.Genome.Annotation
{
  public class AnnovarResultMultipleToOneBuilderOptions : AbstractOptions
  {
    public AnnovarResultMultipleToOneBuilderOptions()
    { }

    [Option('i', "inputFile", Required = true, MetaValue = "FILE", HelpText = "Input file which lists the annovar files")]
    public string InputFile { get; set; }

    [Option('o', "outputFile", Required = false, MetaValue = "FILE", HelpText = "Output file")]
    public string OutputFile { get; set; }

    public override bool PrepareOptions()
    {
      if (!File.Exists(this.InputFile))
      {
        ParsingErrors.Add(string.Format("Input file not exists {0}.", this.InputFile));
        return false;
      }

      var lines = GetAnnovarFiles();
      if (lines.Count == 0)
      {
        ParsingErrors.Add(string.Format("Input file is empty {0}.", this.InputFile));
        return false;
      }

      var missed = (from l in lines
                    where !File.Exists(l.Key)
                    select l.Key).Union(
                   from l in lines
                   where !string.IsNullOrEmpty(l.Value) && !File.Exists(l.Value)
                   select l.Value).ToArray();
      if (missed.Length > 0)
      {
        ParsingErrors.Add(string.Format("One or more file are missing: \n{0}.", missed.Merge("\n")));
        return false;
      }

      return true;
    }

    /// <summary>
    /// Get File/Name map
    /// </summary>
    /// <returns></returns>
    public Dictionary<string, string> GetAnnovarFiles()
    {
      return (from line in File.ReadAllLines(this.InputFile)
              let file = line.Trim()
              where !string.IsNullOrEmpty(file)
              let parts = file.Split('\t', ' ')
              select new KeyValuePair<string, string>(parts[0], parts.Length > 1 ? parts[1] : string.Empty)).ToDictionary(
          m => m.Key, m => m.Value);
    }
  }
}
