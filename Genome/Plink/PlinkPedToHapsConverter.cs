using CQS.Genome.Gwas;
using RCPA;
using System.Collections.Generic;

namespace CQS.Genome.Plink
{
  public class PlinkPedToHapsConverter : AbstractThreadProcessor
  {
    private PlinkPedToHapsConverterOptions _options;

    public PlinkPedToHapsConverter(PlinkPedToHapsConverterOptions options)
    {
      _options = options;
    }

    public override IEnumerable<string> Process()
    {
      Progress.SetMessage("Reading " + _options.InputFile + "...");
      var data = new PlinkPedFile().ReadFromFile(_options.InputFile);

      Progress.SetMessage("Saving " + _options.OutputFile + "...");
      new GwasHapsFormat().WriteToFile(_options.OutputFile, data);

      return new[] { _options.OutputFile };
    }
  }
}
