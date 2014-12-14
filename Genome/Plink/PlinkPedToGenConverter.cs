using CQS.Genome.Gwas;
using RCPA;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CQS.Genome.Plink
{
  public class PlinkPedToGenConverter : AbstractThreadProcessor
  {
    private PlinkPedToGenConverterOptions _options;

    public PlinkPedToGenConverter(PlinkPedToGenConverterOptions options)
    {
      _options = options;
    }

    public override IEnumerable<string> Process()
    {
      Progress.SetMessage("Reading ... " + _options.InputFile + "...");
      var data = new PlinkPedFile().ReadFromFile(_options.InputFile);

      Progress.SetMessage("Saving " + _options.OutputFile + "...");
      new GwasGenFormat().WriteToFile(_options.OutputFile, data);

      return new[] { _options.OutputFile };
    }
  }
}
