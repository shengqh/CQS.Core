﻿using System;
using System.IO;
using System.Linq;

namespace CQS.Genome.SomaticMutation
{
  public class ValidationItem
  {
    public string Chr { get; set; }
    public int Pos { get; set; }
    public string Line { get; set; }
  }

  public class ValidationFile
  {
    public string[] Comments { get; set; }
    public string Header { get; set; }
    public ValidationItem[] Items { get; set; }

    public ValidationFile ReadFromFile(string fileName)
    {
      var allLines = File.ReadAllLines(fileName);
      if (fileName.ToLower().EndsWith(".vcf"))
      {
        this.Comments = allLines.TakeWhile(m => m.StartsWith("##")).ToArray();
        var lines = allLines.SkipWhile(m => m.StartsWith("##")).ToArray();
        this.Header = lines[0];
        this.Items = (from line in lines.Skip(1)
                      where !string.IsNullOrWhiteSpace(line)
                      let parts = line.Split('\t')
                      let chr = parts[0]
                      let pos = parts[1]
                      select new ValidationItem() { Chr = chr, Pos = int.Parse(pos), Line = line }).ToArray();
      }
      else //assume it's bed format
      {
        this.Comments = allLines.TakeWhile(m => m.StartsWith("#")).ToArray();
        var lines = allLines.SkipWhile(m => m.StartsWith("#")).ToArray();
        var firstLine = lines[0];
        var firstparts = firstLine.Split('\t');
        int firstpos;
        if (!int.TryParse(firstparts[1], out firstpos))
        {
          this.Header = firstLine;
          lines = lines.Skip(1).ToArray();
        }
        else
        {
          this.Header = new string('\t', firstparts.Length - 1);
        }
        this.Items = (from line in lines
                      where !string.IsNullOrWhiteSpace(line)
                      let parts = line.Split('\t')
                      let chr = parts[0]
                      let pos = parts[1]
                      select new ValidationItem() { Chr = chr, Pos = int.Parse(pos) + 1, Line = line }).ToArray();
      }

      return this;
    }

    public void WriteToFile(string filename, int extension)
    {
      using (var sw = new StreamWriter(filename))
      {
        this.Items.ForEach(m => sw.WriteLine("{0}\t{1}\t{2}", m.Chr, Math.Max(m.Pos - extension, 1), m.Pos + extension));
      }
    }
  }
}
