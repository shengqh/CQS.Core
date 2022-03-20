﻿using RCPA.Gui;
using RCPA.Gui.Command;
using RCPA.Gui.FileArgument;

namespace CQS.Genome.Mapping
{
  public partial class MappedCountProcessorUI : AbstractProcessorUI
  {
    public static string title = "Mapped Count Processor";
    public static string version = "1.0.1";

    private RcpaComboBox<string> engines;

    public MappedCountProcessorUI()
    {
      InitializeComponent();

      bamFile.FileArgument = new OpenFileArgument("BAM/SAM", new string[] { "bam", "sam" });
      gffFile.FileArgument = new OpenFileArgument("GFF", new string[] { "gff3", "bed" });
      fileFastq.FileArgument = new OpenFileArgument("FASTQ", new string[] { "fastq", "fq" });
      countFile.FileArgument = new OpenFileArgument("Count", "dupcount");

      engines = new RcpaComboBox<string>(cbEngine, "Engine", new string[] { "Bowtie1", "Bowtie2", "BWA" }, 0);
      AddComponent(engines);

      this.Text = Constants.GetSqhVanderbiltTitle(title, version);
    }

    protected override RCPA.IProcessor GetProcessor()
    {
      MappedCountProcessorOptions options = new MappedCountProcessorOptions()
      {
        InputFile = bamFile.FullName,
        CoordinateFile = gffFile.FullName,
        FastqFile = fileFastq.FullName,
        EngineType = engines.SelectedIndex + 1,
        CountFile = countFile.FullName,
        BedAsGtf = cbBedAsGtf.Checked
      };
      return new MappedCountProcessor(options);
    }
  }
}
