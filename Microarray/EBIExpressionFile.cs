using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace CQS.Microarray
{
  public class EBIExpressionFile : ISummaryFile
  {
    public HashSet<string> ReadGenes(string fileName)
    {
      var result = new HashSet<string>();

      using (StreamReader sr = new StreamReader(fileName))
      {
        string line;
        while ((line = sr.ReadLine()) != null)
        {
          if (line.StartsWith("Scan REF"))
          {
            break;
          }
        }

        while ((line = sr.ReadLine()) != null)
        {
          if (line.Trim().Length == 0 || line.Contains(" REF"))
          {
            continue;
          }
          result.Add(line.Substring(0, line.IndexOf('\t')).Trim());
        }
      }
      return result;
    }

  }
}
