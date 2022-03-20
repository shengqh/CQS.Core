using RCPA;
using RCPA.Gui;
using RCPA.Gui.Command;
using RCPA.Gui.FileArgument;
using System.IO;

namespace CQS.Genome.Fastq
{
  public partial class Bam2FastqProcessorUI : AbstractProcessorUI
  {
    public static readonly string Title = "BAM to FastQ";
    public static readonly string Version = "1.0.0";

    public Bam2FastqProcessorUI()
    {
      InitializeComponent();

      bamFile.FileArgument = new OpenFileArgument("BAM", "bam");

      Text = Constants.GetSQHTitle(Title, Version);
    }

    protected override IProcessor GetProcessor()
    {
      var options = new Bam2FastqProcessorOptions
      {
        InputFile = bamFile.FullName,
        OutputPrefix = Path.ChangeExtension(bamFile.FullName, ".fastq"),
      };

      return new Bam2FastqProcessorCommand().GetProcessor(options);
    }
  }
}