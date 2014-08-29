using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CQS.Genome.Rnaediting;
using RCPA;

namespace CQS.Genome.Annotation
{
  public class RnaeditingExporter : IAnnotationTsvExporter
  {
    private List<RnaeditItem> items;
    private Dictionary<string, List<RnaeditItem>> maps;
    Func<string, long, string> keyFunc;
    string header;
    string emptyStr;

    public RnaeditingExporter(string database, Func<string, long, string> keyFunc)
    {
      this.keyFunc = keyFunc;
      Console.WriteLine("reading rnaediting database " + database + " ...");
      this.items = new DarnedReader().ReadFromFile(database);
      Console.WriteLine("reading rnaediting database " + database + " finished.");
      this.maps = CollectionUtils.ToGroupDictionary(items, m => keyFunc(m.Chrom, m.Coordinate));
      Console.WriteLine("rnaediting directionary built.");
      this.header = (from m in new string[] { "strand", "inchr", "inrna", "gene", "seqReg", "exReg", "source", "PubMedID" }
                     let n = "rnaediting_" + m
                     select n).Merge("\t");
      this.emptyStr = new String('\t', header.Count(m => m == '\t'));
    }

    public string GetHeader()
    {
      return header;
    }

    public string GetValue(string chrom, long start, long end)
    {
      var key = keyFunc(chrom, start);
      if (maps.ContainsKey(key))
      {
        var items = maps[key];
        return string.Format("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}\t{7}",
           GetValue(items, m => m.Strand.ToString()),
           GetValue(items, m => m.NucleotideInChromosome.ToString()),
           GetValue(items, m => m.NucleotideInRNA.ToString()),
           GetValue(items, m => m.Gene.ToString()),
           GetValue(items, m => m.SeqReg.ToString()),
           GetValue(items, m => m.ExReg.ToString()),
           GetValue(items, m => m.Source.ToString()),
           GetValue(items, m => m.PubmedId.ToString()));
      }
      else
      {
        return emptyStr;
      }
    }

    private string GetValue(List<RnaeditItem> items, Func<RnaeditItem, string> func)
    {
      var strs = (from item in items
                  select func(item).Trim().Replace('\t',' ')).ToList();

      if (strs.All(m => string.IsNullOrEmpty(m)))
      {
        return string.Empty;
      }
      else
      {
        return strs.Merge(";");
      }
    }
  }
}
