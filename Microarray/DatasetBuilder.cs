using CQS.BreastCancer;
using CQS.Microarray.Affymatrix;
using CQS.Ncbi.Geo;
using CQS.Sample;
using Microsoft.Office.Interop.Excel;
using RCPA;
using RCPA.Gui;
using RCPA.R;
using RCPA.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;

namespace CQS.Microarray
{
  public class DatasetBuilder : ProgressClass
  {
    private string rExecute;

    public DatasetBuilder(string rExecute, string rootDir)
    {
      this.rExecute = rExecute;
      this.RootDir = rootDir;
    }

    public string RootDir { get; private set; }
    public virtual string DataDir { get { return Path.Combine(RootDir, "datasets"); } }
    public virtual string ExclusionDir { get { return Path.Combine(RootDir, "excluded"); } }
    public virtual string DuplicatedRoot { get { return Path.Combine(RootDir, "duplicated"); } }

    public static string zip7 = @"C:\Program Files\7-Zip\7z.exe";

    /// <summary>
    /// Build cel count and chip type summary file
    /// </summary>
    public void BuildChipTypeTable(string tableFileName, string summaryFileName)
    {
      if (!FileUtils.IsAbsolutePath(tableFileName))
      {
        tableFileName = Path.Combine(DataDir, tableFileName);
      }

      if (!FileUtils.IsAbsolutePath(summaryFileName))
      {
        summaryFileName = Path.Combine(DataDir, summaryFileName);
      }

      var dic = CelFile.GetChipTypes(this.rExecute, DataDir, true, tableFileName);

      using (StreamWriter sw = new StreamWriter(summaryFileName))
      {
        sw.WriteLine("Dataset\tChiptype\tSample");

        var dsMap = dic.ToGroupDictionary(m => Path.GetFileName(Path.GetDirectoryName(m.Key)));
        var subdirs = GetDatasetDirectories();

        foreach (var subdir in subdirs)
        {
          var dsName = Path.GetFileName(subdir);
          if (dsMap.ContainsKey(dsName))
          {
            var grp = dsMap[dsName].GroupBy(m => m.Value);
            var bFirst = true;
            foreach (var g in grp)
            {
              var name = bFirst ? new DirectoryInfo(subdir).Name : string.Empty;
              bFirst = false;
              sw.WriteLine("{0}\t{1}\t{2}", name, g.Key, g.Count());
            }
          }
        }
      }
    }

    /// <summary>
    /// Read sample information from GSE data directory
    /// </summary>
    /// <param name="gseDirectory"></param>
    /// <returns></returns>
    public static List<SampleItem> ReadSampleItem(string gseDirectory)
    {
      var files = Directory.GetFiles(gseDirectory, "*.txt");

      var samples = new List<SampleItem>();
      foreach (var file in files)
      {
        samples.AddRange(GsmFile.ReadSamples(file));
      }
      return samples;
    }

    /// <summary>
    /// Remove a whole dataset to exclusion directory
    /// </summary>
    /// <param name="dataset"></param>
    public void ExcludeDataset(string dataset)
    {
      var sourceDirectory = Path.Combine(DataDir, dataset);
      if (Directory.Exists(sourceDirectory))
      {
        if (!Directory.Exists(ExclusionDir))
        {
          Directory.CreateDirectory(ExclusionDir);
        }

        var targetDirectory = Path.Combine(ExclusionDir, dataset);
        if (Directory.Exists(targetDirectory))
        {
          foreach (var file in Directory.GetFiles(sourceDirectory))
          {
            ExcludeFile(file);
          }
          Directory.Delete(sourceDirectory);
        }
        else
        {
          Directory.Move(sourceDirectory, targetDirectory);
        }
      }
    }

    /// <summary>
    /// After chip type check, remove file from specified chip types
    /// </summary>
    public void FilterChipType(string chipTypeTableFileName, string[] removeChipTypes)
    {
      chipTypeTableFileName = CheckFileName(chipTypeTableFileName);

      var dic = new MapReader(0, 1).ReadFromFile(chipTypeTableFileName);

      foreach (var cel in dic)
      {
        var chiptype = cel.Value;
        if (removeChipTypes.Contains(chiptype))
        {
          var celfile = cel.Key;
          if (File.Exists(celfile))
          {
            Console.WriteLine("Removing {0} : {1}", chiptype, celfile);

            ExcludeFile(celfile);
          }
        }
      }
    }

