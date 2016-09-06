using CommandLine;

namespace CQS.Genome.Mapping
{
  public class ChromosomeCountTableBuilderOptions : SimpleDataTableBuilderOptions
  {
    [Option("categoryMapFile", Required = false, MetaValue = "FILE", HelpText = "Category mapping file, each line contains Id and Species")]
    public string CategoryMapFile { get; set; }

    [Option("outputReadTable", Required = false, MetaValue = "BOOELAN", HelpText = "Output unique read count table")]
    public bool OutputReadTable { get; set; }

    [Option("outputReadContigTable", Required = false, MetaValue = "BOOELAN", HelpText = "Output read contig count table")]
    public bool OutputReadContigTable { get; set; }

    public override bool PrepareOptions()
    {
      base.PrepareOptions();

      if (!string.IsNullOrEmpty(CategoryMapFile))
      {
        CheckFile("categoryMapFile", CategoryMapFile);
      }

      return ParsingErrors.Count == 0;
    }
  }
}
