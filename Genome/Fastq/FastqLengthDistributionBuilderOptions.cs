﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommandLine;
using CQS.Commandline;
using System.IO;

namespace CQS.Genome.Fastq
{
  public class FastqLengthDistributionBuilderOptions : AbstractOptions
  {
    [OptionList('i', "inputFiles", Required = true, MetaValue = "FILES", Separator = ',', HelpText = "Input fastq files, separated by ','")]
    public IList<string> InputFiles { get; set; }

    [Option('o', "outputFile", Required = false, MetaValue = "FILE", HelpText = "Output file")]
    public string OutputFile { get; set; }

    public override bool PrepareOptions()
    {
      foreach (var file in InputFiles)
      {
        if (!File.Exists(file))
        {
          ParsingErrors.Add(string.Format("Fastq file not exists {0}.", file));
          return false;
        }
      }

      if (string.IsNullOrEmpty(this.OutputFile))
      {
        this.OutputFile = InputFiles[0] + ".len";
      }

      return true;
    }
  }
}
