using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RCPA;
using System.IO;

namespace CQS.TCGA.TCGATechnologyImpl
{
  public abstract class AbstractTCGATechnology : ITCGATechnology
  {
    protected virtual bool IsDataLevel(SpiderTreeNode node)
    {
      return node.Depth == 6;
    }

    protected virtual string FindSdrfFile(string platformDir)
    {
      var sdrfFile  = (from subdir in Directory.GetDirectories(platformDir)
              where Path.GetFileName(subdir).ToLower().Contains(".mage-tab.")
              from file in Directory.GetFiles(subdir, "*.sdrf.txt")
              select file).ToList();

      if (sdrfFile.Count == 0)
      {
        throw new Exception(string.Format("Cannot find sdrf file in mage-tab directory of {0}", platformDir));
      }

      return sdrfFile.Last();
    }

    public abstract string NodeName { get; }

    public abstract IFileReader<ExpressionData> GetReader();

    public abstract IParticipantFinder GetFinder(string tumorDir, string platformDir);

    public abstract Func<string, bool> GetFilenameFilter();

    public virtual bool IsData(SpiderTreeNode node)
    {
      return node.Name.ToLower().Equals(this.NodeName) && IsDataLevel(node);
    }

    public virtual IFileReader<ExpressionData> GetCountReader()
    {
      return null;
    }

    public virtual IFileReader<ExpressionData> GetReader(TCGADataType tdt)
    {
      if (tdt == TCGADataType.Normalized)
      {
        return GetReader();
      }
      else
      {
        return GetCountReader();
      }
    }

    public virtual string GetCountFilename(string filename)
    {
      return filename;
    }

    public virtual string GetTechnologyDirectory(string tumorDir)
    {
      return tumorDir + @"\data\" + this.NodeName;
    }

    public virtual DatasetInfo GetDataset(string tumordir, Func<string, bool> fileFilter)
    {
      var technologyDir = GetTechnologyDirectory(tumordir);

      if (Directory.Exists(technologyDir))
      {
        return new DatasetInfo()
        {
          BarInfoListMap = GetFiles(tumordir, fileFilter),
          Reader = this.GetReader()
        };
      }
      return null;
    }

    public virtual Dictionary<string, List<BarInfo>> GetFiles(string tumordir, Func<string, bool> fileFilter)
    {
      var technologyDir = GetTechnologyDirectory(tumordir);

      if (!Directory.Exists(technologyDir))
      {
        return new Dictionary<string, List<BarInfo>>();
      }

      if (fileFilter == null)
      {
        fileFilter = GetFilenameFilter();
      }

      return (from platformDir in Directory.GetDirectories(technologyDir)
              //get finder
              let finder = new DefaultParticipantFinder(GetFinder(tumordir, platformDir), string.Empty)
              //for each data directory
              from dataDir in Directory.GetDirectories(platformDir)
              //get files
              from file in Directory.GetFiles(dataDir)
              //filter files
              where fileFilter(Path.GetFileName(file)) && !File.Exists(file + ".bad")
              //parse barcode
              let barcode = finder.FindParticipant(Path.GetFileName(file))
              //filter barcode
              where barcode != string.Empty
              //group by filename since there may be duplicated files in different data directory
              select new BarInfo(barcode, file)).GroupBy(m => Path.GetFileName(m.FileName)).ToList().
        //keep the last one
                   ConvertAll(m => m.Last()).OrderBy(m => m.BarCode).ToList().
        //there may be multiple samples from same barcode sample
                   GroupBy(m => m.BarCode).ToDictionary(m => m.Key, m => m.ToList());
    }

    public override string ToString()
    {
      return this.NodeName;
    }
  }
}
