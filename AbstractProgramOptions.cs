using RCPA.Commandline;

namespace CQS
{
  public abstract class AbstractProgramOptions : AbstractOptions
  {
    public CommandConfig Config { get; set; }

    public AbstractProgramOptions()
    {
      this.Config = new CommandConfig();
      this.Config.Load();
    }
  }
}
