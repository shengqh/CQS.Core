using CommandLine;
using RCPA.Commandline;
using RCPA.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CQS.Ncbi.Geo
{
  public class GeoFileDownloaderOptions : AbstractOptions
  {
    [Option('d', "geometa", MetaValue = "FILE", Required = true, HelpText = "GEOmetadb, download from http://gbnci.abcc.ncifcrf.gov/geo/")]
    public string GeoMetaDatabase { get; set; }

    [Option('o', "output", MetaValue = "DIRECTORY", Required = true, HelpText = "Output directory")]
    public string RootDirectory { get; set; }

    [Option('s', "gselistfile", MetaValue = "FILE", HelpText = "GSE list file, one GSE per line")]
    public string GseListFile { get; set; }

    [OptionList('g', "gsenames", MetaValue = "STRING", Separator = ',', HelpText = "GSE names")]
    public IList<string> GseNames { get; set; }

    [Option('r', "rexecute", MetaValue = "FILE", HelpText = "Location of R execute")]
    public string RExecute { get; set; }

    public Dictionary<string, Func<GsmRecord, bool>> AcceptMap { get; set; }

    public Dictionary<string, Func<Dictionary<string,List<string>>, bool>> AcceptDescriptionMap { get; set; }

    public GeoFileDownloaderOptions()
    {
      AcceptMap = new Dictionary<string, Func<GsmRecord, bool>>();
      AcceptDescriptionMap = new Dictionary<string, Func<Dictionary<string, List<string>>, bool>>();
    }

    public override bool PrepareOptions()
    {
      CheckFile("GEOmetadb", GeoMetaDatabase);

      CheckDirectory("output", RootDirectory);

      if (GseNames == null || GseNames.Count == 0)
      {
        CheckFile("Dataset List", GseListFile);
      }

      if (string.IsNullOrWhiteSpace(RExecute) && SystemUtils.IsLinux)
      {
        RExecute = "R";
      }
      else
      {
        CheckFile("R execute", RExecute);
      }

      return ParsingErrors.Count == 0;
    }

    public List<string> GetGseList()
    {
      var result = new List<string>();

      if (GseNames != null)
      {
        result.AddRange(GseNames);
      }

      if (File.Exists(GseListFile))
      {
        result.AddRange(File.ReadAllLines(GseListFile).ToList().ConvertAll(m => m.Split(new[] { '\t', ' ' })[0]));
      }

      return result;
    }
  }
}
