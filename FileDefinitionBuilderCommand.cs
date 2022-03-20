using RCPA.Commandline;

namespace CQS
{
  public class FileDefinitionBuilderCommand : AbstractCommandLineCommand<FileDefinitionBuilderOptions>
  {
    public override string Name
    {
      get { return "file_def"; }
    }

    public override string Description
    {
      get { return "Build file definition"; }
    }

    public override RCPA.IProcessor GetProcessor(FileDefinitionBuilderOptions options)
    {
      return new FileDefinitionBuilder(options);
    }
  }
}