    private void ExcludeFile(string celfile)
    {
      var dataset = Path.GetFileName(Path.GetDirectoryName(celfile));
      var targetDir = Path.Combine(ExclusionDir, dataset);
      if (!File.Exists(targetDir))
      {
        Directory.CreateDirectory(targetDir);
      }
      var targetFile = Path.Combine(targetDir, Path.GetFileName(celfile));

      DoRemoveFile(celfile, targetFile);
      DoRemoveFile(celfile + ".md5", targetFile + ".md5");
      DoRemoveFile(celfile + ".tsv", targetFile + ".tsv");
    }

    private static void DoRemoveFile(string sourceFile, string targetFile)
    {
      if (File.Exists(targetFile))
      {
        File.Delete(targetFile);
      }

      if (File.Exists(sourceFile))
      {
        File.Move(sourceFile, targetFile);
      }
    }

    private string CheckFileName(string chipTypeTableFileName)
    {
      if (!FileUtils.IsAbsolutePath(chipTypeTableFileName))
      {
        chipTypeTableFileName = Path.Combine(DataDir, chipTypeTableFileName);
      }
      return chipTypeTableFileName;
    }

    /// <summary>
    /// Filter GSM files based on accept function. The rejected files will be removed to exclusion directory.
    /// </summary>
    /// <param name="gseName"></param>
    /// <param name="accept"></param>
    public void FilterGseSource(string gseName, Func<SampleItem, bool> accept)
    {
      var gseDirectory = Path.Combine(DataDir, gseName);
      if (!Directory.Exists(gseDirectory))
      {
        return;
      }

      var gseExcluded = Path.Combine(ExclusionDir, gseName);

      var samples = ReadSampleItem(gseDirectory);

      var sg = samples.ToDictionary(m => m.Sample.ToLower());

      var celFiles = CelFile.GetCelFiles(gseDirectory);
      foreach (var cel in celFiles)
      {
        var gsm = GeoUtils.GetGsmName(cel);

        if (!sg.ContainsKey(gsm))
        {
          throw new Exception("Cannot find sample information of " + gsm + " in " + gseDirectory);
        }

        var sample = sg[gsm];
        if (!accept(sample))
        {
          if (!Directory.Exists(gseExcluded))
          {
            Directory.CreateDirectory(gseExcluded);
          }
          var target = Path.Combine(gseExcluded, Path.GetFileName(cel));
          Console.WriteLine("\t{0}\t{1}\t{2}\t{3}", Path.GetFileName(cel), sample.Sample, sample.SourceName, sample.SampleTitle);
          if (File.Exists(target))
          {
            File.Delete(target);
          }
          File.Move(cel, target);
        }
      }
    }

    /// <summary>
    /// Make sure no data duplicated
    /// </summary>
    public void CheckDuplication(string outputFile)
    {
      outputFile = CheckFileName(outputFile);

      var cels = new Dictionary<string, HashSet<string>>();

      var dirs = GetDatasetDirectories();
      foreach (var subdir in dirs)
      {
        Console.Out.WriteLine(subdir);
        var files = CelFile.GetCelFiles(subdir);
        foreach (var file in files)
        {
          try
          {
            string md5 = HashUtils.GetDecompressedMD5Hash(file);

            var name = Path.GetFileName(subdir) + ":" + Path.GetFileName(file);
            if (cels.ContainsKey(md5))
            {
              cels[md5].Add(name);
            }
            else
            {
              cels[md5] = new HashSet<string>(new string[] { name });
            }
          }
          catch (Exception)
          {
            Console.Error.WriteLine(file);
            if (Path.GetFileName(file).ToLower().StartsWith("gsm"))
            {
              var gsm = GeoUtils.GetGsmName(file).ToUpper();
              var filename = Path.GetFileNameWithoutExtension(file);

              WebClient webClient = new WebClient();
              var uri = string.Format(@"http://www.ncbi.nlm.nih.gov/geosuppl/?acc={0}&file={1}%2ECEL%2Egz", gsm, filename);
              Console.WriteLine(uri);
              webClient.DownloadFile(uri, file + ".gz");
            }

            throw;
          }
        }
      }

      var dupfile = Path.Combine(DataDir, outputFile);

      using (StreamWriter sw = new StreamWriter(dupfile))
      {
        sw.WriteLine("Duplicated entries = " + cels.Count(m => m.Value.Count > 1).ToString());
        sw.WriteLine("Duplicated cels = " + (from cel in cels
                                             where cel.Value.Count > 1
                                             select cel.Value.Count).Sum().ToString());

        var dscount = (from c in cels.Values
                       from v in c
                       select v).GroupBy(m => m.StringBefore(":")).ToDictionary(m => m.Key, m => m.Count());

        var keys = cels.Keys.OrderBy(m => m).ToList();

        var sets = (from c in cels
                    where c.Value.Count > 1
                    from v in c.Value
                    select new { MD5 = c.Key, Dataset = v.StringBefore(":"), FileName = v.StringAfter(":") });
        var grp = sets.GroupBy(m => m.Dataset).OrderByDescending(m => m.Count()).ToList();

        sw.WriteLine();
        sw.Write("md5");
        foreach (var g in grp)
        {
          sw.Write(string.Format("\t{0}({1}/{2})", g.Key, g.Count(), dscount[g.Key]));
        }
        sw.WriteLine();

        foreach (var md5 in keys)
        {
          if (cels[md5].Count > 1)
          {
            sw.Write(md5);

            foreach (var g in grp)
            {
              var m = g.FirstOrDefault(n => n.MD5.Equals(md5));
              if (m != null)
              {
                sw.Write("\t" + m.FileName);
              }
              else
              {
                sw.Write("\t");
              }
            }
            sw.WriteLine();
          }
        }
      }
    }

