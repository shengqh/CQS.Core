using RCPA.Commandline;

namespace CQS.Genome.CNV
{
  public class CnMOPSCallProcessorCommand : AbstractCommandLineCommand<CnMOPSCallProcessorOptions>
  {
    public override string Name
    {
      get { return "cnmops_merge"; }
    }

    public override string Description
    {
      get { return "Merge overlapped Cn.MOPS calls"; }
    }

    public override RCPA.IProcessor GetProcessor(CnMOPSCallProcessorOptions options)
    {
      return new CnMOPSCallProcessor(options);
    }
  }
}
