using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace CQS.BreastCancer
{
  public static class SampleUtils
  {
    public static string[] GetDatasets(string root)
    {
      var subdirs = Directory.GetDirectories(root);
      Array.Sort(subdirs, delegate(string name1, string name2)
      {
        var n1 = new FileInfo(name1).Name;
        var n2 = new FileInfo(name2).Name;
        if (n1.StartsWith("GSE") && n2.StartsWith("GSE"))
        {
          return int.Parse(n1.Substring(3)).CompareTo(int.Parse(n2.Substring(3)));
        }
        else
        {
          return name1.CompareTo(name2);
        }
      });
      return subdirs;
    }
  }
}
