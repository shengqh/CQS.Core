using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CQS.Genome.Fastq;
using CQS.Genome.Sam;
using CQS.Genome.Mirna;
using CQS.Genome.Feature;
using CQS.Genome.Mapping;
using RCPA.Utils;
using RCPA;

namespace CQS.Genome.SmallRNA
{
  public class SmallRNABamInfoFixer : AbstractThreadProcessor
  {
    private SmallRNABamInfoFixerOptions options;

    public SmallRNABamInfoFixer(SmallRNABamInfoFixerOptions options)
    {
      this.options = options;
    }

    public override IEnumerable<string> Process()
    {
      var baminfofiles = Directory.GetFiles(options.RootDirectory, "*.bam.info", SearchOption.AllDirectories);
      var baminfogroup = baminfofiles.GroupBy(m => Path.GetDirectoryName(Path.GetDirectoryName(m))).ToList();

      bool singleDirectory = false;
      string singleDirectoryPath = "";
      var result = new List<string>();
      if (baminfogroup.Count == 1)
      {
        Progress.SetMessage("Single directory mode");
        singleDirectory = true;
        singleDirectoryPath = baminfogroup.First().Key;
        baminfogroup = baminfofiles.GroupBy(m => Path.GetDirectoryName(m)).ToList();
        result.Add(singleDirectoryPath);
      }
      else
      {
        Progress.SetMessage("Multiple directories mode");
      }

      Progress.SetMessage("Total {0} info files in {1} directory/directories found.", baminfofiles.Length, singleDirectory ? 1 : baminfogroup.Count);
      var files = new List<string>();
      int count = 0;
      foreach (var group in baminfogroup)
      {
        count++;
        var file = group.First();
        Progress.SetMessage("{0}/{1} : Checking {2} ...", count, baminfogroup.Count, file);
        var lines = File.ReadAllLines(file);
        var countfileline = lines.FirstOrDefault(m => m.StartsWith("#countFile"));
        if (string.IsNullOrWhiteSpace(countfileline))
        {
          Progress.SetMessage("  not count file used, ignore.");
          continue;
        }

        var countfile = countfileline.StringAfter("\t");
        if (!File.Exists(countfile))
        {
          Progress.SetMessage("  count file {0} not exist, ignore.", countfile);
          continue;
        }
        var countIndex = lines.ToList().FindIndex(m => m.StartsWith("TotalReads"));
        var totalCountInInfoFile = int.Parse(lines[countIndex].StringAfter("\t"));
        var totalCountInCountFile = new SmallRNACountMap(countfile).GetTotalCount();
        if (totalCountInInfoFile != totalCountInCountFile)
        {
          Progress.SetMessage(" Failed : {0} : {1} => {2}", file, totalCountInInfoFile, totalCountInCountFile);

          foreach (var f in group)
          {
            files.Add(f);
          }

          if (!singleDirectory)
          {
            result.Add(group.Key);
          }

          if (options.PerformUpdate && singleDirectory)
          {
            lines[countIndex] = "TotalReads\t" + totalCountInCountFile.ToString();
            File.WriteAllLines(file, lines);
          }
        }
      }

      if (options.PerformUpdate)
      {
        if (!singleDirectory)
        {
          Progress.SetMessage("Updating {0} info files from {1} groups ...", files.Count, result.Count);
          count = 0;
          foreach (var file in files)
          {
            count++;
            Progress.SetMessage("{0}/{1}: updating {2} ...", count, files.Count, file);

            var lines = File.ReadAllLines(file);
            var countfileline = lines.FirstOrDefault(m => m.StartsWith("#countFile"));
            var countfile = countfileline.StringAfter("\t");
            var countIndex = lines.ToList().FindIndex(m => m.StartsWith("TotalReads"));
            var totalCountInInfoFile = int.Parse(lines[countIndex].StringAfter("\t"));
            var totalCountInCountFile = new SmallRNACountMap(countfile).GetTotalCount();
            if (totalCountInInfoFile != totalCountInCountFile)
            {
              lines[countIndex] = "TotalReads\t" + totalCountInCountFile.ToString();
              File.WriteAllLines(file, lines);
            }
          }
          Progress.SetMessage("Please redo the category analysis which uses the information from following directoris :");
          foreach (var dir in result)
          {
            Progress.SetMessage("  " + dir);
          }
        }
        else
        {
          Progress.SetMessage("Please redo the category analysis which uses the information from following directory :");
          Progress.SetMessage("  " + singleDirectoryPath);
        }
      }
      else
      {
        if (files.Count > 0)
        {
          Progress.SetMessage("Total {0} info files from {1} groups need to be updated.", files.Count, result.Count);
          foreach (var dir in result)
          {
            Progress.SetMessage("  " + dir);
          }
          Progress.SetMessage("Please redo the smallrna_baminfo_fix with option --update in each directory and redo corresponding category analysis");
        }
        else
        {
          Progress.SetMessage("No failed counting found.");
        }
      }

      return result;
    }
  }
}