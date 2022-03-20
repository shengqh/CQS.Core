using RCPA;
using RCPA.Gui;
using RCPA.Gui.Command;
using RCPA.Gui.FileArgument;

namespace CQS.Genome.Annotation
{
  public partial class AnnovarGenomeSummaryRefinedResultBuilderUI : AbstractProcessorUI
  {
    public static string title = "Annovar gene summary refined result builder";
    public static string version = "1.0.1";

    public AnnovarGenomeSummaryRefinedResultBuilderUI()
    {
      InitializeComponent();

      this.annFile.FileArgument = new OpenFileArgument("Annovar Gene Summary", "tsv");
      this.affyFile.FileArgument = new OpenFileArgument("Affymetrix Annotation", "csv");

      this.Text = Constants.GetSQHTitle(title, version);
    }

    protected override IProcessor GetProcessor()
    {
      var options = new AnnovarGenomeSummaryRefinedResultBuilderOptions()
      {
        AffyAnnotationFile = this.affyFile.FullName,
        InputFile = this.annFile.FullName,
        OutputFile = this.annFile.FullName + ".xls"
      };

      return new AnnovarGenomeSummaryRefinedResultTsvBuilder(options);
    }
  }
}
