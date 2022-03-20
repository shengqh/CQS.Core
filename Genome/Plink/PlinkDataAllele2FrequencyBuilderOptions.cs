using CommandLine;
using CQS.Genome.Gwas;
using RCPA;
using RCPA.Commandline;
using System;
using System.IO;

namespace CQS.Genome.Plink
{
  public class PlinkDataAllele2FrequencyBuilderOptions : AbstractOptions
  {
    [Option('i', "inputFile", Required = true, MetaValue = "FILE", HelpText = "Input file")]
    public string InputFile { get; set; }

    /// <summary>
    /// Input file format, 0:ped, 1:haps, 2:gen
    /// </summary>
    [Option('f', "format", Required = true, MetaValue = "STRING", HelpText = "Input file format, 0:ped, 1:haps, 2:gen")]
    public int FileFormat { get; set; }

    [Option('o', "outputFile", Required = false, MetaValue = "FILE", HelpText = "Output allele2 frequency file")]
    public string OutputFile { get; set; }

    public override bool PrepareOptions()
    {
      if (!File.Exists(this.InputFile))
      {
        ParsingErrors.Add(string.Format("Input file not exists {0}.", this.InputFile));
        return false;
      }

      try
      {
        GetFileReader();
      }
      catch (Exception ex)
      {
        ParsingErrors.Add(ex.Message);
        return false;
      }

      return true;
    }

    public IFileReader<PlinkData> GetFileReader()
    {
      switch (this.FileFormat)
      {
        case 0: return new PlinkPedFile();
        case 1: return new GwasHapsFormat();
        case 2: return new GwasGenFormat();
        default: throw new Exception("Unknown format " + this.FileFormat.ToString());
      }
    }
  }
}
