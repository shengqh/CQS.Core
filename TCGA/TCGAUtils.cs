using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using RCPA;
using RCPA.Utils;
using CQS.TCGA.Microarray;

namespace CQS.TCGA
{
  public enum TCGASampleType { Tumor, Normal, Control, Other };

  public enum TCGATechnologyType { Affy, Agilent, RPKM, RSEM };

  public enum TCGADataType { Normalized, Count };

  public static class TCGAUtils
  {
    private static Regex reg = new Regex("(TCGA-[^-]+-[^-]+-[^-]+)");
    private static Regex sampleTypeReg = new Regex(@"TCGA-[^-]+-[^-]+-(\d+)");

    public static string GetSampleBarCode(string otherBarCode)
    {
      var m = reg.Match(otherBarCode);
      if (!m.Success)
      {
        throw new Exception(string.Format("Cannot find TCGA sample bar code pattern from {0}.", otherBarCode));
      }
      return m.Groups[1].Value;
    }

    public static TCGASampleType GetSampleType(string barCode)
    {
      var m = sampleTypeReg.Match(barCode);
      if (!m.Success)
      {
        throw new Exception(string.Format("It is not a valid TCGA sample bar code: {0}.", barCode));
      }
      var code = int.Parse(m.Groups[1].Value);

      if (code > 0 && code < 10)
      {
        return TCGASampleType.Tumor;
      }

      if (code > 9 && code < 20)
      {
        return TCGASampleType.Normal;
      }

      if (code > 19 && code < 30)
      {
        return TCGASampleType.Control;
      }

      throw new Exception(string.Format("It is not a valid TCGA sample code [should be 1-29]: {0} from {1}.", code, barCode));
    }

    public static bool IsNormalSample(string barCode)
    {
      return GetSampleType(barCode) == TCGASampleType.Normal;
    }

    public static bool IsTumorSample(string barCode)
    {
      return GetSampleType(barCode) == TCGASampleType.Tumor;
    }

    public static bool IsControlSample(string barCode)
    {
      return GetSampleType(barCode) == TCGASampleType.Control;
    }

    public static bool IsLevel1(string directoryName)
    {
      return directoryName.ToLower().Contains("level_1");
    }

    public static bool IsLevel2(string directoryName)
    {
      return directoryName.ToLower().Contains("level_2");
    }

    public static bool IsLevel3(string directoryName)
    {
      return directoryName.ToLower().Contains("level_3");
    }

    public static void AddRnaSeqV2DataSet(string tumordir, List<DatasetInfo> datasets)
    {
      var ds = GetRnaSeqV2DataSet(tumordir);
      if (ds != null)
      {
        datasets.Add(ds);
      }
    }

    public static void AddRnaSeqV1DataSet(string tumordir, List<DatasetInfo> datasets)
    {
      var ds = GetRnaSeqV1DataSet(tumordir);
      if (ds != null)
      {
        datasets.Add(ds);
      }
    }

    public static readonly string[] DefaultV2Pattern = { "*.genes.normalized_results", "*.rsem.genes.results" };
    public static readonly string[] DefaultV1Pattern = { "*.gene.quantification.txt" };

    public static Dictionary<string, List<BarInfo>> GetRnaseqV2Files(string aliquotFile, string dir, bool paired, string[] filePattern = null, bool allFilePattern = false)
    {
      if (!Directory.Exists(dir))
      {
        return new Dictionary<string, List<BarInfo>>();
      }

      var finder = TCGATechnology.RNAseq_RSEM.GetFinder(null, dir);
      if (filePattern == null)
      {
        filePattern = DefaultV2Pattern;
      }

      if (allFilePattern)
      {
        return GetAllSampleMap(dir, filePattern, finder);
      }
      else
      {
        return GetSampleMap(dir, filePattern, finder, paired);
      }
    }

