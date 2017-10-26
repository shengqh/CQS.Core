using Bio.IO.SAM;
using CQS.Genome.Sam;
using RCPA;
using RCPA.Gui;
using RCPA.Seq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace CQS.Genome.Mapping
{
  public class ChromosomeCountSlimItemBuilder : ProgressClass
  {
    private ChromosomeCountProcessorOptions options;
    private Dictionary<string, string> nameMap = null;

    /// <summary>
    /// Constructor of ChromosomeCountSlimItemBuilder
    /// </summary>
    public ChromosomeCountSlimItemBuilder(ChromosomeCountProcessorOptions options)
    {
      this.options = options;
    }

    private string GetName(string sourceName)
    {
      if (this.nameMap == null)
      {
        if (options.KeepChrInName)
        {
          return sourceName;
        }
        
        if (sourceName.Length >= 4 && sourceName.StartsWith("chr") && char.IsLetterOrDigit(sourceName[3]))
        {
          return sourceName.StringAfter("chr");
        }

        return sourceName;
      }
      else
      {
        string value;
        if (nameMap.TryGetValue(sourceName, out value))
        {
          return nameMap[sourceName];
        }
        else
        {
          throw new Exception(string.Format("Cannot get name {0} from file {1}", sourceName, options.CategoryMapFile));
        }
      }
    }

    public List<ChromosomeCountSlimItem> Build(string fileName)
    {
      if (File.Exists(options.CategoryMapFile))
      {
        Progress.SetMessage("Reading name map file " + options.CategoryMapFile + " ...");
        nameMap = new MapItemReader(0, 1).ReadFromFile(options.CategoryMapFile).ToDictionary(m => m.Key, m => m.Value.Value);
      }

      var result = new List<ChromosomeCountSlimItem>();

      var queries = new Dictionary<string, SAMChromosomeItem>();
      var chromosomes = new Dictionary<string, ChromosomeCountSlimItem>();

      Regex chromosomeRegex = null;
      Func<string, bool> acceptChromosome;
      if (string.IsNullOrEmpty(options.ChromosomePattern))
      {
        acceptChromosome = m => true;
      }
      else
      {
        chromosomeRegex = new Regex(options.ChromosomePattern);
        acceptChromosome = m => chromosomeRegex.Match(m).Success;
      }

      Progress.SetMessage("Parsing alignment file " + fileName + " ...");
      using (var sr = SAMFactory.GetReader(fileName, true))
      {
        int count = 0;
        int waitingcount = 0;
        string line;
        while ((line = sr.ReadLine()) != null)
        {
          if (count % 1000 == 0)
          {
            if (Progress.IsCancellationPending())
            {
              throw new UserTerminatedException();
            }
          }

          if (count % 100000 == 0 && count > 0)
          {
            Progress.SetMessage("{0} candidates from {1} reads", waitingcount, count);
          }

          count++;

          var parts = line.Split('\t');

          SAMFlags flag = (SAMFlags)int.Parse(parts[SAMFormatConst.FLAG_INDEX]);

          //unmatched
          if (flag.HasFlag(SAMFlags.UnmappedQuery))
          {
            continue;
          }

          var seqname = GetName(parts[SAMFormatConst.RNAME_INDEX]);
          if (!acceptChromosome(seqname))
          {
            continue;
          }

          var qname = parts[SAMFormatConst.QNAME_INDEX];
          SAMChromosomeItem query;
          if (!queries.TryGetValue(qname, out query))
          {
            query = new SAMChromosomeItem();
            query.Qname = qname;
            queries[qname] = query;

            if (options.KeepSequence)
            {
              query.Sequence = parts[SAMFormatConst.SEQ_INDEX];
              if (flag.HasFlag(SAMFlags.QueryOnReverseStrand))
              {
                query.Sequence = SequenceUtils.GetReverseComplementedSequence(query.Sequence);
              }
            }
          }

          query.Chromosomes.Add(seqname);

          ChromosomeCountSlimItem item;
          if (!chromosomes.TryGetValue(seqname, out item))
          {
            item = new ChromosomeCountSlimItem();
            item.Names.Add(seqname);
            chromosomes[seqname] = item;
            result.Add(item);
          }
          item.Queries.Add(query);

          waitingcount++;
        }

        Progress.SetMessage("Finally, there are {0} candidates from {1} reads", waitingcount, count);
      }

      foreach (var query in queries.Values)
      {
        query.Chromosomes = query.Chromosomes.Distinct().OrderBy(m => m).ToList();
      }

      foreach (var sam in chromosomes.Values)
      {
        sam.Queries = sam.Queries.Distinct().OrderBy(m => m.Qname).ToList();
      }

      if (!string.IsNullOrEmpty(options.PreferPrefix))
      {
        foreach (var query in queries.Values)
        {
          if (query.Chromosomes.Any(l => l.StartsWith(options.PreferPrefix)))
          {
            var chroms = query.Chromosomes.Where(l => l.StartsWith(options.PreferPrefix)).ToArray();
            foreach (var chrom in chroms)
            {
              chromosomes[chrom].Queries.Remove(query);
              query.Chromosomes.Remove(chrom);
            }
          }
        }

        result.RemoveAll(l => l.Queries.Count == 0);
      }
      return result;
    }
  }
}
