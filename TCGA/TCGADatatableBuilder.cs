using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CQS.FileTemplate;
using RCPA;

namespace CQS.TCGA
{
  public class TCGADatatableBuilder : AbstractThreadProcessor
  {
    private readonly TCGADatatableBuilderOptions _options;

    public DirectoryInfo TemplateDirectory { get; set; }

    public TCGADatatableBuilder(TCGADatatableBuilderOptions options)
    {
      _options = options;
      _options.PrepareOptions();
      this.TemplateDirectory = FileUtils.GetTemplateDir();
    }

    private Dictionary<string, Dictionary<string, double>> GetData(Dictionary<string, BarInfo> barMap,
      out Func<double, double> getValue)
    {
      Func<string, string> getFilename;
      IFileReader<ExpressionData> reader = null;

      ITCGATechnology tec = _options.GetTechnology();
      if (_options.IsCount && null != tec.GetCountReader())
      {
        reader = tec.GetCountReader();
        getFilename = tec.GetCountFilename;
        getValue = Math.Round;
      }
      else
      {
        reader = tec.GetReader();
        getFilename = m => m;
        getValue = m => m;
      }

      //tumor=>barcode=>gene=>value
      var result = new Dictionary<string, Dictionary<string, double>>();

      Progress.SetRange(0, barMap.Count);
      long count = 0;
      foreach (var bm in barMap)
      {
        var data = reader.ReadFromFile(getFilename(bm.Value.FileName));
        var values = data.Values.ToDictionary(m => m.Name, m => m.Value);
        result[bm.Key] = values;
        count++;
        Progress.SetPosition(count);
      }

      return result;
    }

