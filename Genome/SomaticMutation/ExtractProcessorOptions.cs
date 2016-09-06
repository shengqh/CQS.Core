using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CommandLine;
using CQS.Genome.Pileup;
using RCPA.Seq;
using System.Windows.Forms;
using CQS.Genome.Samtools;

namespace CQS.Genome.SomaticMutation
{
  public class ExtractProcessorOptions : MpileupOptions
  {
    private const int DEFAULT_MaximumReadDepth = 8000;

    public ExtractProcessorOptions()
    {
      IgnoreInsertionDeletion = true;
      IgnoreTerminalBase = false;
      IgnoreN = true;
      MaximumReadDepth = DEFAULT_MaximumReadDepth;
    }

    public bool IgnoreInsertionDeletion { get; set; }

    public bool IgnoreTerminalBase { get; set; }

    public bool IgnoreN { get; set; }

    [Option('v', "bed_file", MetaValue = "FILE", Required = false, HelpText = "Bed format file for sites")]
    public string BedFile { get; set; }

    [OptionList("bam_files", MetaValue = "FILES", Required = true, Separator = ',', HelpText = "Bam files, separated by ','")]
    public IList<string> BamFiles { get; set; }

    [OptionList("bam_names", MetaValue = "STRINGS", Required = true, Separator = ',', HelpText = "Bam names, separated by ','")]
    public IList<string> BamNames { get; set; }

    [Option("max_read_depth", MetaValue = "INT", DefaultValue = DEFAULT_MaximumReadDepth, HelpText = "Maximum read depth of base passed mapping quality filter in each sample")]
    public override int MaximumReadDepth { get; set; }

    [Option('o', "output", MetaValue = "STRING", Required = true, HelpText = "Output file")]
    public string OutputFile { get; set; }

    public PileupItemParser GetPileupItemParser()
    {
      return new PileupItemParser(0, MinimumBaseQuality, IgnoreInsertionDeletion, IgnoreN, IgnoreTerminalBase);
    }

    public override bool PrepareOptions()
    {
      Console.WriteLine("BAM file...");

      base.PrepareOptions();

      foreach (var file in BamFiles)
      {
        if (!File.Exists(file))
        {
          ParsingErrors.Add(string.Format("Bam file not exists : {0}", file));
        }
      }

      if (BamFiles.Count != BamNames.Count)
      {
        ParsingErrors.Add("Bam file count is not equals to the bam names.");
      }

      return ParsingErrors.Count == 0;
    }

    public IList<string> GetBamNames()
    {
      if (BamNames == null)
      {
        return (from file in BamFiles
                select Path.GetFileNameWithoutExtension(file)).ToList();
      }
      else
      {
        return BamNames;
      }
    }

    protected static string GetLinuxFile(string filename)
    {
      return Path.GetFullPath(filename).Replace("\\", "/");
    }

    public override void PrintParameter(TextWriter tw)
    {
      base.PrintParameter(tw);
      tw.WriteLine("#bam files: {0}", this.BamFiles.Merge(","));
      tw.WriteLine("#bam names: {0}", this.BamNames.Merge(","));
      tw.WriteLine("#output file: {0}", this.OutputFile);
      tw.WriteLine("#bed file: {0}", this.BedFile);
    }
  }
}