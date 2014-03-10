using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;

namespace CQS.Genome.Dbsnp
{
  public class DbsnpVcfFile : AbstractHeaderFile<DbsnpItem>
  {
    private bool snpOnly;

    public DbsnpVcfFile(bool snpOnly)
    {
      this.snpOnly = snpOnly;
    }

    protected override bool AcceptItem(string line)
    {
      if (!base.AcceptItem(line))
      {
        return false;
      }

      if (this.snpOnly)
      {
        return line.Contains("VC=SNV");
      }

      return true;
    }

    protected override string FindHeader(TextReader sr)
    {
      string result;
      while ((result = sr.ReadLine()) != null)
      {
        if (result.StartsWith("##"))
        {
          continue;
        }

        if (result.StartsWith("#"))
        {
          return result.Substring(1);
        }
      }

      return null;
    }

    private Regex infoReg = new Regex("VC=([^;]+)");

    protected virtual void ParseInformation(string info, DbsnpItem item)
    {
      item.Information = info;
      var m = infoReg.Match(info);
      if (m.Success)
      {
        item.VariationClass = m.Groups[1].Value;
      }
    }

    protected override Dictionary<string, Action<string, DbsnpItem>> GetHeaderActionMap()
    {
      var result = new Dictionary<string, Action<string, DbsnpItem>>();

      result["CHROM"] = (m, n) => n.Chrom = m;
      result["POS"] = (m, n) => n.Position = long.Parse(m);
      result["ID"] = (m, n) => n.Id = m;
      result["REF"] = (m, n) => n.Reference = m;
      result["ALT"] = (m, n) => n.Alternative = m;
      result["QUAL"] = (m, n) => n.Quality = m;
      result["FILTER"] = (m, n) => n.Filter = m;
      result["INFO"] = ParseInformation;

      return result;
    }
  }
}
