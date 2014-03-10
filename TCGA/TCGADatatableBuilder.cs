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

    public TCGADatatableBuilder(TCGADatatableBuilderOptions options)
    {
      _options = options;
    }

    private Dictionary<string, Dictionary<string, double>> GetData(Dictionary<string, BarInfo> barMap,
      out Func<double, double> getValue)
    {
      Func<string, string> getFilename;
      IFileReader<ExpressionData> reader;

      ITCGATechnology tec = _options.GetTechnology();
      if (_options.IsCount)
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
      var barMap = new Dictionary<string, BarInfo>();
      foreach (var tumor in _options.TumorTypes)
      {
        var curMap = TCGAUtils.GetBarcodeFileMap(_options.TCGADirectory,
          _options.GetTechnology(), tumor, _options.GetTCGASampleCodes().ToArray());
        foreach (var v in curMap)
        {
          barMap[GetSampleKey(tumor, v.Key)] = v.Value;
        }
      }

      var headers = new List<string>();
      var clindata = new Dictionary<string, Dictionary<string, string>>();
      foreach (var tumor in _options.TumorTypes)
      {
        ReadClinData(clindata, tumor, headers);
      }

      foreach (var bm in barMap)
      {
        if (!clindata.ContainsKey(GetSampleKey(GetTumorType(bm.Key), bm.Value.Paticipant)))
        {
          Console.Error.WriteLine(string.Format("Cannot find clinical data for patient {0}", bm.Value.Paticipant));
        }
      }

      Progress.SetMessage("Reading data ...");
      Func<double, double> getValue;
      var valueMap = GetData(barMap, out getValue);

      var genes = GetCommonGenes(valueMap);
      var samples = valueMap.Keys.OrderBy(m => m).ToList();

      Progress.SetMessage("Saving data ...");
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

      using (var sw = new StreamWriter(_options.OutputFile + ".design"))
      {
        sw.Write("Sample\tBarcode\tPatient\tTumorType\tSampleType\tSampleTypeDescription");
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
          sw.Write("{0}\t{1}\t{2}\t{3}\t{4}\t{5}", entry.Key, entry.Value.BarCode, entry.Value.Paticipant, tumor, type.ShortLetterCode,
            type.Definition);
          var key = GetSampleKey(tumor, entry.Value.Paticipant);
          var vdata = clindata.ContainsKey(key) ? clindata[key] : new Dictionary<string, string>();
          foreach (var header in headers)
          {
            if (vdata.ContainsKey(header))
            {
              sw.Write("\t{0}", vdata[header]);
            }
            else
            {
              sw.Write("\t");
            }
          }
          sw.WriteLine();
        }
      }
      Progress.End();

      return new[] { _options.OutputFile, _options.OutputFile + ".design" };
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
    private void ReadClinData(IDictionary<string, Dictionary<string, string>> clinicalData, string tumorType,
      ICollection<string> queryHeaders)
    {
      var clinfile = TCGAUtils.GetClinicPatientFile(_options.TCGADirectory, tumorType);
      if (!File.Exists(clinfile))
      {
        return;
      }

      using (var sr = new StreamReader(clinfile))
      {
        var header = sr.ReadLine();
        var headers = header.Split('\t');
        string line;

        while ((line = sr.ReadLine()) != null)
        {
          if (string.IsNullOrEmpty(line))
          {
            continue;
          }
          var parts = line.Split('\t');
          if (parts.Length != headers.Length)
          {
            continue;
          }
          var data = new Dictionary<string, string>();
          for (var i = 0; i < parts.Length; i++)
          {
            data[headers[i]] = parts[i];
          }
          clinicalData[GetSampleKey(tumorType, data["bcr_patient_barcode"])] = data;
        }
      }

      string configheader = FileUtils.GetTemplateDir() + "/" + Path.GetFileNameWithoutExtension(clinfile) + ".header.xml";
      if (!File.Exists(configheader))
      {
        configheader = FileUtils.GetTemplateDir() + "/clinical_patient_tcga.header.xml";
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