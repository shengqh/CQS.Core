using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CQS.Genome.SomaticMutation
{
  public class ValidationItem
  {
    public string Chr { get; set; }
    public string Pos { get; set; }
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
                      select new ValidationItem() { Chr = chr, Pos = pos, Line = line }).ToArray();
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
                      select new ValidationItem() { Chr = chr, Pos = pos, Line = line }).ToArray();
      }

      return this;
    }
  }
}
