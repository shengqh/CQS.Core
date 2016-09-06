using CQS.Genome.Feature;
using RCPA;
using RCPA.Utils;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CQS.Genome.SmallRNA
{
  public class SmallRNAMappedPositionBuilder
  {
    public static void DrawImage(List<FeatureItemGroup> features, string title, string outputFile, bool positionByPercentage=false, int minimumReadCount = 5)
    {
      //var format = new MappedItemGroupXmlFileFormat();

      var groups = minimumReadCount > 0 ? features.Where(l => l.GetEstimatedCount() >= minimumReadCount).ToList() : features;

      groups.Sort((m1, m2) => m2.GetEstimatedCount().CompareTo(m1.GetEstimatedCount()));

      using (StreamWriter sw = new StreamWriter(outputFile))
      {
        sw.WriteLine("File\tFeature\tStrand\tCount\tPosition\tPercentage");

        foreach (var group in groups)
        {
          var item = group[0];
          Dictionary<long, double> positionCount = new Dictionary<long, double>();
          foreach (var region in item.Locations)
          {
            var regionLength = region.Length;
            foreach (var loc in region.SamLocations)
            {
              Dictionary<long, double> curCount = new Dictionary<long, double>();
              var estimatedCount = loc.SamLocation.Parent.GetEstimatedCount();

              var offsetStart = region.Strand == '+' ? loc.SamLocation.Start - region.Start : region.End - loc.SamLocation.End;
              var offsetEnd = region.Strand == '+' ? loc.SamLocation.End - region.Start : region.End - loc.SamLocation.Start;
              if (positionByPercentage)
              {
                offsetStart = offsetStart * 100 / regionLength;
                offsetEnd = offsetEnd * 100 / regionLength;
              }

              double v;
              for (var offset = offsetStart; offset <= offsetEnd; offset++)
              {
                if (!curCount.TryGetValue(offset, out v) || v < estimatedCount)
                {
                  curCount[offset] = estimatedCount;
                }
              }

              foreach(var cc in curCount)
              {
                double vv;
                if(!positionCount.TryGetValue(cc.Key, out vv))
                {
                  positionCount[cc.Key] = cc.Value;
                }
                else
                {
                  positionCount[cc.Key] = vv + cc.Value;
                }
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
              allcount,
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
