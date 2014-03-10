using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using HtmlAgilityPack;
using RCPA;
using System.Xml.Linq;
using System.Text.RegularExpressions;
using RCPA.Utils;
using System.ComponentModel;
using System.Threading;

namespace CQS.TCGA
{
  public class DownloadItem
  {
    public string Url { get; set; }

    public string TargetFile { get; set; }

    public int RetryCount { get; set; }

    public bool Exists
    {
      get
      {
        return File.Exists(TargetFile);
      }
    }

    public override string ToString()
    {
      return Path.GetFileName(TargetFile);
    }
  }

  public static class TCGASpider
  {
    public static string RootUrl = @"https://tcga-data.nci.nih.gov/tcgafiles/ftp_auth/distro_ftpusers/anonymous/tumor/";

    public static SpiderTreeNode GetDirectoryTree(string name, string uri, bool recursive)
    {
      var result = new SpiderTreeNode()
      {
        Name = name,
        Uri = uri,
        Depth = 1
      };

      FillDirectoryTree(result, recursive);

      return result;
    }

    public static void FillDirectoryTree(SpiderTreeNode node, bool recursive)
    {
      node.Print(Console.Out);

      if (node.Name.Contains(".Level_") || node.Name.Contains(".mage-tab."))
      {
        return;
      }

      var content = WebUtils.DownloadHtml(node.Uri);

      //Console.WriteLine(content);

      HtmlDocument doc = new HtmlDocument();
      doc.LoadHtml(content);
      foreach (HtmlNode link in doc.DocumentNode.SelectNodes("//a[@href]"))
      {
        HtmlAttribute att = link.Attributes["href"];
        var curlink = att.Value;
        if (curlink.StartsWith("/") || !curlink.EndsWith("/") || curlink.Equals("lost+found/"))
        {
          continue;
        }

        string cururi;
        if (node.Uri.EndsWith("/"))
        {
          cururi = node.Uri + curlink;
        }
        else
        {
          cururi = node.Uri + "/" + curlink;
        }

        var curname = curlink.TrimEnd('/');
        var lastModified = link.NextSibling.InnerText.Trim();
        lastModified = lastModified.Substring(0, lastModified.Length - 1).Trim();

        node.Nodes.Add(new SpiderTreeNode()
        {
          Name = curname,
          Uri = cururi,
          LastModified = lastModified,
          Depth = node.Depth + 1
        });
      }

      if (recursive)
      {
        foreach (var subnode in node.Nodes)
        {
          FillDirectoryTree(subnode, true);
        }
      }
    }

    public static void FilterExists(List<DownloadItem> items)
    {
      items.RemoveAll(m => m.Exists);
    }

    public static List<DownloadItem> GetDownloadFiles(SpiderTreeNode node, string targetDir, Action<List<DownloadItem>> filterFile)
    {
      List<DownloadItem> result = new List<DownloadItem>();

      var content = WebUtils.DownloadHtml(node.Uri);

      HtmlDocument doc = new HtmlDocument();
      doc.LoadHtml(content);

      var links = new List<string>();

      foreach (HtmlNode link in doc.DocumentNode.SelectNodes("//a[@href]"))
      {
        HtmlAttribute att = link.Attributes["href"];
        var curlink = att.Value;
        if (curlink.EndsWith("/") || curlink.StartsWith("?"))
        {
          continue;
        }

        string cururi;
        if (node.Uri.EndsWith("/"))
        {
          cururi = node.Uri + curlink;
        }
        else
        {
          cururi = node.Uri + "/" + curlink;
        }

        var targetFile = new FileInfo(targetDir + "/" + curlink).FullName;
        result.Add(new DownloadItem()
        {
          Url = cururi,
          TargetFile = targetFile,
          RetryCount = 0
        });
      }

      if (null != filterFile)
      {
        filterFile(result);
      }

      FilterExists(result);

      return result;
    }

    public static void DownloadFiles(SpiderTreeNode node, string targetDir, Action<List<DownloadItem>> filterFile, IProgressCallback callback = null)
    {
      List<DownloadItem> items = GetDownloadFiles(node, targetDir, filterFile);

      foreach (var item in items)
      {
        if (!WebUtils.DownloadFile(item.Url, item.TargetFile, callback))
        {
          throw new Exception(string.Format("Download {0} to {1} failed!", item.Url, item.TargetFile));
        }
      }
    }
  }
}
