using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using RCPA;

namespace CQS.Genome.Cuffdiff
{
  public class ReadGroupTrackingDirectoryExtractor:AbstractThreadFileProcessor
  {
    private string targetFile;
    private string gtfFile;

    public ReadGroupTrackingDirectoryExtractor(string targetFile, string gtfFile)
    {
      this.targetFile = targetFile;
      this.gtfFile = gtfFile;
    }

    public override IEnumerable<string> Process(string root)
    {
      return null;

    //  var trackingfiles = (from subdir in Directory.GetDirectories(root)
    //                 let tracking = subdir + "/genes.read_group_tracking"
    //                 where File.Exists(tracking)
    //                 select tracking).ToList();



    //  var filename = "count";

    //  var genes = new Dictionary<string, Dictionary<string, double>>();
    //  foreach (var dir in Directory.GetDirectories(root))
    //  {
    //    Console.WriteLine(dir);
    //    var file = Path.Combine(dir, "gene_exp.diff." + filename);
    //    var localgenes = new Dictionary<string, Dictionary<string, double>>();

    //    using (StreamReader sr = new StreamReader(file))
    //    {
    //      var line = sr.ReadLine();
    //      var samples = line.Split('\t');

    //      while ((line = sr.ReadLine()) != null)
    //      {
    //        if (string.IsNullOrWhiteSpace(line))
    //        {
    //          break;
    //        }

    //        var parts = line.Split('\t');
    //        Dictionary<string, double> gene;
    //        if (!localgenes.TryGetValue(parts[0], out gene))
    //        {
    //          gene = new Dictionary<string, double>();
    //          localgenes[parts[0]] = gene;
    //        }

    //        foreach (var group in samples)
    //        {
    //          var group = samples[index];
    //          double value;
    //          if (gene.TryGetValue(group, out value))
    //          {
    //            gene[group] = value + double.Parse(parts[index]);
    //          }
    //          else
    //          {
    //            gene[group] = double.Parse(parts[index]);
    //          }
    //        }
    //      }
    //    }


    //    foreach (var g in localgenes)
    //    {
    //      Dictionary<string, double> gene;
    //      if (!genes.TryGetValue(g.Key, out gene))
    //      {
    //        genes[g.Key] = new Dictionary<string, double>(g.Value);
    //      }
    //      else
    //      {
    //        foreach (var gg in g.Value)
    //        {
    //          double value;
    //          if (gene.TryGetValue(gg.Key, out value))
    //          {
    //            if (value != gg.Value)
    //            {
    //              Console.WriteLine(Path.GetFileName(Path.GetDirectoryName(file)) + "\t" + g.Key + "\t" + gg.Key + "\t" + value + "\t" + gg.Value);
    //            }
    //          }
    //          else
    //          {
    //            gene[gg.Key] = gg.Value;
    //          }
    //        }
    //      }
    //    }
    //  }

    //  using (StreamWriter sw = new StreamWriter(@"E:\sqh\Dropbox\Projects\vangard\VANGARD00028_liuqi_rnaseq\" + filename + ".tsv"))
    //  {
    //    var geneNames = (from key in genes.Keys
    //                     orderby key
    //                     select key).ToList();

    //    var groups = (from v in genes.Values
    //                  from g in v.Keys
    //                  select g).Distinct().ToList();
    //    groups.Sort();

    //    sw.Write("Gene");
    //    groups.ForEach(m => sw.Write("\t" + m));
    //    sw.WriteLine();

    //    foreach (var name in geneNames)
    //    {
    //      sw.Write(name);
    //      var gmap = genes[name];
    //      foreach (var group in groups)
    //      {
    //        if (gmap.ContainsKey(group))
    //        {
    //          sw.Write("\t" + gmap[group]);
    //        }
    //        else
    //        {
    //          sw.Write("\t0");
    //        }
    //      }
    //      sw.WriteLine();
    //    }
    //  }
    }
  }
}
