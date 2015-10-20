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
using CQS.Genome.Feature;

namespace CQS.Genome.SmallRNA
{
  public class SmallRNACountTableBuilderPlus : AbstractThreadProcessor
  {
    private SmallRNACountTableBuilderOptions options;

    public SmallRNACountTableBuilderPlus(SmallRNACountTableBuilderOptions options)
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
      foreach (var file in countfiles)
      {
        samples.Add(file.Name);

        Progress.SetMessage("Reading mapped file " + file.File + " ...");
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
      var miRNAGroup = features.Where(m => m.Name.StartsWith(SmallRNAConsts.miRNA)).GroupBySequence().OrderByDescending(m => m.EstimateCount).ThenBy(m => m.Name).ToList();
      var miRNAFile = Path.ChangeExtension(options.OutputFile, SmallRNAConsts.miRNA + ".count");
      result.AddRange(new MirnaNTACountTableWriter().WriteToFile(miRNAFile, miRNAGroup, samples, SmallRNAConsts.miRNA + ":"));
      allGroups.AddRange(miRNAGroup);

      //output tRNA
      var tRNAGroup = features.Where(m => m.Name.StartsWith(SmallRNAConsts.tRNA)).GroupByIdenticalQuery().OrderByDescending(m => m.EstimateCount).ThenBy(m => m.Name).ToList();
      var tRNAFile = Path.ChangeExtension(options.OutputFile, SmallRNAConsts.tRNA + ".count");
      result.AddRange(new SmallRNACountTableWriter().WriteToFile(tRNAFile, tRNAGroup, samples, SmallRNAConsts.tRNA + ":"));
      allGroups.AddRange(tRNAGroup);

      //output other smallRNA
      var otherGroups = features.Where(m => !m.Name.StartsWith(SmallRNAConsts.miRNA) && !m.Name.StartsWith(SmallRNAConsts.tRNA)).GroupByIdenticalQuery().OrderByDescending(m => m.EstimateCount).ThenBy(m => m.Name).ToList();
      var otherFile = Path.ChangeExtension(options.OutputFile, "other.count");
      result.AddRange(new SmallRNACountTableWriter().WriteToFile(otherFile, otherGroups, samples, ""));
      allGroups.AddRange(otherGroups);

      //output all smallRNA
      result.AddRange(new SmallRNACountTableWriter().WriteToFile(options.OutputFile, allGroups, samples, ""));

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
