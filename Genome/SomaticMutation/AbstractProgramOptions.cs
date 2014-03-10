using CQS.Commandline;

namespace CQS.Genome.SomaticMutation
{
  public abstract class AbstractProgramOptions : AbstractOptions
  {
    public CommandConfig Config { get; set; }
  }
}
