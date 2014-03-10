using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using RCPA.Gui;
using RCPA.Gui.Command;
using RCPA.Gui.FileArgument;

namespace CQS.Genome.Mirna
{
  public partial class MirnaCountProcessorUI : AbstractFileProcessorUI
  {
    public static string title = "miRNA Count Processor";
    public static string version = "1.0.3";

    private RcpaComboBox<string> engines;

    public MirnaCountProcessorUI()
    {
      InitializeComponent();

      SetFileArgument("SamFile", new OpenFileArgument("BAM/SAM", new[] { "bam", "sam" }));
      gffFile.FileArgument = new OpenFileArgument("GFF", new string[] { "gff3", "bed" });
      fileFastq.FileArgument = new OpenFileArgument("FASTQ", new string[] { "fastq", "fq" });
      countFile.FileArgument = new OpenFileArgument("Count", "dupcount");
      fileFasta.FileArgument = new OpenFileArgument("FASTA", new string[] { "fasta", "fa" });

      engines = new RcpaComboBox<string>(cbEngine, "Engine", new string[] { "Bowtie1", "Bowtie2", "BWA" }, 0);
      AddComponent(engines);

      this.Text = Constants.GetSqhVanderbiltTitle(title, version);
    }

    protected override RCPA.IFileProcessor GetFileProcessor()
    {
      MirnaCountProcessorOptions options = new MirnaCountProcessorOptions()
      {
        CoordinateFile = gffFile.FullName,
        FastqFile = fileFastq.FullName,
        EngineType = engines.SelectedIndex + 1,
        CountFile = countFile.FullName,
        BedAsGtf = cbBedAsGtf.Checked,
        FastaFile = fileFasta.FullName
      };
      return new MirnaCountProcessor(options);
    }
  }
}
