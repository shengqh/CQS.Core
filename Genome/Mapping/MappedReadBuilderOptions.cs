using CommandLine;
using RCPA.Commandline;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CQS.Genome.Mapping
{
  public class MappedReadBuilderOptions : AbstractOptions
  {
    private const int DefaultMinQueryCount = 5;

    public MappedReadBuilderOptions()
    {
      this.MinQueryCount = DefaultMinQueryCount;
    }

    [Option('i', "inputFile", Required = true, MetaValue = "FILE", HelpText = "Read count file")]
    public string InputFile { get; set; }

    [OptionList('m', "mappedFiles", Required = true, Separator = ',', MetaValue = "FILES", HelpText = "Mapped read name files or bam/sam files")]
    public IList<string> MappedFiles { get; set; }

    [OptionList("fileTags", Required = false, Separator = ',', HelpText = "File tags, should be matched with mappedFiles")]
    public IList<string> FileTags { get; set; }

    [Option('o', "outputFile", Required = true, MetaValue = "FILE", HelpText = "Output read file")]
    public string OutputFile { get; set; }

    [Option("unmapped", Required = false, HelpText = "Extract non-perfect mapped reads instead of perfect mapped reads")]
    public bool Unmapped { get; set; }

    [Option("minQueryCount", Required = false, DefaultValue = DefaultMinQueryCount, HelpText = "Minimum query count of read for report")]
    public int MinQueryCount { get; set; }

    public override bool PrepareOptions()
    {
      if (!File.Exists(this.InputFile))
      {
        ParsingErrors.Add(string.Format("Input file is not exists: {0}.", this.InputFile));
        return false;
      }

      foreach (var file in MappedFiles)
      {
        if (!File.Exists(file))
        {
          ParsingErrors.Add(string.Format("Mapped file is not exists: {0}.", file));
          return false;
        }
      }

      if (!Unmapped)
      {
        if (FileTags == null || FileTags.Count == 0)
        {
          FileTags = (from f in MappedFiles
                      select Path.GetFileNameWithoutExtension(f)).ToList();
        }

        if (FileTags.Count != MappedFiles.Count)
        {
          ParsingErrors.Add(string.Format("The number of file tag {0} should be equals to number of mapped file {1}.", FileTags.Count, MappedFiles.Count));
          return false;
        }
      }

      return true;
    }

  }
}
