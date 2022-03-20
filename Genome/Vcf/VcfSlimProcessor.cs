using RCPA;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CQS.Genome.Vcf
{
  public class VcfSlimProcessor : AbstractThreadProcessor
  {
    private VcfSlimProcessorOptions _options;

    public VcfSlimProcessor(VcfSlimProcessorOptions options)
    {
      this._options = options;
    }

    public override IEnumerable<string> Process()
    {
      Progress.SetMessage("Reading " + _options.InputFile + " ...");

      var chrs = (from line in File.ReadAllLines(_options.DictFile).Skip(1)
                  where !string.IsNullOrWhiteSpace(line)
                  let parts = line.Split('\t')
                  let chr = parts[1].StringAfter("SN:")
                  select chr).ToList();
      Progress.SetMessage("Target sequence names: {0}", chrs.Merge(","));
      var chrHash = new HashSet<string>(chrs);
      var dbHasChr = chrs.All(m => m.StartsWith("chr"));

      using (var sw = new StreamWriter(_options.OutputFile))
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

          sw.WriteLine(line.Split('\t').Take(8).Merge('\t'));
          int count = 0;
          while ((line = sr.ReadLine()) != null)
          {
            var parts = line.Split('\t').ToArray();
            if (parts.Length < 8)
            {
              break;
            }

            if (!parts[6].Equals("PASS"))
            {
              continue;
            }
            count++;
            if (count % 100000 == 0)
            {
              Progress.SetMessage("{0} saved", count);
            }

            var export = chrHash.Contains(parts[0]);
            if (!export)
            {
              if (dbHasChr)
              {
                if (!parts[0].StartsWith("chr"))
                {
                  parts[0] = "chr" + parts[0];
                  export = chrHash.Contains(parts[0]);
                }
              }
              else
              {
                if (parts[0].StartsWith("chr"))
                {
                  parts[0] = parts[0].Substring(3);
                  export = chrHash.Contains(parts[0]);
                }
              }
            }

            if (export)
            {
              sw.WriteLine("{0}\t{1}", parts.Take(7).Merge("\t"), parts[7].StringBefore(";"));
            }
          }
        }
      }

      return new string[] { _options.OutputFile };
    }
  }
}

