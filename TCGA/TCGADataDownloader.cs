using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RCPA;
using System.IO;
using RCPA.Utils;

namespace CQS.TCGA
{
  public class TCGADataDownloader : AbstractThreadProcessor
  {
    private readonly TCGADataDownloaderOptions _options;

    public TCGADataDownloader(TCGADataDownloaderOptions options)
    {
      this._options = options;
    }

    public override IEnumerable<string> Process()
    {
      var tecs = _options.Technologies.ToArray();
      if (tecs.Length == 0)
      {
        tecs = TCGATechnology.Technoligies;
      }

      var tumors = new HashSet<string>(_options.TumorTypes);

      var rootNode = new SpiderTreeNodeXmlFormat().ReadFromFile(_options.XmlFile);
      foreach (var subnode in rootNode.Nodes)
      {
        if (tumors.Count > 0 && !tumors.Contains(subnode.Name))
        {
          continue;
        }

        var nodes = subnode.FindDeepestNode(m => tecs.Any(n => n.IsData(m)));
        if (nodes.Count == 0)
        {
          continue;
        }

        Progress.SetMessage("Processing " + subnode.Name + "...");

        var targetRootDir = FileUtils.CreateDirectory(_options.OutputDirectory, subnode.Name);
        var dataDir = FileUtils.CreateDirectory(targetRootDir, "data");

        foreach (var m in nodes)
        {
          Progress.SetMessage("Processing {0}/{1}...", m.Parent.Name, m.Name);

          var mDir = FileUtils.CreateDirectory(dataDir, m.Name);
          var currDir = FileUtils.CreateDirectory(mDir, m.Parent.Name);

          DownloadLevel3Data(m, currDir);
        }

        //Download clinic data
        var cnode = subnode.FindDeepestNode(m => m.Name.Equals("clin") && m.Parent.Name.Equals("biotab") && m.Parent.Parent.Name.Equals("bcr")).First();
        var cDir = FileUtils.CreateDirectory(dataDir, cnode.Name);
        var files = Directory.GetFiles(cDir);
        if (files.Length > 0)
        {
          foreach (var file in files)
          {
            File.Delete(file);
          }
        }
        Progress.SetMessage("Processing {0}.{1} ...", subnode.Name, cnode.Name);
        DownloadClinicalData(subnode.Name, cnode, cDir, Progress);
      }

      return new[] { _options.OutputDirectory };
    }

    private void DownloadLevel3Data(SpiderTreeNode m, string currDir)
    {
      m.Nodes.RemoveAll(n => TCGAUtils.IsLevel1(n.Name) || TCGAUtils.IsLevel2(n.Name));
      m.MarkHighestVersionNodes();
      foreach (var node in m.Nodes)
      {
        if (Progress.IsCancellationPending())
        {
          throw new UserTerminatedException();
        }

        var fDir = currDir + "/" + node.Name;
        var compressed = fDir + ".tar.gz";
        var compressedMd5 = fDir + ".tar.gz.md5";

        var parentDir = Path.GetDirectoryName(currDir);
        var parentFDir = parentDir + "/" + node.Name;
        var parentFComparessed = parentFDir + ".tar.gz";
        var parentFComparessedMd5 = parentFDir + ".tar.gz.md5";

        if (Directory.Exists(parentFDir))
        {
          Directory.Move(parentFDir, fDir);
        }
        if (File.Exists(parentFComparessed))
        {
          File.Move(parentFComparessed, compressed);
        }
        if (File.Exists(parentFComparessedMd5))
        {
          File.Move(parentFComparessedMd5, compressedMd5);
        }

        if (node.IsPreviousVersion)
        {
          if (Directory.Exists(fDir))
          {
            Progress.SetMessage("Deleting previous version : " + fDir);
            Directory.GetFiles(fDir).ToList().ForEach(File.Delete);
            Directory.Delete(fDir);
          }

          if (!File.Exists(compressed)) 
            continue;

          File.Delete(compressed);
          File.Delete(compressedMd5);
        }
        else
        {
          Progress.SetMessage("Processing {0}.{1} ...", m.Name, node.Name);

          var bDownload = !File.Exists(compressed);
          var bTar = bDownload || !Directory.Exists(fDir);

          if (bDownload)
          {
            var uri = node.Uri.Substring(0, node.Uri.Length - 1) + ".tar.gz";
            if (!WebUtils.DownloadFile(uri, compressed, this.Progress))
            {
              throw new Exception(string.Format("Downloading {0} failed", uri));
            }

            if (Progress.IsCancellationPending())
            {
              throw new UserTerminatedException();
            }

            WebUtils.DownloadFile(uri + ".md5", compressedMd5);
          }

          UncompressFile(currDir, fDir, compressed, bTar);
        }
      }
    }

    public void DownloadClinicalData(string tumor, SpiderTreeNode node, string targetDir, IProgressCallback callback = null)
    {
      var gzfile = string.Format("clinical_{0}.tar.gz", tumor);
      var uri = string.Format("{0}/{1}", node.Uri, gzfile);
      var targetFile = string.Format("{0}/{1}", targetDir, gzfile);

      if (!WebUtils.DownloadFile(uri, targetFile, callback))
      {
        TCGASpider.DownloadFiles(node, targetDir, null, callback);
        return;
      }
      else
      {
        UncompressFile(targetDir, targetDir, targetFile, true);
      }
    }

    private void UncompressFile(string currDir, string targetDir, string tarGzFile, bool bTar)
    {
      if (SystemUtils.IsLinux)
      {
        if (bTar)
        {
          SystemUtils.Execute("tar", " -xzvf \"" + tarGzFile + "\"", currDir);
        }
      }
      else
      {
        if (string.IsNullOrEmpty(_options.Zip7))
        {
          throw new ArgumentException("Define 7zip location first!");
        }

        var tarFile = tarGzFile.Substring(0, tarGzFile.Length - 3);
        if (bTar)
        {
          SystemUtils.Execute(_options.Zip7, " e -y \"" + tarGzFile + "\"", currDir);
          Directory.CreateDirectory(targetDir);
          SystemUtils.Execute(_options.Zip7, " e -y \"" + tarFile + "\"", targetDir);
        }

        if (File.Exists(tarFile))
        {
          File.Delete(tarFile);
        }
      }
    }
  }
}
