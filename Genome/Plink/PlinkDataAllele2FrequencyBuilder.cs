using RCPA;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CQS.Genome.Plink
{
  public class PlinkDataAllele2FrequencyBuilder : AbstractThreadProcessor
  {
    private PlinkDataAllele2FrequencyBuilderOptions _options;

    public PlinkDataAllele2FrequencyBuilder(PlinkDataAllele2FrequencyBuilderOptions options)
    {
      _options = options;
    }

    public override IEnumerable<string> Process()
    {
      Progress.SetMessage("Reading data from " + _options.InputFile + "...");
      var data = _options.GetFileReader().ReadFromFile(_options.InputFile);
      var locusList = data.Locus;
      var individualList = data.Individual;
      for (int i = 0; i < locusList.Count; i++)
      {
        var locus = locusList[i];

        int count1 = 0;
        int count2 = 0;
        int validSample = 0;
        for (int j = 0; j < individualList.Count; j++)
        {
          if (data.IsMissing(i, j))
          {
            continue;
          }

          validSample++;

          if (data.IsHaplotype1Allele2[i, j])
          {
            count2++;
          }
          else
          {
            count1++;
          }

          if (data.IsHaplotype2Allele2[i, j])
          {
            count2++;
          }
          else
          {
            count1++;
          }
        }
        locus.Allele1Frequency = ((double)(count2)) / (count1 + count2);
        locus.TotalSample = individualList.Count;
        locus.ValidSample = validSample;
      }

      PlinkLocus.WriteToFile(_options.OutputFile, locusList, false, true);

      return new string[] { _options.OutputFile };
    }
  }
}
