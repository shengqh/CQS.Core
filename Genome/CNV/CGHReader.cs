using RCPA;
using System;
using System.Collections.Generic;
using System.IO;

namespace CQS.Genome.CNV
{
  public class CGHReader : IFileReader<List<CNVItem>>
  {
    private static Dictionary<string, Action<string, CNVItem>> headerMap;

    static CGHReader()
    {
      headerMap = new Dictionary<string, Action<string, CNVItem>>();
      headerMap["Chr"] = CNVItemUtils.FuncChrom;
      headerMap["Cytoband"] = CNVItemUtils.FuncItemName;
      headerMap["Start"] = CNVItemUtils.FuncChromStart;
      headerMap["Stop"] = CNVItemUtils.FuncChromEnd;
      headerMap["Amplification"] = (m, n) =>
      {
        var v = double.Parse(m);
        if (v > 0)
        {
          n.ItemType = CNVType.DUPLICATION;
        }
      };
      headerMap["Deletion"] = (m, n) =>
      {
        var v = double.Parse(m);
        if (v < 0)
        {
          n.ItemType = CNVType.DELETION;
        }
      };
      headerMap["pval"] = CNVItemUtils.FuncPValue;
      headerMap["Gene Names"] = CNVItemUtils.FuncAnnotation;

    }

    public List<CNVItem> ReadFromFile(string fileName)
    {
      List<CNVItem> result = new List<CNVItem>();

      using (StreamReader sr = new StreamReader(fileName))
      {
        string line;

        //ignore parameter lines
        while ((line = sr.ReadLine()) != null)
        {
          if (line.Trim().Length == 0)
          {
            break;
          }
        }

        //ignore empty lines
        while ((line = sr.ReadLine()) != null)
        {
          if (line.Trim().Length != 0)
          {
            break;
          }
        }

        if (line != null)
        {
          List<Action<string, CNVItem>> actionMap = new List<Action<string, CNVItem>>();
          var parts = line.Split('\t');
          int headerLength = parts.Length;
          for (int i = 0; i < headerLength; i++)
          {
            if (headerMap.ContainsKey(parts[i]))
            {
              actionMap.Add(headerMap[parts[i]]);
            }
            else
            {
              actionMap.Add(CNVItemUtils.FuncNothing);
            }
          }

          string lastfilename = string.Empty;

          while ((line = sr.ReadLine()) != null)
          {
            parts = line.Split('\t');
            if (parts.Length != headerLength)
            {
              lastfilename = line;
              continue;
            }

            CNVItem item = new CNVItem();
            item.FileName = lastfilename;
            for (int i = 0; i < headerLength; i++)
            {
              actionMap[i](parts[i], item);
            }

            result.Add(item);
          }
        }
      }

      return result;
    }
  }
}
