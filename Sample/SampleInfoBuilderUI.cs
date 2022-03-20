using RCPA.Gui;
using RCPA.Gui.Command;
using RCPA.Gui.FileArgument;
using System.IO;

namespace CQS.Sample
{
  public partial class SampleInfoBuilderUI : AbstractProcessorUI
  {
    public static string title = "GEO Sample Info Builder";
    public static string version = "1.0.0";

    public SampleInfoBuilderUI()
    {
      InitializeComponent();

      propertyFile.FileArgument = new OpenFileArgument("Column Definition", ".columns");

      this.Text = Constants.GetSqhVanderbiltTitle(title, version);
    }

    protected override RCPA.IProcessor GetProcessor()
    {
      return new SampleInfoBuilder(new SampleInfoBuilderOptions()
      {
        InputDirectory = rootDirectory.FullName,
        PropertyFile = propertyFile.FullName,
        OutputFile = Path.Combine(rootDirectory.FullName, Path.GetFileName(rootDirectory.FullName) + ".sample.tsv")
      });
    }

    public class Command : IToolCommand
    {
      #region IToolCommand Members

      public string GetClassification()
      {
        return "Sample";
      }

      public string GetCaption()
      {
        return title;
      }

      public string GetVersion()
      {
        return version;
      }

      public void Run()
      {
        new SampleInfoBuilderUI().MyShow();
      }

      #endregion
    }
  }
}