    /// <summary>
    /// Make sure no data duplicated
    /// </summary>
    public void FilterDatasetByMD5(string[] datasetByPriority)
    {
      var dsMap = (from dir in GetDatasetDirectories()
                   let dataset = Path.GetFileName(dir)
                   from file in CelFile.GetCelFiles(dir)
                   select new { Dataset = dataset, File = file, MD5 = HashUtils.GetDecompressedMD5Hash(file) }).ToGroupDictionary(m => m.Dataset);

      foreach (var dataset in datasetByPriority)
      {
        var fileMap = dsMap[dataset].ToDictionary(m => m.MD5);
        foreach (var ds in dsMap)
        {
          if (ds.Key.Equals(dataset))
          {
            continue;
          }

          var dsfiles = ds.Value;
          foreach (var cel in dsfiles)
          {
            if (!File.Exists(cel.File))
            {
              continue;
            }

            if (fileMap.ContainsKey(cel.MD5))
            {
              Console.WriteLine("Excluding " + cel.File);
              ExcludeFile(cel.File);
            }
          }
        }
      }
    }

    private static Regex numberRegex = new Regex(@"(.+?)(\d+)");

    public string[] GetDatasetDirectories()
    {
      var subdirs = Directory.GetDirectories(DataDir);
      Array.Sort(subdirs, delegate (string name1, string name2)
      {
        var n1 = new FileInfo(name1).Name;
        var n2 = new FileInfo(name2).Name;

        var m1 = numberRegex.Match(n1);
        var m2 = numberRegex.Match(n2);

        if (m1.Success && m2.Success)
        {
          var res = m1.Groups[1].Value.CompareTo(m2.Groups[1].Value);
          if (0 == res)
          {
            res = int.Parse(m1.Groups[2].Value).CompareTo(int.Parse(m2.Groups[2].Value));
          }
          return res;
        }
        else
        {
          return n1.CompareTo(n2);
        }
      });
      return subdirs;
    }

    public void Step_06_FormatExcel(string root)
    {
      var xlApp = new Microsoft.Office.Interop.Excel.Application();
      try
      {
        Workbook workbook = xlApp.Workbooks.Open(root + "\\Step_05_CheckCelList.xlsx");
        try
        {
          Worksheet workSheet = workbook.Worksheets[1];

          var blankCount = 0;
          for (int i = 2; i <= workSheet.Rows.Count; i++)
          {
            var position = "A" + i.ToString();
            Range range = workSheet.Range[position];
            var value = range.Value2;
            if (value == null)
            {
              blankCount++;
              if (blankCount > 3)
              {
                break;
              }
              else
              {
                continue;
              }
            }

            string txt = value.ToString();
            if (txt.StartsWith("GSE"))
            {
              var link = string.Format(@"http://www.ncbi.nlm.nih.gov/geo/query/acc.cgi?acc={0}", txt);
              workSheet.Hyperlinks.Add(range, link);
            }
            else if (txt.StartsWith("E-"))
            {
              var link = string.Format(@"http://www.ebi.ac.uk/arrayexpress/experiments/{0}", txt);
              workSheet.Hyperlinks.Add(range, link);
            }
            else
            {
              var link = string.Format(@"https://www.google.com/search?q={0}", txt);
              workSheet.Hyperlinks.Add(range, link);
            }
          }

          workbook.SaveAs(root + "\\Step_06_CheckCelList.xlsx");
        }
        finally
        {
          workbook.Close();
        }
      }
      finally
      {
        xlApp.Quit();
      }
    }

