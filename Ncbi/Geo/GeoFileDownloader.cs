using CQS.Microarray;
using RCPA;
using RCPA.Utils;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;

namespace CQS.Ncbi.Geo
{
  public class GeoFileDownloader : AbstractThreadProcessor
  {
    private GeoFileDownloaderOptions options;
    private DatasetBuilder builder;

    public GeoFileDownloader(GeoFileDownloaderOptions options)
    {
      this.options = options;
      this.builder = new DatasetBuilder(options.RExecute, options.RootDirectory);
    }

    public override IEnumerable<string> Process()
    {
      var gses = File.ReadAllLines(options.GseListFile).Where(m => !string.IsNullOrWhiteSpace(m)).ToList().ConvertAll(m => m.Split(new[] { '\t', ' ' })[0]).ToArray();

      var gseInfoMap = new Dictionary<string, Dictionary<string, Dictionary<string, List<string>>>>();
      for (int gseIndex = 0; gseIndex < gses.Length; gseIndex++)
      {
        var gse = gses[gseIndex];

        Func<GsmRecord, bool> accept;
        if (!options.AcceptMap.TryGetValue(gse, out accept))
        {
          accept = m => true;
        }

        Func<Dictionary<string, List<string>>, bool> acceptDescription;
        if (!options.AcceptDescriptionMap.TryGetValue(gse, out acceptDescription))
        {
          acceptDescription = null;
        }

        var sql = string.Format(@"select gse.gse, gsm.supplementary_file, gsm.title, gsm.gpl, gsm.source_name_ch1
from gse 
	JOIN gse_gsm ON gse.gse=gse_gsm.gse   
	JOIN gsm ON gse_gsm.gsm=gsm.gsm
where 
  gse.gse == '{0}'
  and (gsm.supplementary_file like '%CEL.gz%' or gsm.supplementary_file like '%cel.gz%')
group by gse.gse, gsm.supplementary_file
", gse);

        SQLiteDBHelper sqlite = new SQLiteDBHelper(options.GeoMetaDatabase);

        Progress.SetMessage("{0}/{1}: querying files ...", gseIndex + 1, gses.Length);
        var data = sqlite.ExecuteDataTable(sql, null);

        var records = new List<GsmRecord>();
        for (int row = 0; row < data.Rows.Count; row++)
        {
          var rec = new GsmRecord()
          {
            GSE = data.Rows[row].ItemArray[0].ToString(),
            Url = data.Rows[row].ItemArray[1].ToString(),
            Title = data.Rows[row].ItemArray[2].ToString(),
            GPL = data.Rows[row].ItemArray[3].ToString(),
            SourceName = data.Rows[row].ItemArray[4].ToString()
          };

          records.Add(rec);
        }

        var totalValids = records.Where(m => accept(m)).Count();

        int currentValid = 0;
        foreach (var rec in records)
        {
          var file = rec.Url.Split(';').Where(m => m.Trim().ToLower().EndsWith("cel.gz")).First();
          var dataDir = builder.DataDir + "/";
          var dir = dataDir + rec.GSE;
          var gzipped = dir + "/" + Path.GetFileName(file);
          var tmp = gzipped + "tmp";

          if (!accept(rec))
          {
            DeleteGzippedFile(gzipped);
            continue;
          }

          currentValid++;

          var prefix = string.Format("{0}/{1} ~ {2} : {3}/{4}", gseIndex + 1, gses.Length, rec.GSE, currentValid, totalValids);
          Progress.SetMessage(prefix + " ~ " + file);

          if (!Directory.Exists(dataDir))
          {
            Directory.CreateDirectory(dataDir);
          }

          if (!Directory.Exists(dir))
          {
            Directory.CreateDirectory(dir);
          }

          if (acceptDescription != null)
          {
            Dictionary<string, Dictionary<string, List<string>>> curInfoMap;
            if (!gseInfoMap.TryGetValue(rec.GSE, out curInfoMap))
            {
              if (!GseSeriesMatrixReader.HasMatrixFiles(dir))
              {
                new GseMatrixDownloader(new GseMatrixDownloaderOptions()
                {
                  InputDirectory = dir
                }).Process();
              }
              if (!GseSeriesMatrixReader.HasMatrixFiles(dir))
              {
                throw new Exception("Failed to download matrix file for " + gse);
              }
              curInfoMap = new GseSeriesMatrixReader().ReadDescriptionFromDirectory(dir);
              gseInfoMap[rec.GSE] = curInfoMap;
            }

            var gsmName = Path.GetFileName(file).StringBefore(".cel.gz").StringBefore(".CEL.gz");
            var sampleInfoMap = curInfoMap[gsmName];
            if (!acceptDescription(sampleInfoMap))
            {
              DeleteGzippedFile(gzipped);
              continue;
            }
          }

          if (File.Exists(gzipped) && new FileInfo(gzipped).Length == 0)
          {
            File.Delete(gzipped);
          }

          if (!File.Exists(gzipped))
          {
            Progress.SetMessage(prefix + " ~ downloading " + file + " ...");
            if (!WebUtils.DownloadFile(file, tmp))
            {
              Console.Error.WriteLine("Download {0} failed.", file);
              break;
            }

            File.Move(tmp, gzipped);
          }
        }
      }

      var nocels = (from gse in gses
                    let dir = builder.DataDir + "/" + gse
                    where !Directory.Exists(dir)
                    select gse).Merge("\n");

      if (!string.IsNullOrEmpty(nocels))
      {
        throw new Exception("No cel file found for\n" + nocels);
      }

      return new[] { options.RootDirectory };
    }

    private static void DeleteGzippedFile(string gzipped)
    {
      if (File.Exists(gzipped))
      {
        File.Delete(gzipped);
        if (File.Exists(gzipped + ".tsv"))
        {
          File.Delete(gzipped + ".tsv");
        }
        if (File.Exists(gzipped + ".md5"))
        {
          File.Delete(gzipped + ".md5");
        }
      }
    }
  }
}
