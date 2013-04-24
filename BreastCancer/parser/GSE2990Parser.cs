﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using CQS.TCGA;
using CQS.Ncbi.Geo;

namespace CQS.BreastCancer.parser
{
  public class GSE2990Parser : IBreastCancerSampleInfoParser2
  {
    public void ParseDataset(string datasetDirectory, Dictionary<string, BreastCancerSampleItem> sampleMap)
    {
      var files = GeoUtils.GetGsmNames(datasetDirectory);

      var dirname = Path.GetFileName(datasetDirectory);

      var map = new MapReader("geo_accn", "er").ReadFromFile(datasetDirectory + @"\GSE2990_suppl_info.txt");
      foreach (var m in map)
      {
        var f = datasetDirectory + "\\" + m.Key + ".cel";
        if (File.Exists(f))
        {
          if (!sampleMap.ContainsKey(m.Key))
          {
            sampleMap[m.Key] = new BreastCancerSampleItem(dirname, m.Key);
          }

          var item = sampleMap[m.Key];
          item.ER = m.Value;
        }
      }
    }
  }
}
