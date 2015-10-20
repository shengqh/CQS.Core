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

    public string Format { get; set; }

    public Dictionary<string, string> _samples;
    public Dictionary<string, string> Samples
    {
      get
      {
        if (_samples == null)
        {
          _samples = new Dictionary<string, string>();
        }
        return _samples;
      }
    }
  }

  public class AnnovarGenomeSummaryItemReader : IFileReader<List<AnnovarGenomeSummaryItem>>
  {
    public List<AnnovarGenomeSummaryItem> ReadFromFile(string fileName)
    {
      if (fileName.ToLower().EndsWith("csv"))
      {
        return ReadCsv(fileName);
      }
      else
      {
        return ReadTsv(fileName);
      }
    }

    private List<AnnovarGenomeSummaryItem> ReadTsv(string fileName)
    {
      var result = new List<AnnovarGenomeSummaryItem>();

      var lines = File.ReadAllLines(fileName).Where(m => !m.StartsWith("#")).ToArray();

      string[] headers = lines.First().Split('\t');
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

      var formatIndex = Array.IndexOf(headers, "FORMAT");
      string[] samples = null;
      if (formatIndex > 0)
      {
        samples = headers.Skip(formatIndex + 1).ToArray();
      }

      var chrIndex = Array.IndexOf(headers, "Chr");
      var startIndex = Array.IndexOf(headers, "Start");
      var endIndex = Array.IndexOf(headers, "End");
      for (int i = 1; i < lines.Length; i++)
      {
        var item = new AnnovarGenomeSummaryItem();
        var parts = lines[i].Split('\t');
        item.Func = parts[funcIndex];
        item.GeneString = parts[geneIndex];
        item.ExonicFunc = parts[exonicFuncIndex];
        item.Seqname = parts[chrIndex];
        item.Start = long.Parse(parts[startIndex]);
        item.End = long.Parse(parts[endIndex]);

        if (formatIndex > 0)
        {
          item.Format = parts[formatIndex];
          for (int j = formatIndex + 1; j < parts.Length; j++)
          {
            item.Samples[samples[j - formatIndex - 1]] = parts[j];
          }
        }

        result.Add(item);
      }
      return result;
    }

    private List<AnnovarGenomeSummaryItem> ReadCsv(string fileName)
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

        var formatIndex = Array.IndexOf(headers, "FORMAT");
        string[] samples = null;
        if (formatIndex > 0)
        {
          samples = headers.Skip(formatIndex).ToArray();
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

          if (formatIndex > 0)
          {
            item.Format = csv[formatIndex];
            for (int j = formatIndex + 1; j < csv.FieldCount; j++)
            {
              item.Samples[samples[j - formatIndex - 1]] = csv[j];
            }
          }

          result.Add(item);
        }
      }
      return result;
    }
  }

}
