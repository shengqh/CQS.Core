using CQS.Genome.Gtf;
using RCPA;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CQS.Genome.Vcf
{
  public class GvcfValidationProcessor : AbstractThreadProcessor
  {
    private GvcfValidationProcessorOptions _options;

    public GvcfValidationProcessor(GvcfValidationProcessorOptions options)
    {
      this._options = options;
    }

    public override IEnumerable<string> Process()
    {
      var files = _options.GetGvcfFiles();
      foreach (var file in files)
      {
        Progress.SetMessage("Reading " + file + " ...");

        using (var sr = new StreamReader(file))
        {
          string line;
          while ((line = sr.ReadLine()) != null)
          {
            if (!line.StartsWith("##"))
            {
              break;
            }
          }

          var headerparts = line.Split('\t');
          var formatIndex = Array.IndexOf(headerparts, "FORMAT");
          int count = 0;
          while ((line = sr.ReadLine()) != null)
          {
            if (string.IsNullOrWhiteSpace(line))
            {
              break;
            }

            count++;
            if (count % 1000000 == 0)
            {
              Progress.SetMessage("{0}", count);
            }

            var parts = line.Split('\t');
            if (parts.Length < 8)
            {
              throw new Exception("Wrong column count : " + line + " in file " + file);
            }

            var dpindex = Array.IndexOf(parts[formatIndex].Split(':'), "MIN_DP");
            if (dpindex != -1)
            {
              var value = parts[formatIndex + 1].Split(':')[dpindex];
              int dp;
              if (!int.TryParse(value, out dp))
              {
                throw new Exception("Wrong MIN_DP format : " + value + " from " + line + " in file " + file);
              }
            }
          }
        }
      }
      return null;
    }
  }
}

