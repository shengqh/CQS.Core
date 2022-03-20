using RCPA;
using RCPA.Gui;
using RCPA.Gui.Command;

namespace CQS.TCGA
{
  public partial class TCGATreeBuilderUI : AbstractProcessorUI
  {
    public static readonly string title = " TCGA Data Tree Builder";
    public static readonly string version = "1.0.0";

    public TCGATreeBuilderUI()
    {
      InitializeComponent();

      targetDir.SetDirectoryArgument("DataDirectory", "TCGA Data Target");

      this.Text = Constants.GetSQHTitle(title, version);
    }

    protected override IProcessor GetProcessor()
    {
      var options = new TCGATreeBuilderOptions()
      {
        OutputDirectory = targetDir.FullName
      };
      return new TCGATreeBuilder(options);
    }
  }
}
