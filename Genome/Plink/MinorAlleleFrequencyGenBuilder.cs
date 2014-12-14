using RCPA;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CQS.Genome.Plink
{
  public class MinorAlleleFrequencyGenBuilder : AbstractThreadProcessor
  {
    private PlinkMinorAlleleFrequencyBuilderOptions _options;

    public MinorAlleleFrequencyGenBuilder(PlinkMinorAlleleFrequencyBuilderOptions options)
    {
      _options = options;
    }

    public override IEnumerable<string> Process()
    {
      var locusList = new List<PlinkLocus>();

      using (var sr = new StreamReader(_options.InputFile))
      {
        string line;
        while ((line = sr.ReadLine()) != null)
        {
          var parts = line.Split(' ');
          var locus = new PlinkLocus()
          {
            Chromosome = int.Parse(parts[0]),
            MarkerId = parts[1],
            PhysicalPosition = int.Parse(parts[2]),
            Allele1 = parts[3],
            Allele2 = parts[4]
          };
          locusList.Add(locus);

          var count1 = 0;
          var count2 = 0;
          for (int i = 5; i < parts.Length; i += 3)
          {
            if (parts[i].Equals("1"))
            {
              count1 += 2;
            }
            else if (parts[i + 1].Equals("1"))
            {
              count1++;
              count2++;
            }
            else if (parts[i + 2].Equals("1"))
            {
              count2 += 2;
            }
            else
            {//unknown, ignore 
              Console.Error.WriteLine(string.Format("Unknown, name={0}, i={1}, genotype={2} {3} {4}", locus.MarkerId, i, parts[i], parts[i + 1], parts[i + 2]));
            }
          }
          locus.Allele2Frequency = ((double)(count2)) / (count1 + count2);
        }
      }

      PlinkLocus.WriteToFile(_options.OutputFile, locusList, false, true);

      return new string[] { _options.OutputFile };
    }
  }
}