    public static Dictionary<string, List<BarInfo>> GetRnaseqV1Files(string dir, bool paired, string[] filePattern = null, bool allFilePattern = false)
    {
      if (!Directory.Exists(dir))
      {
        return new Dictionary<string, List<BarInfo>>();
      }

      var reader = TCGATechnology.RNAseq_RPKM.GetReader();
      var finder = TCGATechnology.RNAseq_RSEM.GetFinder(null, dir);
      if (filePattern == null)
      {
        filePattern = DefaultV1Pattern;
      }

      if (allFilePattern)
      {
        return GetAllSampleMap(dir, filePattern, finder);
      }
      else
      {
        return GetSampleMap(dir, filePattern, finder, paired);
      }
    }

    public static Dictionary<string, List<BarInfo>> GetMicroarrayFiles(string dir, bool paired)
    {
      if (!Directory.Exists(dir))
      {
        return new Dictionary<string, List<BarInfo>>();
      }

      var finder = TCGATechnology.Microarray.GetFinder(null, dir);
      var filePattern = "*level3.data.txt";
      return GetSampleMap(dir, new string[] { filePattern }, finder, paired);
    }

    public static Dictionary<string, List<BarInfo>> GetAllSampleMap(string dir, string[] filePatterns, IParticipantFinder finder)
    {
      DefaultParticipantFinder dfinder = new DefaultParticipantFinder(finder, string.Empty);

      var files = (from filepattern in filePatterns
                   from subdir in Directory.GetDirectories(dir)
                   from file in Directory.GetFiles(subdir, filepattern)
                   where !File.Exists(file + ".bad")
                   select file).GroupBy(m => Path.GetFileName(m)).ToList().ConvertAll(m => m.Last());

      var plist = (from file in files
                   let barcode = dfinder.FindParticipant(Path.GetFileName(file))
                   where barcode != string.Empty
                   orderby barcode
                   select new BarInfo(barcode, file)).ToList();

      return plist.GroupBy(m => m.BarCode).ToDictionary(m => m.Key, m => m.ToList());
    }

    public static Dictionary<string, List<BarInfo>> GetSampleMap(string dir, string[] filePatterns, IParticipantFinder finder, bool paired)
    {
      DefaultParticipantFinder dfinder = new DefaultParticipantFinder(finder, string.Empty);

      var files = new HashSet<string>();

      foreach (var filepattern in filePatterns)
      {
        if (files.Count > 0)
        {
          break;
        }

        //some file will be duplicated in multiple directory, so we need to keep only one such file
        files = new HashSet<string>((from subdir in Directory.GetDirectories(dir)
                                     from file in Directory.GetFiles(subdir, filepattern)
                                     where !File.Exists(file + ".bad")
                                     select file).GroupBy(m => Path.GetFileName(m)).ToList().ConvertAll(m => m.Last()));
      }

      var plist = (from file in files
                   let barcode = dfinder.FindParticipant(Path.GetFileName(file))
                   where barcode != string.Empty
                   orderby barcode
                   select new BarInfo(barcode, file)).ToList();

      if (paired)
      {
        var pgroup = plist.GroupBy(m => m.Paticipant).ToList();

        var result = new Dictionary<string, List<BarInfo>>();
        foreach (var m in pgroup)
        {
          if (m.Count() == 1)
          {
            continue;
          }

          if ((from mm in m select mm.BarCode).Distinct().Count() == 1)
          {
            continue;
          }

          var ml = m.ToList();
          for (int i = 0; i < ml.Count; i++)
          {
            for (int j = i + 1; j < ml.Count; j++)
            {
              if (ml[j].Sample - ml[i].Sample == 10)
              {
                result[m.Key] = new List<BarInfo>(new[] { ml[i], ml[j] });
                break;
              }
            }
          }
        }
        return result;
      }
      else
      {
        return plist.GroupBy(m => m.BarCode).ToDictionary(m => m.Key, m => m.ToList());
      }
    }

