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

namespace CQS.Genome.Cuffdiff
{
  public partial class ReadGroupTrackingExtractorUI : AbstractFileProcessorUI
  {
    public static string title = "Cuffdiff Read Group Tracking Extractor";
    public static string version = "1.0.3";

    public ReadGroupTrackingExtractorUI()
    {
      InitializeComponent();

      SetFileArgument("TrackingFile", new OpenFileArgument("Cuffdiff Read Group Tracking", "read_group_tracking"));

      significantFile.FileArgument = new OpenFileArgument("Cuffdiff Significant", new[] { "sig", "diff" });

      mapFile.FileArgument = new OpenFileArgument("Cuffdiff Group/Sample Map", "map");

      this.Text = Constants.GetSqhVanderbiltTitle(title, version);
    }

    protected override RCPA.IFileProcessor GetFileProcessor()
    {
      return new ReadGroupTrackingExtractor(new string[] { this.GetOriginFile() }, new string[] { significantFile.FullName }, mapFile.FullName);
    }
  }
}
