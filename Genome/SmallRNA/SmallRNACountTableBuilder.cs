using CQS.Genome.Feature;
using CQS.Genome.Mapping;
using RCPA;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CQS.Genome.SmallRNA
{
  public class SmallRNACountTableBuilder : AbstractThreadProcessor
  {
    private SmallRNACountTableBuilderOptions options;

    public SmallRNACountTableBuilder(SmallRNACountTableBuilderOptions options)
    {
      this.options = options;
    }

    private class CountItem
    {
      public string Dir { get; set; }
      public Dictionary<string, string[]> Data { get; set; }
    }

    public override IEnumerable<string> Process()
    {
      var countfiles = options.GetCountFiles();

      Dictionary<string, FeatureItem> featureMap = new Dictionary<string, FeatureItem>();
      List<string> samples = new List<string>();
      for (int i = 0; i < countfiles.Count; i++)
      {
        var file = countfiles[i];
        samples.Add(file.Name);

        Progress.SetMessage("Reading {0}/{1} {2}...", i + 1, countfiles.Count, file.File);
        var mapped = new FeatureItemGroupXmlFormat().ReadFromFile(file.File);
        mapped.GetQueries().ForEach(m => m.Sample = file.Name);

        //merge data by feature
        foreach (var group in mapped)
        {
          foreach (var curFeature in group)
          {
            FeatureItem existFeature;
            if (featureMap.TryGetValue(curFeature.Name, out existFeature))
            {
              var existLocationMap = existFeature.Locations.ToDictionary(l => l.GetLocation());
              foreach (var curLocation in curFeature.Locations)
              {
                FeatureLocation existLocation;
                if (existLocationMap.TryGetValue(curLocation.GetLocation(), out existLocation))
                {
                  existLocation.SamLocations.AddRange(curLocation.SamLocations);
                }
                else
                {
                  existFeature.Locations.Add(curLocation);
                }
              }
            }
            else // add to feature map
            {
              featureMap[curFeature.Name] = curFeature;
            }
          }
        }
      }

      var features = featureMap.Values.ToList();

      samples.Sort();

      var allGroups = new List<FeatureItemGroup>();
      var result = new List<string>();

      //output miRNA
      Progress.SetMessage("Grouping microRNA by sequence ...");
      var miRNAGroup = features.Where(m => m.Name.StartsWith(SmallRNAConsts.miRNA)).GroupBySequence().OrderByDescending(m => m.GetEstimatedCount()).ThenBy(m => m.Name).ToList();

      //Progress.SetMessage("Writing microRNA xml file ...");
      //new FeatureItemGroupXmlFormat().WriteToFile(options.OutputFile + ".miRNA.xml", miRNAGroup);

      Progress.SetMessage("Writing microRNA ...");
      var miRNAFile = Path.ChangeExtension(options.OutputFile, SmallRNAConsts.miRNA + ".count");
      result.AddRange(new MirnaNTACountTableWriter().WriteToFile(miRNAFile, miRNAGroup, samples, SmallRNAConsts.miRNA + ":"));
      allGroups.AddRange(miRNAGroup);

      //output tRNA
      Progress.SetMessage("Grouping tRNA by amino acid code ...");
      var tRNAs = features.Where(m => m.Name.StartsWith(SmallRNAConsts.tRNA)).ToList();
      var tRNAGroup = tRNAs.GroupByFunction(SmallRNAUtils.GetTRNACode).OrderByDescending(m => m.GetEstimatedCount()).ThenBy(m => m.Name).ToList();
      var tRNAFile = Path.ChangeExtension(options.OutputFile, SmallRNAConsts.tRNA + ".count");
      Progress.SetMessage("Writing tRNA ...");
      result.AddRange(new SmallRNACountTableWriter().WriteToFile(tRNAFile, tRNAGroup, samples, SmallRNAConsts.tRNA + ":"));
      allGroups.AddRange(tRNAGroup);

      //output tRNA
      Progress.SetMessage("Grouping tRNA by amino acid ...");
      tRNAGroup = tRNAs.GroupByFunction(SmallRNAUtils.GetTRNAAminoacid, true).OrderByDescending(m => m.GetEstimatedCount()).ThenBy(m => m.Name).ToList();
      tRNAFile = Path.ChangeExtension(options.OutputFile, SmallRNAConsts.tRNA + ".aminoacid.count");
      Progress.SetMessage("Writing tRNA aminoacid...");
      result.AddRange(new SmallRNACountTableWriter().WriteToFile(tRNAFile, tRNAGroup, samples, SmallRNAConsts.tRNA + ":"));

      //Progress.SetMessage("Grouping tRNA by identical query ...");
      //var tRNAGroup2 = tRNAs.GroupByIdenticalQuery().OrderByDescending(m => m.GetEstimateCount()).ThenBy(m => m.Name).ToList();
      //var tRNAFile2 = Path.ChangeExtension(options.OutputFile, SmallRNAConsts.tRNA + ".byquery.count");
      //Progress.SetMessage("Writing tRNA ...");
      //result.AddRange(new SmallRNACountTableWriter().WriteToFile(tRNAFile2, tRNAGroup2, samples, SmallRNAConsts.tRNA + ":"));

      //output other smallRNA
      Progress.SetMessage("Grouping other smallRNA by identical query ...");
      var otherGroups = features.Where(m => !m.Name.StartsWith(SmallRNAConsts.miRNA) && !m.Name.StartsWith(SmallRNAConsts.tRNA)).GroupByIdenticalQuery().OrderByDescending(m => m.GetEstimatedCount()).ThenBy(m => m.Name).ToList();
      var otherFile = Path.ChangeExtension(options.OutputFile, ".other.count");
      Progress.SetMessage("Writing other smallRNA ...");
      result.AddRange(new SmallRNACountTableWriter().WriteToFile(otherFile, otherGroups, samples, ""));

      var otherSequenceFile = Path.ChangeExtension(options.OutputFile, ".other.sequence.count");
      result.AddRange(new SmallRNACountTableSequenceWriter().WriteToFile(otherSequenceFile, otherGroups, ""));
      allGroups.AddRange(otherGroups);

      //new FeatureItemGroupXmlFormat().WriteToFile(options.OutputFile + ".other.xml", miRNAGroup);

      //output all smallRNA
      Progress.SetMessage("Writing all smallRNA ...");
      result.AddRange(new SmallRNACountTableWriter().WriteToFile(options.OutputFile, allGroups, samples, ""));

      Progress.SetMessage("Done ...");
      return result;
    }

    private static Dictionary<string, Dictionary<string, FeatureItemGroup>> GetSubset(Dictionary<string, Dictionary<string, FeatureItemGroup>> dic, Func<FeatureItemGroup, bool> accept)
    {
      var miRNAdic = new Dictionary<string, Dictionary<string, FeatureItemGroup>>();
      foreach (var v in dic)
      {
        var fdic = new Dictionary<string, FeatureItemGroup>();
        miRNAdic[v.Key] = fdic;
        foreach (var f in v.Value)
        {
          if (accept(f.Value))
          {
            fdic[f.Key] = f.Value;
          }
        }
      }
      return miRNAdic;
    }
  }
}
