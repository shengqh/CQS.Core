using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RCPA;
using System.IO;
using CQS.Genome.Sam;
using CQS.Genome.Gtf;
using Bio.IO.SAM;
using CQS.Genome.Bed;
using CQS.Genome.Fastq;
using System.Collections.Concurrent;
using System.Threading;
using RCPA.Commandline;
using CommandLine;
using System.Text.RegularExpressions;
using CQS.Genome.Mapping;
using CQS.Genome.Mirna;
using RCPA.Utils;

namespace CQS.Genome.Mapping
{
  public class MappedPositionBuilder : AbstractThreadProcessor
  {
    private SimpleDataTableBuilderOptions options;

    public MappedPositionBuilder(SimpleDataTableBuilderOptions options)
    {
      this.options = options;
    }

    public override IEnumerable<string> Process()
    {
      var format = new MappedItemGroupXmlFileFormat();

      using (StreamWriter sw = new StreamWriter(options.OutputFile))
      {
        sw.WriteLine("File\tFeature\tStrand\tCount\tPosition\tPercentage");
        foreach (var file in options.GetCountFiles())
        {
          var xmlfile = file.File.EndsWith(".xml") ? file.File : file.File + ".mapped.xml";

          var count = format.ReadFromFile(xmlfile).OrderByDescending(m => m.GetEstimatedCount()).ToList();

          foreach (var group in count)
          {
            var item = group[0];
            Dictionary<long, double> positionCount = new Dictionary<long, double>();
            foreach (var region in item.MappedRegions)
            {
              foreach (var loc in region.AlignedLocations)
              {
                for (long p = loc.Start; p <= loc.End; p++)
                {
                  var offset = region.Region.Strand == '+' ? p - region.Region.Start + 1 : region.Region.End - p + 1;
                  double v;
                  if (!positionCount.TryGetValue(offset, out v))
                  {
                    v = 0;
                  }
                  positionCount[offset] = v + loc.Parent.GetEstimatedCount();
                }
              }
            }

            var allcount = item.GetEstimatedCount();
            var keys = positionCount.Keys.ToList();
            keys.Sort();
            foreach (var key in keys)
            {
              sw.WriteLine("{0}\t{1}\t{2}\t{3:0.##}\t{4}\t{5:0.00}",
                file.Name,
                item.Name,
                item.MappedRegions.First().Region.Strand,
                item.GetEstimatedCount(),
                key,
                positionCount[key] / allcount);
            }
          }
        }
      }

      var rfile = new FileInfo(FileUtils.GetTemplateDir() + "/smallrna_position.r").FullName;
      if (File.Exists(rfile))
      {
        var targetr = Path.ChangeExtension(options.OutputFile, ".r").Replace("\\", "/");
        var content = File.ReadAllText(rfile).Replace("$$workspace", Path.GetDirectoryName(Path.GetFullPath(options.OutputFile)).Replace("\\", "/"))
          .Replace("$$positionfile", Path.GetFileName(options.OutputFile).Replace("\\", "/"));
        File.WriteAllText(targetr, content);

        if (File.Exists(targetr))
        {
          SystemUtils.Execute("R", "--vanilla -f " + targetr);
        }
      }

      return new string[] { Path.GetFullPath(options.OutputFile) };
    }
  }
}
