using LumenWorks.Framework.IO.Csv;
using RCPA;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace CQS.Genome.Annotation
{
  public class AnnovarGenomeSummaryItem : SequenceRegion
  {
    public class Gene
    {
      public Gene()
      {
        this.Name = string.Empty;
        this.Annotation = string.Empty;
      }

      public string Name { get; set; }
      public string Annotation { get; set; }

      public override string ToString()
      {
        if (string.IsNullOrEmpty(this.Annotation))
        {
          return this.Name;
        }
        else
        {
          return string.Format("{0}({1})", this.Name, this.Annotation);
        }
      }
    }

    public AnnovarGenomeSummaryItem()
    {
      this.Genes = new List<Gene>();
    }

    public string Func { get; set; }

    public List<Gene> Genes { get; set; }

    public string ExonicFunc { get; set; }

    private readonly Regex _reg = new Regex(@"(.+)\((.+?)\)");

    public string GeneString
    {
      get
      {
        return (from g in Genes
                select g.ToString()).Merge(",");
      }
      set
      {
        this.Genes.Clear();
        var parts = value.Split(',');
        foreach (var part in parts)
        {
          var m = _reg.Match(part);
          if (m.Success)
          {
            var item = new Gene { Name = m.Groups[1].Value, Annotation = m.Groups[2].Value };
            if (!item.Name.Equals("NONE"))
            {
              this.Genes.Add(item);
            }
          }
          else
          {
            var item = new Gene { Name = part };
            this.Genes.Add(item);
          }
        }
      }
    }
  }

  public class AnnovarGenomeSummaryItemReader : IFileReader<List<AnnovarGenomeSummaryItem>>
  {
    public List<AnnovarGenomeSummaryItem> ReadFromFile(string fileName)
    {
      var result = new List<AnnovarGenomeSummaryItem>();

      using (var csv = new CsvReader(new StreamReader(fileName), true))
      {
        string[] headers = csv.GetFieldHeaders();
        var funcIndex = Array.IndexOf(headers, "Func");
        if (funcIndex == -1)
        {
          funcIndex = Array.IndexOf(headers, "Func.refGene");
        }

        var geneIndex = Array.IndexOf(headers, "Gene");
        if (geneIndex == -1)
        {
          geneIndex = Array.IndexOf(headers, "Gene.refGene");
        }

        var exonicFuncIndex = Array.IndexOf(headers, "ExonicFunc");
        if (exonicFuncIndex == -1)
        {
          exonicFuncIndex = Array.IndexOf(headers, "ExonicFunc.refGene");
        }

        var chrIndex = Array.IndexOf(headers, "Chr");
        var startIndex = Array.IndexOf(headers, "Start");
        var endIndex = Array.IndexOf(headers, "End");
        while (csv.ReadNextRecord())
        {
          var item = new AnnovarGenomeSummaryItem();
          item.Func = csv[funcIndex];
          item.GeneString = csv[geneIndex];
          item.ExonicFunc = csv[exonicFuncIndex];
          item.Seqname = csv[chrIndex];
          item.Start = long.Parse(csv[startIndex]);
          item.End = long.Parse(csv[endIndex]);
          result.Add(item);
        }
      }

      return result;
    }
  }

}
