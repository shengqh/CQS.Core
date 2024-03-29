﻿using CommandLine;
using RCPA;
using RCPA.Commandline;

namespace CQS.TCGA
{
  public class TCGAClinicalInformationBuilderOptions : AbstractOptions
  {
    [Option('i', "clinicalFile", Required = true, MetaValue = "FILE", HelpText = "TCGA clinical information file")]
    public string ClinicalFile { get; set; }

    [Option('d', "dataFile", Required = true, MetaValue = "FILE", HelpText = "Data file generated by TCGA Data Table Builder, the first row should be gene+barcodes")]
    public string DataFile { get; set; }

    [Option('e', "throwException", HelpText = "Throw exception when sample has no corresponding clinical information")]
    public bool ThrowException { get; set; }

    [Option('o', "outputFile", Required = false, MetaValue = "FILE", HelpText = "Output file")]
    public string OutputFile { get; set; }

    public override bool PrepareOptions()
    {
      if (string.IsNullOrEmpty(OutputFile))
      {
        OutputFile = FileUtils.ChangeExtension(DataFile, ".patient.tsv");
      }

      return true;
    }
  }
}
