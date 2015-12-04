using CommandLine;
using RCPA.Commandline;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CQS.Genome.Parclip
{
  public class SeedTargetBuilderOptions : AbstractTargetBuilderOptions
  {
    private const int DEFAULT_MaximumSeedLength = int.MaxValue;
    private const int DEFAULT_SeedOffset = 0;

    public SeedTargetBuilderOptions()
    {
      this.MaximumSeedLength = DEFAULT_MaximumSeedLength;
      this.SeedOffset = DEFAULT_SeedOffset;
    }

    [Option('i', "inputFile", Required = true, MetaValue = "FILE", HelpText = "Seed file, each line include one seed")]
    public string InputFile { get; set; }

    [Option("maximumSeedLength", DefaultValue = DEFAULT_MaximumSeedLength, MetaValue = "INTEGER", HelpText = "Maximum seed length")]
    public int MaximumSeedLength { get; set; }

    [Option("seedOffset", DefaultValue = DEFAULT_SeedOffset, MetaValue = "INTEGER", HelpText = "The seed offset of each candidate in seed file")]
    public override int SeedOffset { get; set; }

    public string[] ReadSeeds()
    {
      return (from line in File.ReadAllLines(this.InputFile)
              where !string.IsNullOrWhiteSpace(line)
              let seed = line.Trim().ToUpper()
              where seed.Length >= MinimumSeedLength
              select seed.Replace("U", "T")).ToArray();
    }

    public override bool PrepareOptions()
    {
      base.PrepareOptions();

      if (!File.Exists(this.InputFile))
      {
        ParsingErrors.Add(string.Format("Seed file not exists {0}.", this.InputFile));
      }
      else if (ReadSeeds().Length == 0)
      {
        ParsingErrors.Add(string.Format("No seed with minimum length {0} found in file {1}.", this.MinimumSeedLength, this.InputFile));
      }

      return ParsingErrors.Count == 0;
    }
  }
}
