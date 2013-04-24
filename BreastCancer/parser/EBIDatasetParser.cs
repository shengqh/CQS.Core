using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using RCPA;

namespace CQS.BreastCancer.parser
{
  public class EBIDatasetParser : IBreastCancerSampleInfoParser, IBreastCancerSampleInfoParser2
  {
    Dictionary<ColumnName, HashSet<string>> nameKeyMap;

    private string FindValue(Annotation a, ColumnName key)
    {
      string result;
      FindValue(a, key, out result);
      return result;
    }

    private bool FindValue(Annotation a, ColumnName key, out string value)
    {
      if (nameKeyMap.ContainsKey(key))
      {
        HashSet<string> keys = nameKeyMap[key];
        foreach (var ck in keys)
        {
          if (a.Annotations.ContainsKey(ck))
          {
            value = (a.Annotations[ck] as string).Trim();
            return true;
          }
        }
      }

      value = StatusValue.NA;
      return false;
    }

    public EBIDatasetParser()
    {
      var keymap = new Dictionary<string, ColumnName>();
      keymap["Array Data File"] = ColumnName.Sample;

      //E-MTAB-365
      keymap["Characteristics[Age]"] = ColumnName.Age;
      keymap["Characteristics[Grade::Scarff Bloom Richardson]"] = ColumnName.Grade;
      keymap["Characteristics[ESR1::Protein expression]"] = ColumnName.ER;
      keymap["Characteristics[PGR::Protein expression]"] = ColumnName.PR;
      keymap["Characteristics[ERBB2::Protein expression]"] = ColumnName.HER2;

      //E-TABM-158
      keymap["Characteristics [EstrogenReceptorStatus]"] = ColumnName.ER;
      keymap["Characteristics [Progesterone Receptor status]"] = ColumnName.PR;
      keymap["Characteristics [ErbB2 positive (IHC)]"] = ColumnName.HER2;
      keymap["Characteristics [dead of disease]"] = ColumnName.DeadOfDisease;
      keymap["Characteristics [node positive]"] = ColumnName.NodalStatus;
      keymap["Characteristics [TumorStaging]"] = ColumnName.TumorStage;
      keymap["Characteristics [alive at endpoint]"] = ColumnName.OverallServive;
      keymap["Characteristics [TumorGrading]"] = ColumnName.Grade;


      nameKeyMap = keymap.GroupBy(m => m.Value).ToDictionary(m => m.Key, m => new HashSet<string>(from n in m select n.Key));
    }

    public List<BreastCancerSampleItem> ParseDataset(string datasetDirectory)
    {
      var files = new HashSet<string>(from f in Directory.GetFiles(datasetDirectory, "*.CEL")
                                      select Path.GetFileNameWithoutExtension(f));

      var sdrfFile = Directory.GetFiles(datasetDirectory, "*.sdrf.txt");
      if (sdrfFile.Length == 0)
      {
        throw new ArgumentException("Cannot find sdrf file in directory " + datasetDirectory);
      }

      var ann = new AnnotationFormat("^#").ReadFromFile(sdrfFile[0]);
      return (from a in ann
              let filename = Path.GetFileNameWithoutExtension(FindValue(a, ColumnName.Sample))
              where files.Contains(filename)
              select new BreastCancerSampleItem()
              {
                Dataset = Path.GetFileName(datasetDirectory),
                Sample = filename,
                Age = FindValue(a, ColumnName.Age),
                ER = new StatusValue(FindValue(a, ColumnName.ER)).Value,
                PR = new StatusValue(FindValue(a, ColumnName.PR)).Value,
                HER2 = new StatusValue(FindValue(a, ColumnName.HER2)).Value,
                Stage = FindValue(a, ColumnName.Stage),
                TumorStatus = FindValue(a, ColumnName.TumorStage),
                Grade = FindValue(a, ColumnName.Grade),
                NodalStatus = FindValue(a, ColumnName.NodalStatus),
                PCR = FindValue(a, ColumnName.PCR),
                DFS = FindValue(a, ColumnName.DFS),
                DFSTime = FindValue(a, ColumnName.DFSTime),
                RFS = FindValue(a, ColumnName.RFS),
                RFSTime = FindValue(a, ColumnName.RFSTime),
                DMFS = FindValue(a, ColumnName.DMFS),
                DMFSTime = FindValue(a, ColumnName.DMFSTime),
                OverallSurvival = FindValue(a, ColumnName.OverallServive),
                DeadOfDisease = FindValue(a, ColumnName.DeadOfDisease)
              }).ToList();
    }

    public void ParseDataset(string datasetDirectory, Dictionary<string, BreastCancerSampleItem> sampleMap)
    {
      var files = new HashSet<string>(from f in Directory.GetFiles(datasetDirectory, "*.CEL")
                                      select Path.GetFileNameWithoutExtension(f));

      var sdrfFile = Directory.GetFiles(datasetDirectory, "*.sdrf.txt");
      if (sdrfFile.Length == 0)
      {
        throw new ArgumentException("Cannot find sdrf file in directory " + datasetDirectory);
      }

      var ann = new AnnotationFormat("^#").ReadFromFile(sdrfFile[0]);
      var dataset = Path.GetFileName(datasetDirectory);
      foreach (var a in ann)
      {
        var filename = Path.GetFileNameWithoutExtension(FindValue(a, ColumnName.Sample));
        if (files.Contains(filename))
        {
          if (!sampleMap.ContainsKey(filename))
          {
            sampleMap[filename] = new BreastCancerSampleItem();
            sampleMap[filename].Dataset = dataset;
            sampleMap[filename].Sample = filename;
          }
          var item = sampleMap[filename];

          string value;
          if (FindValue(a, ColumnName.Age, out value))
          {
            item.Age = value;
          }

          if (FindValue(a, ColumnName.ER, out value))
          {
            item.ER = StatusValue.TransferStatus(value);
          }

          if (FindValue(a, ColumnName.PR, out value))
          {
            item.PR = StatusValue.TransferStatus(value);
          }

          if (FindValue(a, ColumnName.HER2, out value))
          {
            item.HER2 = StatusValue.TransferStatus(value);
          }

          if (FindValue(a, ColumnName.Stage, out value))
          {
            item.Stage = value;
          }

          if (FindValue(a, ColumnName.TumorStage, out value))
          {
            item.TumorStatus = value;
          }

          if (FindValue(a, ColumnName.Grade, out value))
          {
            item.Grade = value;
          }

          if (FindValue(a, ColumnName.NodalStatus, out value))
          {
            item.NodalStatus = value;
          }

          if (FindValue(a, ColumnName.PCR, out value))
          {
            item.PCR = value;
          }

          if (FindValue(a, ColumnName.DFS, out value))
          {
            item.DFS = value;
          }

          if (FindValue(a, ColumnName.DFSTime, out value))
          {
            item.DFSTime = value;
          }

          if (FindValue(a, ColumnName.RFS, out value))
          {
            item.RFS = value;
          }

          if (FindValue(a, ColumnName.RFSTime, out value))
          {
            item.RFSTime = value;
          }

          if (FindValue(a, ColumnName.DMFS, out value))
          {
            item.DMFS = value;
          }

          if (FindValue(a, ColumnName.DMFSTime, out value))
          {
            item.DMFSTime = value;
          }

          if (FindValue(a, ColumnName.OverallServive, out value))
          {
            item.OverallSurvival = value;
          }

          if (FindValue(a, ColumnName.DeadOfDisease, out value))
          {
            item.DeadOfDisease = value;
          }
        }
      }
    }
  }
}
