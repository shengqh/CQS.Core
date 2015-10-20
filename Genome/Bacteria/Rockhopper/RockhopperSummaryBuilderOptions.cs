using CommandLine;
using RCPA.Commandline;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CQS.Genome.Bacteria.Rockhopper
{
  public class RockhopperSummaryBuilderOptions : AbstractOptions
  {
    public RockhopperSummaryBuilderOptions()
    {
      MinFoldChange = 2.0;
      MaxQvalue = 0.05;
      Prefix = "";
    }

    [OptionList('i', "comparisonDirs", Required = true, MetaValue = "DIRECTORY", Separator = ',', HelpText = "rockhopper comparison directories")]
    public IList<string> ComparisonDirs { get; set; }

    [Option('m', "mapFile", Required = true, MetaValue = "FILE", HelpText = "Fastq/Sample/Group map file")]
    public string MapFile { get; set; }

    [Option('o', "targetDir", Required = false, MetaValue = "DIRECTORY", HelpText = "Target directory to save summary files")]
    public string TargetDir { get; set; }

    [Option('f', "minFoldChange", Required = false, MetaValue = "DOUBLE", DefaultValue = 2.0, HelpText = "Minimum fold change")]
    public double MinFoldChange { get; set; }

    [Option('q', "maxQvalue", Required = false, MetaValue = "DOUBLE", DefaultValue = 0.05, HelpText = "Maximum Q value")]
    public double MaxQvalue { get; set; }

    [Option('p', "prefix", Required = false, MetaValue = "STRING", DefaultValue = "", HelpText = "Prefix of result files")]
    public string Prefix { get; set; }

    public override bool PrepareOptions()
    {
      var dirs = this.ComparisonDirs.Where(m => !Directory.Exists(m)).ToList();
      if (dirs.Count > 0)
      {
        ParsingErrors.Add(string.Format("Directory not exists:\n{0}.", dirs.Merge("\n")));
        return false;
      }

      dirs = this.ComparisonDirs.Where(dir =>
      {
        var summaryfile = dir + "/summary.txt";
        var transcriptFile = Directory.GetFiles(dir).Where(m => m.EndsWith("_transcripts.txt")).FirstOrDefault();
        var operonFile = Directory.GetFiles(dir).Where(m => m.EndsWith("_operons.txt")).FirstOrDefault();
        return !File.Exists(summaryfile) || transcriptFile == null || operonFile == null;
      }).ToList();
      if (dirs.Count > 0)
      {
        ParsingErrors.Add(string.Format("Directory doesn't contain rockhopper result:\n{0}.", dirs.Merge("\n")));
        return false;
      }

      if (!File.Exists(this.MapFile))
      {
        ParsingErrors.Add(string.Format("Map file not exists {0}.", this.MapFile));
        return false;
      }

      return true;
    }
  }
}
