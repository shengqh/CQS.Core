using CommandLine;

namespace CQS.Genome.Mapping
{
  public class ChromosomeCountTableBuilderOptions : SimpleDataTableBuilderOptions
  {
    [Option("categoryMapFile", Required = false, MetaValue = "FILE", HelpText = "Category mapping file, each line contains Id and Species")]
    public string CategoryMapFile { get; set; }

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