    public override IEnumerable<string> Process()
    {
      var result = new List<string>();

      Dictionary<string, BarInfo> barMap;

      barMap = new Dictionary<string, BarInfo>();
      foreach (var tumor in _options.TumorTypes)
      {
        var curMap = TCGAUtils.GetBarcodeFileMap(_options.TCGADirectory,
          _options.GetTechnology(), tumor, _options.Platforms, _options.GetTCGASampleCodes().ToArray());

        foreach (var v in curMap)
        {
          barMap[GetSampleKey(tumor, v.Key)] = v.Value;
        }
      }

      var headers = new List<string>();
      var clindata = new Dictionary<string, IAnnotation>();
      foreach (var tumor in _options.TumorTypes)
      {
        ReadClinData(clindata, tumor, headers);
      }
      Console.WriteLine("{0} patient clinical information readed", clindata.Count);

      List<string> noclinical = new List<string>();
      var keyvalues = barMap.ToList();
      foreach (var bm in keyvalues)
      {
        if (!clindata.ContainsKey(GetSampleKey(GetTumorType(bm.Key), bm.Value.Paticipant)))
        {
          noclinical.Add(bm.Key);

          Console.Error.WriteLine(string.Format("Cannot find clinical data for patient {0}", bm.Value.Paticipant));
          if (_options.WithClinicalInformationOnly)
          {
            barMap.Remove(bm.Key);
          }
        }
      }

      Progress.SetMessage("Reading data ...");
      Func<double, double> getValue;
      var valueMap = GetData(barMap, out getValue);

      var genes = GetCommonGenes(valueMap);
      var samples = valueMap.Keys.OrderBy(m => m).ToList();

      Progress.SetMessage("Saving data ...");

      result.Add(_options.OutputFile);
      result.Add(_options.DesignFile);
      if (_options.TumorTypes.Count > 1)
      {
        using (var sw = new StreamWriter(_options.OutputFile))
        {
          sw.WriteLine("Gene\t{0}", samples.Merge("\t"));
          foreach (var gene in genes)
          {
            sw.Write(gene);
            foreach (var sample in samples)
            {
              sw.Write("\t{0}", getValue(valueMap[sample][gene]));
            }
            sw.WriteLine();
          }
        }

        using (var sw = new StreamWriter(_options.DesignFile))
        {
          sw.Write("Sample\tBarcode\tPatient\tTumorType\tPlatform\tSampleType\tSampleTypeDescription");
          if (headers.Count > 0)
          {
            sw.WriteLine("\t{0}", headers.Merge("\t"));
          }
          else
          {
            sw.WriteLine();
          }

          foreach (var entry in barMap)
          {
            var tumor = GetTumorType(entry.Key);
            var type = TCGASampleCode.Find(entry.Value.Sample);
            sw.Write("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}", entry.Key, entry.Value.BarCode, entry.Value.Paticipant, tumor, entry.Value.Platform, type.ShortLetterCode,
              type.Definition);
            var key = GetSampleKey(tumor, entry.Value.Paticipant);
            var vdata = clindata.ContainsKey(key) ? clindata[key] : new Annotation();
            foreach (var header in headers)
            {
              if (vdata.Annotations.ContainsKey(header))
              {
                sw.Write("\t{0}", vdata.Annotations[header]);
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
      else
      {
        using (var sw = new StreamWriter(_options.OutputFile))
        {
          sw.WriteLine("Gene\t{0}", (from s in samples select s.StringAfter("_")).Merge("\t"));
          foreach (var gene in genes)
          {
            sw.Write(gene);
            foreach (var sample in samples)
            {
              sw.Write("\t{0}", getValue(valueMap[sample][gene]));
            }
            sw.WriteLine();
          }
        }

        using (var sw = new StreamWriter(_options.DesignFile))
        {
          sw.Write("Sample\tBarcode\tPatient\tTumorType\tPlatform\tSampleType\tSampleTypeDescription");
          if (headers.Count > 0)
          {
            sw.WriteLine("\t{0}", headers.Merge("\t"));
          }
          else
          {
            sw.WriteLine();
          }

          foreach (var entry in barMap)
          {
            var tumor = _options.TumorTypes.First();
            var type = TCGASampleCode.Find(entry.Value.Sample);
            sw.Write("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}", entry.Key.StringAfter("_"), entry.Value.BarCode, entry.Value.Paticipant, tumor, entry.Value.Platform, type.ShortLetterCode,
              type.Definition);
            var key = GetSampleKey(tumor, entry.Value.Paticipant);
            var vdata = clindata.ContainsKey(key) ? clindata[key] : new Annotation();
            foreach (var header in headers)
            {
              if (vdata.Annotations.ContainsKey(header))
              {
                sw.Write("\t{0}", vdata.Annotations[header]);
              }
              else
              {
                sw.Write("\t");
              }
            }
            sw.WriteLine();
          }
        }

        var clinicalOptions = new TCGAClinicalInformationBuilderOptions()
        {
          ClinicalFile = TCGAUtils.GetClinicPatientFile(_options.TCGADirectory, _options.TumorTypes.First()),
          DataFile = _options.OutputFile,
          ThrowException = false,
        };
        result.AddRange(new TCGAClinicalInformationBuilder(clinicalOptions) { Progress = this.Progress }.Process());
      }
      Progress.End();

      if (noclinical.Count == 0)
      {
        return result.ToArray();
      }
      else
      {
        return new[] { string.Format("There are {0} samples without patient information:\n  {1}\n\nResult have been saved to:\n  {2}", noclinical.Count, noclinical.Merge("\n  "), result.Merge("\n  ")) };
      }
    }

    private static string GetTumorType(string sample)
    {
      var tumor = sample.StringBefore("_");
      return tumor;
    }

    private static string GetSampleKey(string tumor, string paticipant)
    {
      return tumor + "_" + paticipant;
    }

    /// <summary>
    ///   Read the clinical patient file and store the data into directory. The key is tumor type plus bar code.
    ///   Also read the
    /// </summary>
    /// <param name="clinicalData"></param>
    /// <param name="tumorType"></param>
    /// <param name="queryHeaders"></param>
    private void ReadClinData(IDictionary<string, IAnnotation> clinicalData, string tumorType,
      ICollection<string> queryHeaders)
    {
      var clinfile = TCGAUtils.GetClinicPatientFile(_options.TCGADirectory, tumorType);
      if (!File.Exists(clinfile))
      {
        return;
      }

      var data = new TCGAClinicalInformationFormat().ReadFromFile(clinfile);
      foreach (var entry in data)
      {
        clinicalData[GetSampleKey(tumorType, entry.BarCode())] = entry;
      }
      var configheader = TemplateDirectory + "/" + Path.GetFileNameWithoutExtension(clinfile) + ".header.xml";
      if (!File.Exists(configheader))
      {
        configheader = TemplateDirectory + "/clinical_patient_tcga.header.xml";
      }

      var fd = HeaderDefinition.LoadFromFile(configheader);
      foreach (var line in fd.Properties)
      {
        if (!queryHeaders.Contains(line))
        {
          queryHeaders.Add(line);
        }
      }
    }

    private List<string> GetCommonGenes(Dictionary<string, Dictionary<string, double>> valueMap)
    {
      var genes = new HashSet<string>(valueMap.First().Value.Keys);
      foreach (var v in valueMap.Values)
      {
        genes.IntersectWith(v.Keys);
      }
      return genes.OrderBy(m => m).ToList();
    }
  }
}