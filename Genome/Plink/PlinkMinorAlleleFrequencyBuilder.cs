using RCPA;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CQS.Genome.Plink
{
  public class PlinkMinorAlleleFrequencyBuilder : AbstractThreadProcessor
  {
    private PlinkMinorAlleleFrequencyBuilderOptions _options;

    public PlinkMinorAlleleFrequencyBuilder(PlinkMinorAlleleFrequencyBuilderOptions options)
    {
      _options = options;
    }

    public override IEnumerable<string> Process()
    {
      var file = new PlinkBedRandomFile(_options.InputFile);
      try
      {
        var locusList = file.Data.Locus;
        var individualList = file.Data.Individual;

        for (int i = 0; i < locusList.Count; i++)
        {
          var locus = locusList[i];
          var data = file.Read(locus.MarkerId);

          int count1 = 0;
          int count2 = 0;
          for (int j = 0; j < individualList.Count; j++)
          {
            if (PlinkData.IsMissing(data[0, j], data[1,j]))
            {
              continue;
            }

            if (data[0, j])
            {
              count2++;
            }
            else
            {
              count1++;
            }

            if (data[1, j])
            {
              count2++;
            }
            else
            {
              count1++;
            }
          }
          locus.Allele2Frequency = ((double)(count2)) / (count1 + count2);
        }

        PlinkLocus.WriteToFile(_options.OutputFile, locusList, false, true);
      }
      finally
      {
        file.Close();
      }

      return new string[] { _options.OutputFile };
    }
  }
}
