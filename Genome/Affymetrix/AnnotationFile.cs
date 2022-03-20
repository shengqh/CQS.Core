using LumenWorks.Framework.IO.Csv;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CQS.Genome.Affymetrix
{
  public static class AnnotationFile
  {
    public static Dictionary<string, string> GetGeneSymbolDescriptionMap(string fileName)
    {
      var result = new Dictionary<string, string>();
      using (StreamReader sr = new StreamReader(fileName))
      {
        string line;
        while ((line = sr.ReadLine())[0] == '#') ;
        var csv = new CsvReader(sr, false);

        while (csv.ReadNextRecord())
        {
          var titles = (from g in csv[13].Split(new string[] { "///" }, StringSplitOptions.None)
                        select g.Trim()).ToArray();
          var genelist = (from g in csv[14].Split(new string[] { "///" }, StringSplitOptions.None)
                          select g.Trim()).ToArray();
          for (int i = 0; i < genelist.Length; i++)
          {
            result[genelist[i]] = titles[i];
          }
        }
      }
      return result;
    }
  }
}