    public void Step_07_CelListFile(string root)
    {
      using (var sw = new StreamWriter(root + @"\Step_07_CelFileList.tsv"))
      {
        sw.WriteLine("File\tType\tName\tDataset");
        foreach (var dir in Directory.GetDirectories(root))
        {
          Console.WriteLine(dir);
          var cels = CelFile.GetCelFiles(dir);
          //var cels = GetCelFiles(dir).Take(3);
          foreach (var cel in cels)
          {
            var name = Path.GetFileNameWithoutExtension(cel).Replace("-", ".");
            if (char.IsNumber(name[0]))
            {
              name = "X" + name;
            }
            sw.WriteLine("{0}\t{1}\t{2}\t{3}", cel.Substring(root.Length + 1).Replace("\\", "/"), CelFile.GetChipType(cel), name, Path.GetFileName(dir));
          }
        }
      }
    }

    private class ProbeExpressionFile
    {
      public string[] Samples { get; set; }
      public List<Pair<string, string>> Values { get; set; }
    }

    //public void Step_09_BuildCommonProbeFile(string root)
    //{
    //  var celFiles = GetCelFiles(chipDir);

    //  var rmafiles = Directory.GetFiles(root, "Step_08_*_justRMA.tsv");

    //  var commonProbes = new HashSet<string>();
    //  foreach (var rmafile in rmafiles)
    //  {
    //    Console.WriteLine(rmafile);
    //    var map = new MapItemReader(0, 1).ReadFromFile(rmafile);
    //    //var curprobes = (from k in map.Keys select k.ToLower()).ToList();
    //    var curprobes = map.Keys.ToList();
    //    if (commonProbes.Count == 0)
    //    {
    //      commonProbes = new HashSet<string>(curprobes);
    //    }
    //    else
    //    {
    //      commonProbes.IntersectWith(curprobes);
    //    }
    //  }

    //  Console.WriteLine("Common probes = {0}", commonProbes.Count);
    //  var probes = (from p in commonProbes
    //                orderby p
    //                select p).ToList();

    //  var values = (from rmafile in rmafiles
    //                select ReadFile(rmafile, commonProbes)).ToList();

    //  using (var sw = new StreamWriter(root + "\\Step_09_expression_commonprobes.tsv"))
    //  {
    //    sw.WriteLine("Probe\t{0}", (from v in values
    //                                from s in v.Samples
    //                                select s).Merge("\t"));
    //    for (int i = 0; i < probes.Count; i++)
    //    {
    //      var p = probes[i];
    //      sw.Write(p);

    //      foreach (var v in values)
    //      {
    //        var vv = v.Values[i];
    //        sw.Write("\t{0}", v.Values[i].Second);
    //      }
    //      sw.WriteLine();
    //    }
    //  }

    //Assert.IsTrue(RHelper.ExtractCelData(celFiles, false), string.Format("Extrace tsv from cel failed"));

    //var tsvFiles = Directory.GetFiles(chipDir, "*.tsv");
    //var reader = new ExpressionDataRawReader(2, 1);
    //var datas = (from tsvfile in tsvFiles
    //             select reader.ReadFromFile(tsvfile)).ToList();
    //var commonGenes = datas.GetCommonGenes();

    //Console.WriteLine("Common genes = {0}", commonGenes.Count);

    ////get sample files
    //string[] files;
    //if (smallDataset)
    //{
    //  files = (from dir in Directory.GetDirectories(root)
    //           from file in Directory.GetFiles(dir, "*.tsv").Take(3)
    //           select file).ToArray();
    //}
    //else
    //{
    //  files = (from dir in Directory.GetDirectories(root)
    //           from file in Directory.GetFiles(dir, "*.tsv")
    //           select file).ToArray();
    //}

    ////get batch information
    //var list = (from tsvFile in files
    //            let dir = Path.GetDirectoryName(tsvFile)
    //            let file = FileUtils.ChangeExtension(tsvFile, "")
    //            select new BarInfo()
    //            {
    //              FileName = tsvFile,
    //              Dataset = Path.GetFileName(dir).Replace("-", ""),
    //              BarCode = "S_" + GetBarCode(file),
    //              ChipType = CelFile.GetChipType(file).Replace("-", "")
    //            }).ToList();

