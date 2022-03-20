using RCPA;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CQS.Genome.Affymetrix
{
  public class ResToTsvConverter : AbstractThreadProcessor
  {
    private ResToTsvConverterOptions options;

    public ResToTsvConverter(ResToTsvConverterOptions options)
    {
      this.options = options;
    }

    public override IEnumerable<string> Process()
    {
      using (var sw = new StreamWriter(options.OutputFile))
      {
        using (var sr = new StreamReader(options.InputFile))
        {
          var headers = (from part in sr.ReadLine().Split('\t')
                         where !string.IsNullOrEmpty(part)
                         select part).Merge("\t");

          sw.WriteLine(headers);

          //skip second line
          sr.ReadLine();

          //skip third line
          sr.ReadLine();

          string line;
          while ((line = sr.ReadLine()) != null)
          {
            var parts = line.Split('\t');
            sw.Write("{0}\t{1}", parts[0], parts[1]);
            for (int i = 2; i < parts.Length; i += 2)
            {
              sw.Write("\t{0}", parts[i]);
            }
            sw.WriteLine();
          }
        }
      }

      return new[] { options.OutputFile };
    }
  }
}