    public static List<string> GetCommonGenes(List<DatasetInfo> datasets)
    {
      var geneKeys = new List<string>();
      foreach (var ds in datasets)
      {
        var genes = ds.Reader.ReadFromFile(ds.BarInfoListMap.First().Value[0].FileName).Values.ConvertAll(m => m.Name);
        if (ds == datasets[0])
        {
          geneKeys = genes;
        }
        else
        {
          geneKeys = geneKeys.Intersect(genes).ToList();
        }
      }
      return geneKeys;
    }

    public static List<string> GetCommonSamples(List<DatasetInfo> datasets)
    {
      HashSet<string> result = new HashSet<string>();
      foreach (var ds in datasets)
      {
        if (result.Count == 0)
        {
          result.UnionWith(ds.GetBarCodes());
        }
        else
        {
          result.IntersectWith(ds.GetBarCodes());
        }
      }
      return result.ToList();
    }

    public static List<double?> GetValueList<T>(IFileReader<T> reader, HashSet<string> genes, BarInfo barInfo) where T : ExpressionData
    {
      var mTumor = reader.ReadFromFile(barInfo.FileName);
      mTumor.Values.RemoveAll(m => !genes.Contains(m.Name));
      var mValues = mTumor.Values.ToDictionary(m => m.Name, m => m.Value);

      List<double?> result = new List<double?>();
      foreach (var gene in genes)
      {
        if (mValues.ContainsKey(gene))
        {
          result.Add(mValues[gene]);
        }
        else
        {
          result.Add(null);
        }
      }
      return result;
    }

    private static List<double?> GetRatioList<T>(IFileReader<T> reader, HashSet<string> genes, List<BarInfo> pair) where T : ExpressionData
    {
      var mTumor = reader.ReadFromFile(pair[0].FileName);
      var mNormal = reader.ReadFromFile(pair[1].FileName);
      mTumor.Values.RemoveAll(m => !genes.Contains(m.Name));
      mNormal.Values.RemoveAll(m => !genes.Contains(m.Name));
      var tValues = mTumor.Values.ToDictionary(m => m.Name, m => m.Value);
      var nValues = mNormal.Values.ToDictionary(m => m.Name, m => m.Value);

      List<double?> result = new List<double?>();
      foreach (var gene in genes)
      {
        if (nValues[gene] == 0)
        {
          result.Add(null);
        }
        else
        {
          if (mTumor.IsLog2Value)
          {
            result.Add(Math.Pow(2, tValues[gene]) / Math.Pow(2, nValues[gene]));
          }
          else
          {
            result.Add(tValues[gene] / nValues[gene]);
          }
        }
      }
      return result;
    }

    public static double CalculateSpearmanCorrelation(HashSet<string> genes, string barCode, DatasetInfo ds1, DatasetInfo ds2)
    {
      try
      {
        var dsData1 = GetValueList(ds1.Reader, genes, ds1.BarInfoListMap[barCode].First());
        var dsData2 = GetValueList(ds2.Reader, genes, ds2.BarInfoListMap[barCode].First());

        return CalculateSpearmanCorrelation(dsData1, dsData2);
      }
      catch (Exception ex)
      {
        Console.WriteLine(ex.Message);
        throw;
      }
    }

    public static double CalculateSpearmanCorrelation(List<double?> dsData1, List<double?> dsData2)
    {
      for (int k = dsData1.Count - 1; k >= 0; k--)
      {
        if (!dsData1[k].HasValue || !dsData2[k].HasValue || double.IsNaN(dsData1[k].Value) || double.IsNaN(dsData2[k].Value))
        {
          dsData1.RemoveAt(k);
          dsData2.RemoveAt(k);
        }
        else if (dsData1[k] == 0 && dsData2[k] == 0)
        {
          dsData1.RemoveAt(k);
          dsData2.RemoveAt(k);
        }
      }
      return StatisticsUtils.SpearmanCorrelation(dsData1, dsData2);
    }

