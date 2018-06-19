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

      if (!options.NoCategory)
      {
        //output miRNA
        Progress.SetMessage("Grouping microRNA by sequence ...");
        var miRNAGroup = features.Where(m => m.Name.StartsWith(SmallRNAConsts.miRNA)).GroupBySequence().OrderByDescending(m => m.GetEstimatedCount()).ThenBy(m => m.Name).ToList();

        //Progress.SetMessage("Writing microRNA xml file ...");
        //new FeatureItemGroupXmlFormat().WriteToFile(options.OutputFile + ".miRNA.xml", miRNAGroup);

        Progress.SetMessage("Writing microRNA ...");
        var miRNAFile = Path.ChangeExtension(options.OutputFile, SmallRNAConsts.miRNA + ".count");
        result.AddRange(new MirnaNTACountTableWriter().WriteToFile(miRNAFile, miRNAGroup, samples, SmallRNAConsts.miRNA + ":"));
        new SmallRNAPositionWriter().WriteToFile(miRNAFile + ".position", miRNAGroup);
        allGroups.AddRange(miRNAGroup);

        //output tRNA
        Progress.SetMessage("Grouping tRNA by anticodon ...");
        var tRNAs = features.Where(m => m.Name.StartsWith(SmallRNAConsts.tRNA)).ToList();
        var tRNAGroup = tRNAs.GroupByFunction(SmallRNAUtils.GetTrnaAnticodon).OrderByDescending(m => m.GetEstimatedCount()).ThenBy(m => m.Name).ToList();
        var tRNAFile = Path.ChangeExtension(options.OutputFile, SmallRNAConsts.tRNA + ".count");
        Progress.SetMessage("Writing tRNA anticodon ...");
        result.AddRange(new TrnaNTACountTableWriter().WriteToFile(tRNAFile, tRNAGroup, samples, SmallRNAConsts.tRNA + ":"));
        Progress.SetMessage("Writing tRNA anticodon position ...");
        new SmallRNAPositionWriter(m => SmallRNAUtils.GetTrnaAnticodon(m[0]), positionByPercentage: true).WriteToFile(tRNAFile + ".position", tRNAGroup);
        allGroups.AddRange(tRNAGroup);

        //output tRNA aminoacid 
        Progress.SetMessage("Grouping tRNA by amino acid ...");
        tRNAGroup = tRNAs.GroupByFunction(SmallRNAUtils.GetTrnaAminoacid, true).OrderByDescending(m => m.GetEstimatedCount()).ThenBy(m => m.Name).ToList();
        tRNAFile = Path.ChangeExtension(options.OutputFile, SmallRNAConsts.tRNA + ".aminoacid.count");
        Progress.SetMessage("Writing tRNA amino acid ...");
        result.AddRange(new SmallRNACountTableWriter().WriteToFile(tRNAFile, tRNAGroup, samples, SmallRNAConsts.tRNA + ":"));
        Progress.SetMessage("Writing tRNA aminoacid position ...");
        new SmallRNAPositionWriter(m => SmallRNAUtils.GetTrnaAminoacid(m[0]), positionByPercentage: true).WriteToFile(tRNAFile + ".position", tRNAGroup);

        var exportBiotypes = SmallRNAUtils.GetOutputBiotypes(options);
        foreach (var biotype in exportBiotypes)
        {
          OutputBiotype(samples, features, allGroups, result, biotype, m => m.StartsWith(biotype), !biotype.Equals(SmallRNABiotype.rRNA.ToString()), !biotype.Equals(SmallRNABiotype.rRNA.ToString()));
        }

        var biotypes = new[] { SmallRNAConsts.miRNA, SmallRNAConsts.tRNA }.Union(exportBiotypes).ToList();
        OutputBiotype(samples, features, allGroups, result, "", m => !biotypes.Any(l => m.StartsWith(l)), false, false);
      }
      else
      {
        Progress.SetMessage("Grouping features by identical query ...");
        allGroups = features.GroupByIdenticalQuery().OrderByDescending(m => m.GetEstimatedCount()).ThenBy(m => m.Name).ToList();
      }

      //output all smallRNA
      Progress.SetMessage("Writing all smallRNA ...");
      result.AddRange(new SmallRNACountTableWriter().WriteToFile(options.OutputFile, allGroups, samples, ""));

      Progress.SetMessage("Done ...");
      return result;
    }

    private void OutputBiotype(List<string> samples, List<FeatureItem> features, List<FeatureItemGroup> allGroups, List<string> result, string biotype, Func<string, bool> acceptName, bool exportPosition, bool exportSequence)
    {
      //output other smallRNA
      Progress.SetMessage("Grouping {0} by identical query ...", biotype);
      var groups = features.Where(m => acceptName(m.Name)).GroupByIdenticalQuery().OrderByDescending(m => m.GetEstimatedCount()).ThenBy(m => m.Name).ToList();
      allGroups.AddRange(groups);

      var name = string.IsNullOrEmpty(biotype) ? "other" : biotype;
      var prefix = string.IsNullOrEmpty(biotype) ? "" : biotype + ":";
      var file = Path.ChangeExtension(options.OutputFile, "." + name + ".count");
      Progress.SetMessage("Writing {0} ...", biotype);
      result.AddRange(new SmallRNACountTableWriter().WriteToFile(file, groups, samples, prefix));

      if (exportPosition)
      {
        Progress.SetMessage("Writing {0} position ...", biotype);
        new SmallRNAPositionWriter(positionByPercentage: true).WriteToFile(file + ".position", groups);
      }

      if (exportSequence)
      {
        var sequenceFile = Path.ChangeExtension(options.OutputFile, "." + name + ".sequence.count");
        result.AddRange(new SmallRNACountTableSequenceWriter().WriteToFile(sequenceFile, groups, prefix));
      }
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
