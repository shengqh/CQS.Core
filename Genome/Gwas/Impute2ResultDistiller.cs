using RCPA;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CQS.Genome.Gwas
{
  public class Impute2ResultDistiller : AbstractThreadProcessor
  {
    private Impute2ResultDistillerOptions _options;

    public Impute2ResultDistiller(Impute2ResultDistillerOptions options)
    {
      this._options = options;
    }

    private static char delimiter = ' ';

    public override IEnumerable<string> Process()
    {
      var targetSNPs = SNPItem.ReadFromFile(_options.TargetSnpFile);
      using (var sw = new StreamWriter(_options.OutputFile))
      using (var swInfo = new StreamWriter(_options.OutputFile + ".info"))
      {
        swInfo.WriteLine("MarkerId\tIsImputed");
        foreach (var file in _options.InputFiles)
        {
          int chromosome = DetectChromosome(file);
          Progress.SetMessage("Chromosome {0} : {1}", chromosome, file);

          var locusMap = targetSNPs.Where(m => m.Chrom == chromosome).ToDictionary(m => m.Position.ToString());
          using (var sr = new StreamReader(file))
          {
            string line;
            SNPItem item;
            while ((line = sr.ReadLine()) != null)
            {
              string[] parts = line.Take(delimiter, 3);
              bool isImputed = parts[0].Equals("---");
              if(isImputed)
              {
                if (locusMap.TryGetValue(parts[2], out item))
                {
                  var name = string.IsNullOrEmpty(item.Name) ? parts[1] : item.Name;
                  var markerid = string.IsNullOrEmpty(item.Dataset) ? name : item.Dataset + ":" + name;
                  sw.WriteLine("{0} {1}{2}",
                    item.Chrom,
                    markerid,
                    line.StringAfter(parts[1]));
                  swInfo.WriteLine("{0}\t{1}", markerid, isImputed);
                }
              }
              else
              {
                sw.WriteLine(line);
                swInfo.WriteLine("{0}\t{1}", parts[1], isImputed);
              }
            }
          }
        }
      }

      return new[] { _options.OutputFile };
    }

    private int DetectChromosome(string file)
    {
      using (var sr = new StreamReader(file))
      {
        string line;
        while ((line = sr.ReadLine()) != null)
        {
          string[] parts = line.Take(delimiter, 1);
          if (!parts[0].Equals("---"))
          {
            return int.Parse(parts[0]);
          }
        }
        throw new Exception(string.Format("No chromosome found in file {0}", file));
      }
    }
  }
}
