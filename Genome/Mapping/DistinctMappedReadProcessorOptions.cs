using CommandLine;
using RCPA.Commandline;
using CQS.Genome.Sam;

namespace CQS.Genome.Mapping
{
  public class DistinctMappedReadProcessorOptions : AbstractOptions
  {
    private const int DefaultEngineType = 1;

    public DistinctMappedReadProcessorOptions()
    {
      EngineType = DefaultEngineType;
    }

    [Option("inputfile1", Required = true, MetaValue = "FILE", HelpText = "Input count xml file 1")]
    public string InputFile1 { get; set; }

    [Option("inputfile2", Required = true, MetaValue = "FILE", HelpText = "Input count xml file 2")]
    public string InputFile2 { get; set; }

    [Option("outputfile1", Required = true, MetaValue = "FILE", HelpText = "Output count file 1")]
    public string OutputFile1 { get; set; }

    [Option("outputfile2", Required = true, MetaValue = "FILE", HelpText = "Output count file 2")]
    public string OutputFile2 { get; set; }

    [Option('e', "engineType", DefaultValue = DefaultEngineType, MetaValue = "INT",
      HelpText = "Engine type (1:bowtie1, 2:bowtie2, 3:bwa)")]
    public int EngineType { get; set; }

    public override bool PrepareOptions()
    {
      CheckFile("InputFile1", InputFile1);

      CheckFile("InputFile2", InputFile2);

      return ParsingErrors.Count == 0;
    }

    public ISAMFormat GetEngineFormat()
    {
      switch (EngineType)
      {
        case 2:
          return SAMFormat.Bowtie2;
        case 3:
          return SAMFormat.Bwa;
        default:
          return SAMFormat.Bowtie1;
      }
    }
  }
}