    //var dsmap = list.GroupBy(m => m.Dataset).ToList();
    //foreach (var ds in dsmap)
    //{
    //  var dss = ds.GroupBy(m => m.ChipType).ToList();
    //  if (dss.Count > 1)
    //  {
    //    foreach (var info in ds)
    //    {
    //      info.BatchName = info.Dataset + "_" + info.ChipType;
    //    }
    //  }
    //  else
    //  {
    //    foreach (var info in ds)
    //    {
    //      info.BatchName = info.Dataset;
    //    }
    //  }
    //}

    //var map = list.ToDictionary(m => m.FileName);

    ////output all informations
    //var batchDefinitionFile = targetFile + ".batchdefinition";

    //Console.WriteLine("Total {0} files", files.Length);

    //var icount = 0;
    //var genes = commonGenes.ToList();
    //genes.Sort();

    //using (StreamWriter sw = new StreamWriter(targetFile))
    //{
    //  using (StreamWriter rw = new StreamWriter(batchDefinitionFile))
    //  {
    //    sw.Write("GENE");
    //    genes.ForEach(m => sw.Write("\tG_" + m));
    //    sw.WriteLine();

    //    foreach (var file in files)
    //    {
    //      icount++;

    //      BarInfo bi = map[file];
    //      rw.WriteLine(bi.BatchName + "\t" + bi.BarCode + "\t" + bi.FileName);

    //      Console.WriteLine("reading {0}/{1} : {2}", icount, files.Length, file);
    //      try
    //      {
    //        var data = reader.ReadFromFile(file);

    //        data.Values.RemoveAll(m => !commonGenes.Contains(m.Name));
    //        data.Values.Sort((m1, m2) => m1.Name.CompareTo(m2.Name));

    //        Assert.AreEqual(commonGenes.Count, data.Values.Count, "Gene count should equal to common gene count : " + Path.GetFileName(file));

    //        sw.Write(bi.BarCode);
    //        data.Values.ForEach(m =>
    //        {
    //          if (double.IsNaN(m.Value))
    //          {
    //            sw.Write("\tNA");
    //          }
    //          else
    //          {
    //            sw.Write("\t{0:0.00}", Math.Pow(2, m.Value));
    //          }
    //        });
    //        sw.WriteLine();
    //      }
    //      catch (Exception ex)
    //      {
    //        Console.Error.WriteLine(file + " : " + ex.Message);
    //        throw;
    //      }
    //    }
    //  }
    //}
    //Console.WriteLine("Finished!");
    //}

    private ProbeExpressionFile ReadFile(string rmafile, HashSet<string> commonProbes)
    {
      var result = new ProbeExpressionFile();
      using (var sr = new StreamReader(rmafile))
      {
        var line = sr.ReadLine();
        result.Samples = (from sample in line.Split('\t').Skip(1) select Path.GetFileNameWithoutExtension(sample)).ToArray();
        result.Values = new List<Pair<string, string>>();

        while ((line = sr.ReadLine()) != null)
        {
          var name = line.StringBefore("\t");
          if (commonProbes.Contains(name))
          {
            result.Values.Add(new Pair<string, string>(name, line.StringAfter("\t")));
          }
        }
      }
      result.Values.Sort((m1, m2) => m1.First.CompareTo(m2.First));
      return result;
    }

    private string GetBarCode(string file)
    {
      var filename = Path.GetFileNameWithoutExtension(file);
      var pos = filename.ToLower().IndexOf(".cel");
      if (pos >= 0)
      {
        return filename.Substring(0, pos);
      }
      else
      {
        return filename;
      }
    }

    public void Step_08_GenerateSampleInformation(string root)
    {
      new BreastCancerSampleInformationBuilder().Process(root);
    }

    class BarInfo
    {
      public string FileName { get; set; }
      public string Dataset { get; set; }
      public string BarCode { get; set; }
      public string ChipType { get; set; }
      public string BatchName { get; set; }
    }

    private string GetAnswer(string s)
    {
      return s.Substring(s.IndexOf(":") + 1).Trim();
    }

    private string FindValue(Annotation a, Dictionary<string, HashSet<string>> namekeys, string p)
    {
      var keys = namekeys[p];
      foreach (var ann in a.Annotations)
      {
        if (keys.Contains(ann.Key))
        {
          return (ann.Value as string).Trim();
        }
      }
      return "NA";
    }

    public void DownloadGseMatrixFile()
    {
      new GseMatrixDownloader(new GseMatrixDownloaderOptions()
      {
        InputDirectory = this.DataDir
      }).Process();
    }

