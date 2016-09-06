using RCPA.Gui;
using RCPA.Gui.Command;
using RCPA.Gui.FileArgument;
using System.IO;

namespace CQS.Genome.Cuffdiff
{
  public partial class ReadGroupTrackingExtractorUI : AbstractProcessorUI
  {
    public static string title = "Cuffdiff Read Group Tracking Extractor";
    public static string version = "1.0.3";

    public ReadGroupTrackingExtractorUI()
    {
      InitializeComponent();

      trackingFile.FileArgument = new OpenFileArgument("Cuffdiff Read Group Tracking", "read_group_tracking");

      significantFile.FileArgument = new OpenFileArgument("Cuffdiff Significant", new[] { "sig", "diff" });

      mapFile.FileArgument = new OpenFileArgument("Cuffdiff Group/Sample Map", "map");

      this.Text = Constants.GetSqhVanderbiltTitle(title, version);
    }

    protected override RCPA.IProcessor GetProcessor()
    {
      var options = new ReadGroupTrackingExtractorOptions()
      {
        InputFiles = new[] { trackingFile.FullName },
        SignificantFiles = new[] { significantFile.FullName },
        MapFile = mapFile.FullName,
        OutputFilePrefix = Path.ChangeExtension(trackingFile.FullName, "")
      };

      return new ReadGroupTrackingExtractor(options);
    }
  }
}