    public static string GetMicroarrayName(string dirName)
    {
      var name = Path.GetFileName(dirName);
      if (name.Contains("hg"))
      {
        return name.Substring(name.IndexOf("hg") + 3).ToUpper();
      }
      else
      {
        return "Agil" + name.Substring(name.Length - 1);
      }
    }

    public static List<DatasetInfo> GetMicroarrayDatasets(string tumordir)
    {
      List<DatasetInfo> result = new List<DatasetInfo>();
      var dir = tumordir + @"\data\transcriptome";
      if (!Directory.Exists(dir))
      {
        return result;
      }

      var mdirs = Directory.GetDirectories(dir);
      foreach (var mdir in mdirs)
      {
        result.Add(new DatasetInfo()
        {
          Name = TCGAUtils.GetMicroarrayName(mdir),
          BarInfoListMap = TCGAUtils.GetMicroarrayFiles(mdir, false),
          Reader = new Level3MicroarrayDataTxtReader()
        });
      }

      return result;
    }

    public static DatasetInfo GetRnaSeqV2DataSet(string tumordir, string[] filepattern = null, bool allFilePattern = false)
    {
      var rnaseq2dir = tumordir + @"\data\rnaseqv2";
      if (Directory.Exists(rnaseq2dir))
      {
        var aliquotFile = Directory.GetFiles(tumordir + @"\data\clin\", "*_aliquot_*.txt").First();
        return new DatasetInfo()
        {
          Name = "RSeqV2",
          BarInfoListMap = GetRnaseqV2Files(aliquotFile, rnaseq2dir, false, filepattern, allFilePattern),
          Reader = TCGATechnology.RNAseq_RSEM.GetReader()
        };
      }
      return null;
    }

    public static DatasetInfo GetRnaSeqV1DataSet(string tumordir, string[] filepattern = null, bool allFilePattern = false)
    {
      var rnaseq1dir = tumordir + @"\data\rnaseq";
      if (Directory.Exists(rnaseq1dir))
      {
        return new DatasetInfo()
        {
          Name = "RSeqV1",
          BarInfoListMap = GetRnaseqV1Files(rnaseq1dir, false, filepattern, allFilePattern),
          Reader = TCGATechnology.RNAseq_RPKM.GetReader()
        };
      }
      return null;
    }

    public static Dictionary<string, Dictionary<TCGATechnologyType, Dictionary<TCGASampleType, List<BarInfo>>>> GetSampleMap(string root)
    {
      var result = new Dictionary<string, Dictionary<TCGATechnologyType, Dictionary<TCGASampleType, List<BarInfo>>>>();
      foreach (var tumordir in Directory.GetDirectories(root))
      {
        var dirname = Path.GetFileName(tumordir);

        result[dirname] = GetTumorSampleMap(tumordir);
      }
      return result;
    }

    public static Dictionary<TCGATechnologyType, Dictionary<TCGASampleType, List<BarInfo>>> GetTumorSampleMap(string tumordir)
    {
      var result = new Dictionary<TCGATechnologyType, Dictionary<TCGASampleType, List<BarInfo>>>();

      List<DatasetInfo> microarray = TCGAUtils.GetMicroarrayDatasets(tumordir);
      var affy = microarray.Find(m => m.Name.Equals("U133A"));
      var agilent = microarray.Find(m => m.Name.Equals("Agil3"));
      if (agilent == null)
      {
        agilent = microarray.Find(m => m.Name.Equals("Agil2"));
      }

      var rnaseqv1 = TCGAUtils.GetRnaSeqV1DataSet(tumordir);
      var rnaseqv2 = TCGAUtils.GetRnaSeqV2DataSet(tumordir);

      DatasetInfo[] dis = new[] { affy, agilent, rnaseqv1, rnaseqv2 };
      var technologies = EnumUtils.EnumToArray<TCGATechnologyType>();
      for (int i = 0; i < dis.Length; i++)
      {
        AddDataset(result, technologies[i], dis[i]);
      }
      return result;
    }

    private static void AddDataset(Dictionary<TCGATechnologyType, Dictionary<TCGASampleType, List<BarInfo>>> tumormap, TCGATechnologyType technolyType, DatasetInfo datasetInfo)
    {
      var map = new Dictionary<TCGASampleType, List<BarInfo>>();
      tumormap[technolyType] = map;
      foreach (var type in EnumUtils.EnumToArray<TCGASampleType>())
      {
        map[type] = new List<BarInfo>();
      }

      if (datasetInfo == null)
      {
        return;
      }

      foreach (var key in datasetInfo.BarInfoListMap)
      {
        var type = TCGAUtils.GetSampleType(key.Key);
        map[type].Add(key.Value.First());
      }
    }

    public static int GetSampleCount(this Dictionary<TCGASampleType, List<BarInfo>> map)
    {
      return map.Sum(m => m.Value.Count);
    }

    public static bool HasSample(this Dictionary<TCGATechnologyType, Dictionary<TCGASampleType, List<BarInfo>>> tumormap, TCGATechnologyType tType)
    {
      return tumormap[tType].GetSampleCount() > 0;
    }

    public static bool HasSample(this Dictionary<TCGATechnologyType, Dictionary<TCGASampleType, List<BarInfo>>> tumormap, TCGASampleType sType)
    {
      return tumormap.Sum(m => m.Value[sType].Count) > 0;
    }

    public static HashSet<string> GetBarCodes(this Dictionary<TCGASampleType, List<BarInfo>> map)
    {
      return new HashSet<string>((from m in map.Values
                                  from b in m
                                  select b.BarCode));
    }

    public static void KeepCommonSample(this Dictionary<TCGATechnologyType, Dictionary<TCGASampleType, List<BarInfo>>> tumormap)
    {
      //get common samples
      HashSet<string> commonsamples = null;
      foreach (var m in tumormap.Values)
      {
        if (commonsamples == null)
        {
          commonsamples = m.GetBarCodes();
        }
        else
        {
          commonsamples.IntersectWith(m.GetBarCodes());
        }
      }

      //keep common samples only
      foreach (var o in tumormap.Values)
      {
        foreach (var l in o.Values)
        {
          l.RemoveAll(m => !commonsamples.Contains(m.BarCode));
        }
      }
    }

    public static HashSet<Pair<string, string>> GetPairedBarCodes(this Dictionary<TCGASampleType, List<BarInfo>> map)
    {
      var codes = new HashSet<int>(new int[] { 1, 11 });
      return GetPairedBarCodes(map, codes);
    }

    public static HashSet<Pair<string, string>> GetPairedBarCodes(this Dictionary<TCGASampleType, List<BarInfo>> map, HashSet<int> sampleCodes)
    {
      var result = new HashSet<Pair<string, string>>();

      var barcodes = (from v in map.Values
                      from b in v
                      where sampleCodes.Contains(b.Sample)
                      select b).GroupBy(m => m.Paticipant, m => m.BarCode).ToList();
      barcodes.RemoveAll(m => m.Count() == 1);

      foreach (var b in barcodes)
      {
        if (b.Count() > 2)
        {
          Console.WriteLine(b.Key);
          foreach (var code in b)
          {
            Console.WriteLine("  " + code);
          }
        }
        else
        {
          result.Add(new Pair<string, string>(b.First(), b.Last()));
        }
      }
      return result;
    }

    public static void KeepPairedSample(this Dictionary<TCGATechnologyType, Dictionary<TCGASampleType, List<BarInfo>>> tumormap)
    {
      HashSet<Pair<string, string>> paired = null;

      foreach (var m in tumormap.Values)
      {
        if (paired == null)
        {
          paired = m.GetPairedBarCodes();
        }
        else
        {
          paired.IntersectWith(m.GetPairedBarCodes());
        }
      }

      var commonsamples = new HashSet<string>();
      foreach (var p in paired)
      {
        commonsamples.Add(p.First);
        commonsamples.Add(p.Second);
      }

      //keep common samples only
      foreach (var o in tumormap.Values)
      {
        foreach (var l in o.Values)
        {
          l.RemoveAll(m => !commonsamples.Contains(m.BarCode));
        }
      }
    }

    public static Dictionary<TCGATechnologyType, Dictionary<TCGASampleType, List<BarInfo>>> CloneMap(this Dictionary<TCGATechnologyType, Dictionary<TCGASampleType, List<BarInfo>>> tumormap)
    {
      var result = new Dictionary<TCGATechnologyType, Dictionary<TCGASampleType, List<BarInfo>>>();
      foreach (var tec in tumormap)
      {
        result[tec.Key] = new Dictionary<TCGASampleType, List<BarInfo>>();
        foreach (var stype in tec.Value)
        {
          result[tec.Key][stype.Key] = new List<BarInfo>(stype.Value);
        }
      }
      return result;
    }

    public static Dictionary<string, Dictionary<ITCGATechnology, Dictionary<string, BarInfo>>> GetTumorTechnologyBarcodeFileMap(ITCGATechnology[] tecs, string tcgaRootDir, TCGASampleCode[] sampleTypes = null, bool commonSampleOnly = true)
    {
      Func<string, bool> acceptBarcode = null;
      HashSet<int> sampleCodes = null;
      if (sampleTypes != null)
      {
        sampleCodes = new HashSet<int>(sampleTypes.ToList().ConvertAll(m => m.Code));
        acceptBarcode = m => sampleCodes.Contains(new BarInfo(m, null).Sample);
      }

      //tumor=>technology=>barcode=>file
      var result = new Dictionary<string, Dictionary<ITCGATechnology, Dictionary<string, BarInfo>>>();

      foreach (var dir in Directory.GetDirectories(tcgaRootDir))
      {
        //must contains all technologies 
        if (!tecs.All(m =>
        {
          return Directory.Exists(m.GetTechnologyDirectory(dir));
        }))
        {
          continue;
        }

        var datasets = (from tec in tecs
                        select new { Tec = tec, Dataset = tec.GetDataset(dir, null) }).ToList();

        if (acceptBarcode != null)
        {
          foreach (var ds in datasets)
          {
            var barcodes = ds.Dataset.GetBarCodes();
            foreach (var barcode in barcodes)
            {
              if (!acceptBarcode(barcode))
              {
                ds.Dataset.BarInfoListMap.Remove(barcode);
              }
            }
          }
        }

        if (commonSampleOnly && datasets.Count > 1)
        {
          //keep common samples only
          var commonsamples = (from ds in datasets
                               select ds.Dataset).GetCommonSamples();
          if (commonsamples.Count == 0)
          {
            continue;
          }

          datasets.ForEach(m => m.Dataset.KeepSamples(commonsamples));
        }

        result[Path.GetFileName(dir)] = datasets.ToDictionary(m => m.Tec, m => m.Dataset.BarInfoListMap.ToDictionary(n => n.Key, n => n.Value.First()));
      }
      return result;
    }
  }

  public static class TCGATechnolyTypeExtention
  {
    public static ITCGATechnology GetTechnology(this TCGATechnologyType ttype)
    {
      switch (ttype)
      {
        case TCGATechnologyType.Affy:
        case TCGATechnologyType.Agilent:
          return TCGATechnology.Microarray;
        case TCGATechnologyType.RPKM:
          return TCGATechnology.RNAseq_RPKM;
        case TCGATechnologyType.RSEM:
          return TCGATechnology.RNAseq_RSEM;
        default:
          throw new Exception("Unknown type " + ttype);
      }
    }
  }

  public static class TCGADataTypeExtension
  {
    public static string GetSuffix(this TCGADataType dt)
    {
      if (dt == TCGADataType.Count)
      {
        return "_Count";
      }
      else
      {
        return "";
      }
    }
  }
}
