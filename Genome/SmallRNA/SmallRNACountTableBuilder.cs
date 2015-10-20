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

      var dic = new Dictionary<string, Dictionary<string, FeatureItemGroup>>();
      foreach (var file in countfiles)
      {
        Progress.SetMessage("Reading mapped file " + file.File + " ...");
        var mapped = new FeatureItemGroupXmlFormat().ReadFromFile(file.File);
        dic[file.Name] = mapped.ToDictionary(m => m.DisplayName);
      }

      var names = dic.Keys.OrderBy(m => m).ToList();

      var result = new List<string>();

      //output all data
      result.AddRange(new SmallRNACountTableWriter().WriteToFile(options.OutputFile, dic, ""));

      //output miRNA
      var miRNAMap = GetSubset(dic, m => m.DisplayName.StartsWith(SmallRNAConsts.miRNA));
      var miRNAFile = Path.ChangeExtension(options.OutputFile, SmallRNAConsts.miRNA + ".count");
      result.AddRange(new MirnaNTACountTableWriter().WriteToFile(miRNAFile, miRNAMap, SmallRNAConsts.miRNA + ":"));

      //output tRNA
      var tRNAMap = GetSubset(dic, m => m.DisplayName.StartsWith(SmallRNAConsts.tRNA));
      var tRNAFile = Path.ChangeExtension(options.OutputFile, SmallRNAConsts.tRNA + ".count");
      result.AddRange(new SmallRNACountTableWriter().WriteToFile(tRNAFile, tRNAMap, SmallRNAConsts.tRNA + ":"));

      //output other smallRNA
      var otherMap = GetSubset(dic, m => !m.DisplayName.StartsWith(SmallRNAConsts.miRNA) && !m.DisplayName.StartsWith(SmallRNAConsts.tRNA));
      var otherFile = Path.ChangeExtension(options.OutputFile, "other.count");
      result.AddRange(new SmallRNACountTableWriter().WriteToFile(otherFile, otherMap, ""));

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
