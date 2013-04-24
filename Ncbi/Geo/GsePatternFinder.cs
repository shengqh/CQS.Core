using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CQS.BreastCancer;
using System.Text.RegularExpressions;

namespace CQS.Ncbi.Geo
{
  public class GsePatternFinder
  {
    public Regex[] ValueRegex { get; set; }

    public string[] Keys { get; set; }

    public string Question { get; set; }

    public Action<BreastCancerSampleItem, string> SetValue { get; set; }

    public string DefaultValue { get; set; }

    private static string GetAnswer(string s)
    {
      return s.Substring(s.IndexOf(":") + 1).Trim();
    }

    private static Regex erher2 = new Regex(@"ER: (\S+), Her2Neu: (\S+),");

    public bool Parse(List<string> lst, BreastCancerSampleItem item, bool defaultReturnValue)
    {
      //First of all, using key to find value
      foreach (var s in Keys)
      {
        var l = lst.Find(m => m.StartsWith(s));
        if (l != null)
        {
          SetValue(item, l.Substring(s.Length + 1).Trim());
          return true;
        }
      }

      //If there is line matching regex, using regex to assign value.
      if (ValueRegex != null)
      {
        foreach (var reg in ValueRegex)
        {
          var mLine = lst.Find(m => reg.Match(m).Success);
          if (mLine != null)
          {
            var m = reg.Match(mLine);
            SetValue(item, m.Groups[1].Value);
            return true;
          }
        }
      }

      //Finally, find question and answer
      for (int j = 0; j < lst.Count; j++)
      {
        if (lst[j].StartsWith("Pathological Question:") && GetAnswer(lst[j]).Equals(Question))
        {
          SetValue(item, GetAnswer(lst[j + 1]));
          return true;
        }
      }

      SetValue(item, DefaultValue);
      return defaultReturnValue;
    }
  }

  public static class GsePatternFinderFactory
  {
    public static GsePatternFinder GetERFinder()
    {
      return new GsePatternFinder()
      {
        ValueRegex = new Regex[] { new Regex(@"er/pr/her2 status:(.+?)/"), new Regex(@"ER:(.+?),") },
        Keys = new string[] { "Pathological ER:", "Clinical ER:", "Retreatment ER:", "path er status:", "ER status:", "er_status:", "er status:", "er ihc:", "er ihc status:", "ER:", "er:", "er.ihc:","er_status_ihc:" },
        Question = "ER",
        DefaultValue = "NA",
        SetValue = (m, n) => m.ER = new StatusValue(n)
      };
    }

    public static GsePatternFinder GetPRFinder()
    {
      return new GsePatternFinder()
      {
        ValueRegex = new Regex[] { new Regex(@"er/pr/her2 status:.+?/(.+?)/") },
        Keys = new string[] { "Pathological PR:", "Clinical PR:", "Retreatment PR:", "path pr status:", "pr_status:", "pr ihc:", "pr ihc status:", "prihc:", "pr.ihc:", "pr_status_ihc:" },
        Question = "PR",
        DefaultValue = "NA",
        SetValue = (m, n) => m.PR = new StatusValue(n)
      };
    }

    public static GsePatternFinder GetHER2Finder()
    {
      return new GsePatternFinder()
      {
        ValueRegex = new Regex[] { new Regex(@"er/pr/her2 status:.+?/.+?/(.+)"), new Regex(@"Her2Neu:(.+?),") },
        Keys = new string[] { "Pathological HER/2 Neu:", "Clinical HER/2 Neu:", "Retreatment HER/2 Neu:", "her2 status:", "HER2:", "erbb2:", "erbb2 ihc status:","her2_status:" },
        Question = "HER/2 Neu",
        DefaultValue = "NA",
        SetValue = (m, n) => m.HER2 = new StatusValue(n)
      };
    }
  }
}
