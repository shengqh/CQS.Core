using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RCPA;
using System.IO;
using System.Text.RegularExpressions;

namespace CQS.TCGA.TCGATechnologyImpl
{
  public class TCGATechnologyRNAseqV2 : AbstractTCGATechnologyRNAseq
  {
    public override string NodeName
    {
      get
      {
        return "rnaseqv2";
      }
    }

    private static Regex reg = new Regex(@"(.+?)(\d+)([^\d]+)\.normalized_results$");

    public override string GetCountFilename(string filename)
    {
      var fn = Path.GetFullPath(filename);
      var m = reg.Match(Path.GetFileName(fn));
      if (m.Success)
      {
        var pattern = string.Format("{0}*{1}.results", m.Groups[1].Value, m.Groups[3].Value);
        var files = Directory.GetFiles(Path.GetDirectoryName(fn), pattern);
        if (files.Length > 0)
        {
          return files[0];
        }
        else
        {
          throw new Exception("Cannot find the corresponding count data file " + filename);
        }
      }
      return base.GetCountFilename(filename);
    }

    public override string ValueName
    {
      get { return "RSEM"; }
    }

    public override Func<string, bool> GetFilenameFilter()
    {
      return m => m.ToLower().EndsWith(".genes.normalized_results");
    }

  }
}
