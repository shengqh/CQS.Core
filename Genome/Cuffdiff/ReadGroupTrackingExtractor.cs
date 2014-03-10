using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RCPA;
using System.IO;

namespace CQS.Genome.Cuffdiff
{
  public class ReadGroupTrackingExtractor : AbstractThreadFileProcessor
  {
    class SignificantItem
    {
      public string Gene { get; set; }
      public string GeneName { get; set; }
      public string GeneLocation { get; set; }
    }

    class TrackingItem
    {
      public string Gene { get; set; }
      public string Sample { get; set; }
      public string Count { get; set; }
      public string FPKM { get; set; }
    }

    private IEnumerable<string> inputFiles;
    private IEnumerable<string> significantFiles;
    private string groupSampleMapFile;

    public ReadGroupTrackingExtractor(IEnumerable<string> inputFiles, IEnumerable<string> significantFiles, string groupSampleMapFile)
    {
      this.inputFiles = inputFiles;
      this.significantFiles = significantFiles;
      this.groupSampleMapFile = groupSampleMapFile;
    }

    public override IEnumerable<string> Process(string outputFilePrefix)
    {
      this.Progress.SetMessage("Reading group sample map file ...");
      var groupSampleMap = new MapReader(0, 1).ReadFromFile(this.groupSampleMapFile);

      Dictionary<string, SignificantItem> geneNameMap = new Dictionary<string, SignificantItem>();

      this.Progress.SetMessage("Reading cuffdiff significant files ...");
      var sigs = (from file in this.significantFiles
                  from line in File.ReadAllLines(file).Skip(1)
                  let parts = line.Split('\t')
                  where parts.Length >= 3
                  let gene = parts[1]
                  let name = parts[2]
                  where !name.Equals("-")
                  let location = parts[3]
                  select new SignificantItem() { Gene = gene, GeneName = name, GeneLocation = location }).ToList();
      foreach (var gene in sigs)
      {
        if (!geneNameMap.ContainsKey(gene.Gene))
        {
          geneNameMap[gene.Gene] = gene;
        }
      }
      Func<string, bool> acceptGene = m => geneNameMap.ContainsKey(m);
      var countFile = outputFilePrefix + ".count";
      var fpkmFile = outputFilePrefix + ".fpkm";

      var items = new List<TrackingItem>();
      foreach (var trackingFile in this.inputFiles)
      {
        this.Progress.SetMessage("Reading cuffdiff read_group_tracking file " + trackingFile + "...");
        using (StreamReader sr = new StreamReader(trackingFile))
        {
          string line = sr.ReadLine();
          while ((line = sr.ReadLine()) != null)
          {
            var parts = line.Split('\t');
            if (parts.Length <= 7)
            {
              continue;
            }
            var gene = parts[0];
            if (!acceptGene(gene))
            {
              continue;
            }

            var group_index = parts[1] + "_" + parts[2];
            var sample = groupSampleMap.ContainsKey(group_index) ? groupSampleMap[group_index] : group_index;
            var count = parts[3];
            var fpkm = parts[6];
            var item = new TrackingItem() { Gene = gene, Sample = sample, Count = count, FPKM = fpkm };
            items.Add(item);
          }
        }
      }

      this.Progress.SetMessage("Preparing result ...");
      var samples = new HashSet<string>(from item in items
                                        select item.Sample).OrderBy(m => m).ToList();
      this.Progress.SetMessage(string.Format("There are {0} samples", samples.Count));

      var genes = new HashSet<string>(from item in items
                                      select item.Gene).OrderBy(m => m).ToList();
      this.Progress.SetMessage(string.Format("There are {0} genes", genes.Count));

      var map = ToDoubleDirectory(items);

      this.Progress.SetMessage("Removing empty genes ...");
      foreach (var gene in genes)
      {
        if (map[gene].All(m => m.Value.Count == "0"))
        {
          map.Remove(gene);
        }
      }

      var finalGenes = map.Keys.OrderBy(m => m).ToList();

      this.Progress.SetMessage("Outputing result ...");
      OutputFile(samples, finalGenes, map, geneNameMap, m => Math.Round(double.Parse(m.Count)).ToString(), countFile);
      OutputFile(samples, finalGenes, map, geneNameMap, m => m.FPKM, fpkmFile);

      this.Progress.End();
      return new[] { countFile, fpkmFile };
    }

    private static Dictionary<string, Dictionary<string, TrackingItem>> ToDoubleDirectory(List<TrackingItem> items)
    {
      var map = new Dictionary<string, Dictionary<string, TrackingItem>>();

      foreach (var item in items)
      {
        Dictionary<string, TrackingItem> map2;
        if (!map.TryGetValue(item.Gene, out map2))
        {
          map2 = new Dictionary<string, TrackingItem>();
          map[item.Gene] = map2;
        }

        map2[item.Sample] = item;
      }
      return map;
    }

    private static void OutputFile(List<string> samples, List<string> genes, Dictionary<string, Dictionary<string, TrackingItem>> map, Dictionary<string, SignificantItem> geneNameMap, Func<TrackingItem, string> fieldCount, string filename)
    {
      using (StreamWriter sw = new StreamWriter(filename))
      {
        sw.WriteLine("GeneId\tGene\tLocus\t" + samples.Merge("\t"));
        foreach (var gene in genes)
        {
          var item = geneNameMap[gene];

          sw.Write("{0}\t{1}\t{2}", item.Gene, item.GeneName, item.GeneLocation);
          var genemap = map[gene];
          foreach (var sample in samples)
          {
            if (genemap.ContainsKey(sample))
            {
              sw.Write("\t" + fieldCount(genemap[sample]));
            }
            else
            {
              sw.Write("\t");
            }
          }
          sw.WriteLine();
        }
      }
    }
  }
}
