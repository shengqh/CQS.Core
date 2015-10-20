using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RCPA;
using System.IO;
using CQS.Genome.Fastq;
using System.Text.RegularExpressions;
using System.Net;
using RCPA.Utils;

namespace CQS.Microarray
{
  public class GseMatrixDownloader : AbstractThreadProcessor
  {
    private GseMatrixDownloaderOptions options;

    private class BarFile
    {
      public string Barcode { get; set; }
      public string Filename { get; set; }
      public StreamWriter Stream { get; set; }
      public int Count { get; set; }
    }

    public GseMatrixDownloader(GseMatrixDownloaderOptions options)
    {
      this.options = options;
    }

    public override IEnumerable<string> Process()
    {
      var result = new List<string>();

      foreach (var dir in options.GseDirectories())
      {
        var dsname = Path.GetFileName(dir);
        Progress.SetMessage("Processing " + dsname + " ...");

        var diruri = @"ftp://ftp.ncbi.nlm.nih.gov/pub/geo/DATA/SeriesMatrix/" + dsname + "/";
        FtpWebRequest request = (FtpWebRequest)WebRequest.Create(diruri);
        request.Method = WebRequestMethods.Ftp.ListDirectory;

        // This example assumes the FTP site uses anonymous logon.
        request.Credentials = new NetworkCredential("anonymous", "custumer@microsoft.com");

        using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
        {
          using (Stream responseStream = response.GetResponseStream())
          {
            StreamReader reader = new StreamReader(responseStream);
            var files = (from f in reader.ReadToEnd().Split('\n')
                         let ff = f.Trim()
                         where !string.IsNullOrEmpty(ff)
                         select ff).ToArray();

            foreach (var file in files)
            {
              var localfile = Path.Combine(dir, file);

              string finalfile = localfile;
              bool isGziped = file.EndsWith(".gz");
              if (isGziped)
              {
                finalfile = FileUtils.ChangeExtension(localfile, "");
              }

              if (!File.Exists(finalfile))
              {
                if (!File.Exists(localfile))
                {
                  var fileuri = @"ftp://ftp.ncbi.nlm.nih.gov/pub/geo/DATA/SeriesMatrix/" + dsname + "/" + file;
                  WebUtils.DownloadFile(fileuri, localfile);
                }

                if (isGziped)
                {
                  ZipUtils.DecompressGzip(localfile, finalfile);
                  File.Delete(localfile);
                }

                result.Add(finalfile);
              }
            }
          }
        }
      }

      return result;
    }
  }
}
