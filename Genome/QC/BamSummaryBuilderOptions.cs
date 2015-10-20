using CommandLine;
using RCPA.Commandline;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace CQS.Genome.QC
{
  public class BamSummaryShortBuilderOptions : AbstractOptions
  {
    public BamSummaryShortBuilderOptions()
    { }

    [Option('i', "inputDir", Required = true, MetaValue = "DIR", HelpText = "Input directory")]
    public string InputDir { get; set; }

    [Option('f', "filePattern", MetaValue = "STRING", DefaultValue = ".stat$", HelpText = "Regex pattern of file, example: .stat$")]
    public string FilePattern { get; set; }

    [Option('r', "recursion", DefaultValue = true, HelpText = "Including sub directories")]
    public bool Recursion { get; set; }

    [Option('o', "outputFile", Required = false, MetaValue = "FILE", HelpText = "Output file")]
    public string OutputFile { get; set; }

    public override bool PrepareOptions()
    {
      if (!Directory.Exists(this.InputDir))
      {
        ParsingErrors.Add(string.Format("Input directory not exists {0}.", this.InputDir));
      }

      CheckPattern(this.FilePattern, "File pattern");

      if (GetStatisticFiles().Count == 0)
      {
        ParsingErrors.Add(string.Format("Input directory doesn't contain bam statistic files: {0}", this.InputDir));
      }
      
      return ParsingErrors.Count == 0;
    }

    private void Fill(Regex reg, string dir, List<string> files)
    {
      files.AddRange(Directory.GetFiles(dir).Where(m => reg.Match(Path.GetFileName(m)).Success));
      if (Recursion)
      {
        foreach (var subdir in Directory.GetDirectories(dir))
        {
          Fill(reg, subdir, files);
        }
      }
    }

    public List<string> GetStatisticFiles()
    {
      var result = new List<string>();

      Regex reg = new Regex(FilePattern);

      Fill(reg, this.InputDir, result);

      return result;
    }
  }
}
