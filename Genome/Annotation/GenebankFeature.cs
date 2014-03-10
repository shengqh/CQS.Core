using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace CQS.Genome.Annotation
{
  public class GenebankFeature
  {
    public GenebankFeature()
    {
      this.Context = new List<string>();
    }

    public string FeatureName { get; set; }

    public bool IsComplement { get; set; }

    public int Start { get; set; }

    public int End { get; set; }

    public List<string> Context { get; set; }

    private Regex reg = new Regex(@"(\d+)\.\.(\d+)");
    public string Location
    {
      get
      {
        if (this.IsComplement)
        {
          return string.Format("complement({0}..{1})", this.Start, this.End);
        }
        else
        {
          return string.Format("{0}..{1}", this.Start, this.End);
        }
      }
      set
      {
        this.IsComplement = value.Contains("complement");

        var m = reg.Match(value);
        if (m.Success)
        {
          this.Start = int.Parse(m.Groups[1].Value);
          this.End = int.Parse(m.Groups[2].Value);
        }
      }
    }

    public List<string> GetParagraph(bool emblFormat)
    {
      string prefix = emblFormat ? "FT" : "  ";

      List<string> result = new List<string>();
      result.Add(string.Format("{0}   {1}{2}{3}", prefix, FeatureName, new string(' ', 16 - FeatureName.Length), Location));
      foreach (var line in Context)
      {
        result.Add(string.Format("{0}                   {1}", prefix, line));
      }

      return result;
    }
  }
}
