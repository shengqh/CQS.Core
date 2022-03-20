using RCPA.Gui;
using RCPA.Gui.Command;
using RCPA.Gui.FileArgument;

namespace CQS.Genome.Mirna
{
  public partial class MirnaMappedOverlapBuilderUI : AbstractProcessorUI
  {
    public static string title = "miRNA Overlapped Result Builder";
    public static string version = "1.0.0";

    public MirnaMappedOverlapBuilderUI()
    {
      InitializeComponent();

      fileTarget.FileArgument = new SaveFileArgument("Target", "tsv");
      refFile.FileArgument = new OpenFileArgument("Reference Mapped", "mapped.xml");
      samFile.FileArgument = new OpenFileArgument("Sample Mapped", "mapped.xml");

      this.Text = Constants.GetSqhVanderbiltTitle(title, version);
    }

    protected override RCPA.IProcessor GetProcessor()
    {
      MirnaMappedOverlapBuilderOptions options = new MirnaMappedOverlapBuilderOptions()
      {
        ReferenceFile = refFile.FullName,
        SampleFile = samFile.FullName,
        OutputFile = fileTarget.FullName
      };
      return new MirnaMappedOverlapBuilder(options);
    }
  }
}
