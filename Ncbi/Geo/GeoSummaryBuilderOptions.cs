using System;
using System.Collections.Generic;
using System.Linq;

namespace CQS.Ncbi.Geo
{
  public class GeoSummaryBuilderOptions
  {
    public string GeoMetaDatabase { get; set; }

    public string OutputFile { get; set; }

    /// <summary>
    /// Default is zero
    /// </summary>
    public double MininumGsmPerGse { get; set; }

    /// <summary>
    /// Default is "homo sapiens"
    /// </summary>
    public string[] GPLOrganism { get; set; }

    /// <summary>
    /// Default is "Affymetrix"
    /// </summary>
    public string[] GPLManufacturer { get; set; }

    /// <summary>
    /// Default is "in situ oligonucleotide"
    /// </summary>
    public string[] GPLTechnology { get; set; }

    /// <summary>
    /// Default is GPL96, GPL570, GPL571
    /// </summary>
    public string[] GPLPlatform { get; set; }

    /// <summary>
    /// Default is "RNA"
    /// </summary>
    public string[] GSMType { get; set; }

    /// <summary>
    /// Keywords used to search on four GSE fields - gse.title,gse.summary,gse.type, gse.overall_design
    /// </summary>
    public string[] Keywords { get; set; }

    public GeoSummaryBuilderOptions()
    {
      MininumGsmPerGse = 0;
      GPLOrganism = new[] { "homo sapiens" };
      GPLManufacturer = new[] { "Affymetrix" };
      GPLTechnology = new[] { "in situ oligonucleotide" };
      GPLPlatform = new[] { "GPL96", "GPL570", "GPL571" };
      GSMType = new[] { "RNA" };
      Keywords = new string[] { };
    }

    public string GetSql(string field, string[] values)
    {
      return (from g in values select string.Format("{0} like '%{1}%'", field, g)).Merge(" or ");
    }

    private string GetKeywordSql()
    {
      if (Keywords.Length == 0)
      {
        return string.Empty;
      }

      var sqls = new List<string>();

      sqls.Add(GetSql("gse.title", this.Keywords));
      sqls.Add(GetSql("gse.summary", this.Keywords));
      sqls.Add(GetSql("gse.type", this.Keywords));
      sqls.Add(GetSql("gse.overall_design", this.Keywords));

      return sqls.Merge(" or ");
    }

    public string GetFilterSql()
    {
      var sqls = new List<string>();

      sqls.Add(GetSql("gpl.organism", this.GPLOrganism));
      sqls.Add(GetSql("gpl.manufacturer", this.GPLManufacturer));
      sqls.Add(GetSql("gpl.technology", this.GPLTechnology));
      sqls.Add(GetSql("gpl.gpl", this.GPLPlatform));
      sqls.Add(GetSql("gsm.type", this.GSMType));
      sqls.Add(GetKeywordSql());

      var result = (from r in sqls where !string.IsNullOrEmpty(r) select "(" + r + ")").Merge("\n  and ");
      if (!string.IsNullOrEmpty(result))
      {
        result = "where " + result;
      }

      return result;
    }
  }
}
