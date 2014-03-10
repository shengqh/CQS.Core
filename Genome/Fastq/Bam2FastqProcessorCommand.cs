using RCPA;
using RCPA.Gui.Command;

namespace CQS.Genome.Fastq
{
  public class Bam2FastqProcessorCommand : AbstractCommandLineCommand<Bam2FastqProcessorOptions>, IToolCommand
  {
    public override string Name
    {
      get { return "bam2fastq"; }
    }

    public override string Description
    {
      get { return "bam to fastq"; }
    }

    public string GetClassification()
    {
      return "FastQ";
    }

    public string GetCaption()
    {
      return Bam2FastqProcessorUI.Title;
    }

    public string GetVersion()
    {
      return Bam2FastqProcessorUI.Version;
    }

    public void Run()
    {
      new Bam2FastqProcessorUI().MyShow();
    }

    public override IProcessor GetProcessor(Bam2FastqProcessorOptions options)
    {
      if (FastqItemBAMParser.IsPaired(options.InputFile))
      {
        if (FastqItemBAMParser.IsSortedByName(options.InputFile))
        {
          return new Bam2PairedFastqNameSortedProcessor(options);
        }
        return new Bam2PairedFastqProcessor(options);
      }

      if (FastqItemBAMParser.IsSortedByName(options.InputFile))
      {
        return new Bam2SingleFastqNameSortedProcessor(options);
      }
      return new Bam2SingleFastqProcessor(options);
    }
  }
}