using RCPA;
using System;
using System.Collections.Generic;
using System.IO;

namespace CQS.Genome.Vcf
{
  public class VcfFilterProcessor : AbstractThreadProcessor
  {
    private VcfFilterProcessorOptions _options;

    public VcfFilterProcessor(VcfFilterProcessorOptions options)
    {
      this._options = options;
    }

    public override IEnumerable<string> Process()
    {
      Progress.SetMessage("Reading " + _options.InputFile + " ...");

      var tmpFile = _options.OutputFile + ".tmp";
      using (var sw = new StreamWriter(tmpFile))
      {
        using (var sr = new StreamReader(_options.InputFile))
        {
          string line;
          while ((line = sr.ReadLine()) != null)
          {
            if (line.StartsWith("##"))
            {
              sw.WriteLine(line);
            }
            else
            {
              break;
            }
          }

          sw.WriteLine(line);

          var headerparts = line.Split('\t');
          var formatIndex = Array.IndexOf(headerparts, "FORMAT");

          line = sr.ReadLine();
          var parts = line.Split('\t');
          var dpindex = Array.IndexOf(parts[formatIndex].Split(':'), "DP");

          int totalCount = 0;
          int savedCount = 0;
          while (line != null)
          {
            totalCount++;
            if (totalCount % 100000 == 0)
            {
              Progress.SetMessage("{0} out of {1} saved", savedCount, totalCount);
            }

            parts = line.Split('\t');
            if (parts.Length < 8)
            {
              break;
            }

            List<double> depths = new List<double>();
            for (int i = formatIndex + 1; i < parts.Length; i++)
            {
              var fparts = parts[i].Split(':');
              int depth;
              if (fparts.Length <= dpindex)
              {
                depths.Add(0);
              }
              else if (!int.TryParse(fparts[dpindex], out depth))
              {
                depths.Add(0);
              }
              else
              {
                depths.Add(depth);
              }
            }

            var median = MathNet.Numerics.Statistics.Statistics.Median(depths);
            if (median >= _options.MinimumMedianDepth)
            {
              savedCount++;
              sw.WriteLine(line);
            }

            line = sr.ReadLine();
          }
        }
      }

      File.Move(tmpFile, _options.OutputFile);

      return new string[] { _options.OutputFile };
    }
  }
}

