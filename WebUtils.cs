using RCPA.Utils;
using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading;

namespace CQS
{
  public static class WebUtils
  {
    delegate bool DownloadFileFunc(string uri, string targetFile, IProgressCallback callback = null);
    private static DownloadFileFunc doDownload;

    static WebUtils()
    {
      if (SystemUtils.IsLinux)
      {
        ServicePointManager.ServerCertificateValidationCallback = Validator;
        doDownload = DoDownloadFileDirectly;
      }
      else
      {
        doDownload = DoDownloadFileAsync;
      }
    }

    public static bool Validator(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
    {
      return true;
    }

    public static string DownloadHtml(string uri)
    {
      WebRequest http = HttpWebRequest.Create(uri);
      HttpWebResponse response = (HttpWebResponse)http.GetResponse();
      var responseStream = response.GetResponseStream();

      StreamReader reader = new StreamReader(responseStream);
      return reader.ReadToEnd();
    }

    private static void DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
    {
      var completed = (Boolean)sender;
      completed = true;
    }

    public static bool DownloadFile(string uri, string targetFile, IProgressCallback callback = null)
    {
      return doDownload(uri, targetFile, callback);
    }

    private static bool DoDownloadFileDirectly(string uri, string targetFile, IProgressCallback callback = null)
    {
      if (!File.Exists(targetFile))
      {
        var tempFile = new FileInfo(targetFile).DirectoryName + "/" + Path.GetFileName(Path.GetTempFileName());
        var webClient = new WebClient()
        {
          Proxy = null
        };

        Console.WriteLine("        " + Path.GetFileName(uri) + " ...");
        webClient.DownloadFile(new Uri(uri), tempFile);

        if (File.Exists(tempFile))
        {
          Console.WriteLine("        " + Path.GetFileName(uri) + " ... done.");
          File.Move(tempFile, targetFile);
        }
        else
        {
          Console.WriteLine("        " + Path.GetFileName(uri) + " ... failed.");
        }
      }

      return File.Exists(targetFile);
    }

    private static bool DoDownloadFileAsync(string uri, string targetFile, IProgressCallback callback = null)
    {
      if (!File.Exists(targetFile))
      {
        var tempFile = new FileInfo(targetFile).DirectoryName + "/" + Path.GetFileName(Path.GetTempFileName());
        var webClient = new WebClient()
        {
          Proxy = null,
        };

        Console.WriteLine("        " + Path.GetFileName(uri) + " ...");
        webClient.DownloadFileAsync(new Uri(uri), tempFile);

        while (webClient.IsBusy)
        {
          if (callback != null && callback.IsCancellationPending())
          {
            webClient.CancelAsync();
            return false;
          }
          Thread.Sleep(100);
        }

        if (File.Exists(tempFile))
        {
          Console.WriteLine("        " + Path.GetFileName(uri) + " ... done.");
          File.Move(tempFile, targetFile);
        }
        else
        {
          Console.WriteLine("        " + Path.GetFileName(uri) + " ... failed.");
        }
      }

      return File.Exists(targetFile);
    }
  }
}