    public string Normalization(string outputFile)
    {
      return this.Normalization(this.DataDir, outputFile);
    }

    /// <summary>
    /// Normalization cel files and return the file contains all cel file names
    /// </summary>
    /// <param name="root"></param>
    /// <param name="outputFile"></param>
    /// <returns></returns>
    public string Normalization(string root, string outputFile)
    {
      var cels = CelFile.GetCelFiles(root);

      if (cels.Count == 0)
      {
        Progress.SetMessage("No cel file found in directory " + DataDir);
        return string.Empty;
      }

      var inputFile = Path.Combine(root, "celfiles.tsv");
      using (var sw = new StreamWriter(inputFile))
      {
        foreach (var cel in cels)
        {
          sw.WriteLine(FileUtils.ToLinuxFormat(cel));
        }
      }

      var roptions = new RTemplateProcessorOptions();
      roptions.RExecute = rExecute;
      roptions.InputFile = inputFile;
      roptions.OutputFile = inputFile;
      roptions.NoResultFile = true;
      roptions.RTemplate = FileUtils.GetTemplateDir() + "\\frma.r";
      roptions.CreateNoWindow = true;
      new RTemplateProcessor(roptions)
      {
        Progress = this.Progress
      }.Process();

      CelFile.GetChipTypes(this.rExecute, root, true, outputFile);
      return outputFile;
    }

    public void CombineFiles(string celTypeFile, string outputFile, Func<string, bool> acceptFile = null)
    {
      Func<string, bool> doAccept = acceptFile == null ? m => true : acceptFile;

      var fileCdfMap = (from line in File.ReadAllLines(celTypeFile).Skip(1)
                        where !string.IsNullOrEmpty(line)
                        let parts = line.Split('\t')
                        where doAccept(parts[0])
                        select new { File = parts[0], CelType = parts[1] }).ToList();

      var cdfGroup = fileCdfMap.ToGroupDictionary(m => m.CelType);

      HashSet<string> commonProbes = null;
      using (var sw = new StreamWriter(outputFile + ".probeCount"))
      {
        sw.WriteLine("Platform\tProbeCount");
        foreach (var g in cdfGroup)
        {
          var tsvFile = g.Value.First().File + ".tsv";

          var curProbes = (from line in File.ReadAllLines(tsvFile).Skip(1)
                           select line.StringBefore("\t")).ToArray();

          sw.WriteLine("{0}\t{1}", g.Key, curProbes.Length);

          if (commonProbes == null)
          {
            commonProbes = new HashSet<string>(curProbes);
          }
          else
          {
            commonProbes.IntersectWith(curProbes);
          }
        }
        sw.WriteLine("Common\t{0}", commonProbes.Count);
      }

      Console.WriteLine("Common probes = {0}", commonProbes.Count);

      var probes = commonProbes.OrderBy(m => m).ToArray();
      var files = fileCdfMap.ConvertAll(m => m.File).ToArray();
      foreach (var file in files)
      {
        if (!File.Exists(file))
        {
          throw new FileNotFoundException(file);
        }

        if (!File.Exists(file + ".tsv"))
        {
          throw new FileNotFoundException(file + ".tsv");
        }
      }

      var temp = outputFile + ".tmp";
      using (var sw = new StreamWriter(temp))
      {
        sw.WriteLine("Sample\t" + probes.Merge("\t"));
        int index = 0;

        foreach (var file in files)
        {
          index++;
          Console.Out.WriteLine("{0}/{1} : {2}", index, files.Length, file);
          Console.Out.FlushAsync();
          var tsvFile = file + ".tsv";
          var values = (from line in File.ReadAllLines(tsvFile).Skip(1)
                        let parts = line.Split('\t')
                        where commonProbes.Contains(parts[0])
                        orderby parts[0]
                        let v = double.Parse(parts[1])
                        select string.Format("{0:0.00}", v)).ToArray();
          sw.WriteLine("{0}\t{1}", GeoUtils.GetGsmName(file), values.Merge("\t"));
        }
      }
      FileUtils.TransformFile(temp, outputFile, '\t', "Gene");
      File.Delete(temp);

      using (var sw = new StreamWriter(outputFile + ".design"))
      {
        sw.WriteLine("Sample\tDataset");
        foreach (var file in files)
        {
          sw.WriteLine("{0}\t{1}", GeoUtils.GetGsmName(file), Path.GetFileName(Path.GetDirectoryName(file)));
        }
      }
    }
  }
}
