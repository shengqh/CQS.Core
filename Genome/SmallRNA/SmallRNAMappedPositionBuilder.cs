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
using CQS.Genome.Feature;

namespace CQS.Genome.SmallRNA
{
  public class SmallRNAMappedPositionBuilder
  {
    public static void DrawImage(List<FeatureItemGroup> features, string title, string outputFile)
    {
      var format = new MappedItemGroupXmlFileFormat();

      features.Sort((m1, m2) => m2.GetEstimatedCount().CompareTo(m1.GetEstimatedCount()));

      using (StreamWriter sw = new StreamWriter(outputFile))
      {
        sw.WriteLine("File\tFeature\tStrand\tCount\tPosition\tPercentage");

        foreach (var group in features)
        {
          var item = group[0];
          Dictionary<long, double> positionCount = new Dictionary<long, double>();
          foreach (var region in item.Locations)
          {
            foreach (var loc in region.SamLocations)
            {
              for (long p = loc.SamLocation.Start; p <= loc.SamLocation.End; p++)
              {
                var offset = region.Strand == '+' ? p - region.Start : region.End - p;
                double v;
                if (!positionCount.TryGetValue(offset, out v))
                {
                  v = 0;
                }
                positionCount[offset] = v + loc.SamLocation.Parent.GetEstimatedCount();
              }
            }
          }

          var allcount = item.GetEstimatedCount();
          var keys = positionCount.Keys.ToList();
          keys.Sort();
          foreach (var key in keys)
          {
            sw.WriteLine("{0}\t{1}\t{2}\t{3:0.##}\t{4}\t{5:0.00}",
              title,
              item.Name,
              item.Locations[0].Strand,
              item.GetEstimatedCount(),
              key,
              positionCount[key] / allcount);
          }
        }
      }

      var rfile = new FileInfo(FileUtils.GetTemplateDir() + "/smallrna_position.r").FullName;
      if (File.Exists(rfile))
      {
        var targetr = (outputFile + ".r").Replace("\\", "/");
        var content = File.ReadAllText(rfile).Replace("$$workspace", Path.GetDirectoryName(Path.GetFullPath(outputFile)).Replace("\\", "/"))
          .Replace("$$positionfile", Path.GetFileName(outputFile).Replace("\\", "/"));
        File.WriteAllText(targetr, content);

        if (File.Exists(targetr))
        {
          var r = SystemUtils.IsLinux ? "R" : SystemUtils.GetRExecuteLocation();
          SystemUtils.Execute(r, "--vanilla -f " + targetr);
        }
      }
    }
  }
}
