using RCPA.Gui.Command;

namespace CQS.Sample
{
  public class SampleInfoColumnDefinitionBuilderCommand : IToolCommand
  {
    #region IToolCommand
    public string GetClassification()
    {
      return "Sample";
    }

    public string GetCaption()
    {
      return SampleInfoColumnDefinitionBuilderUI.Title;
    }

    public string GetVersion()
    {
      return SampleInfoColumnDefinitionBuilderUI.Version;
    }

    public void Run()
    {
      new SampleInfoColumnDefinitionBuilderUI().MyShow();
    }
    #endregion IToolCommand
  }
}
