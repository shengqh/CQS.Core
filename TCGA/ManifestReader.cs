using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace CQS.TCGA
{
  public static class ManifestReader
  {
    public static Dictionary<string, string> ReadFromFile(string fileName)
    {
      var result = new Dictionary<string, string>();

      using (StreamReader sr = new StreamReader(fileName))
      {
        string line;
        while ((line = sr.ReadLine()) != null)
        {
          var parts = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

          if (parts.Length > 1)
          {
            result[parts[1].ToLower()] = parts[0].ToLower();
          }
        }
      }

      return result;
    }
  }
}
