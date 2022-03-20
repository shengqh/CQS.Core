using CQS.Ncbi.Geo;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace CQS.BreastCancer.parser
{
  public class GSE1561Parser : IBreastCancerSampleInfoParser2
  {
    private Regex r = new Regex(@"E(\S)P(\S)T(\S)N(\S)G(\S)");

    public void ParseDataset(string datasetDirectory, Dictionary<string, BreastCancerSampleItem> sampleMap)
    {
      var files = GeoUtils.GetGsmNameFileMap(datasetDirectory);

      var dirname = Path.GetFileName(datasetDirectory);

      //The status of ER, PR is on the sample title
      var samples = new GseSeriesMatrixReader().ReadFromDirectory(datasetDirectory);
      foreach (var a in samples)
      {
        var filename = a.Key.ToLower();
        if (files.ContainsKey(filename.ToLower()))
        {
          var title = a.Value[GsmConsts.SampleTitle];
          var m = r.Match(title.First());
          var er = m.Groups[1].Value.Equals("p") ? "pos" : "neg";
          var pr = m.Groups[2].Value.Equals("p") ? "pos" : "neg";
          var ts = m.Groups[3].Value;
          var n = m.Groups[4].Value;
          var grade = m.Groups[5].Value;

          var key = filename.ToUpper();
          if (!sampleMap.ContainsKey(key))
          {
            sampleMap[key] = new BreastCancerSampleItem(dirname, filename.ToUpper());
          }

          BreastCancerSampleItem item = sampleMap[key];

          item.ER = er;
          item.PR = pr;
          item.TumorStatus = ts;
          item.Grade = grade;
        }
      }
    }
  }
}
