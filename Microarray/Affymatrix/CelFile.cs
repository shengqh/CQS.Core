using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using System.IO.Compression;

namespace CQS.Microarray.Affymatrix
{
  public class CelFile
  {
    public static readonly char DELIMCHAR = '\x14';


    /// <summary>
    /// Parsing chip type from CEL file. Only uncompressed file can be parsed.
    /// </summary>
    /// <param name="fileName"></param>
    /// <returns></returns>
    public static string DoGetChipType(StreamReader sr)
    {
      var result = string.Empty;
      using (sr)
      {
        string line;
        List<string> patterns = new string[] { "DatHeader=" }.ToList();
        bool bFirst = true;
        bool bCurrent = false;
        while ((line = sr.ReadLine()) != null)
        {
          var ipos = line.IndexOf('\0');
          if (ipos != -1)
          {
            line = line.Replace("\0", "");
            if (bFirst)
            {
              patterns.Insert(0, "dat-header");
              bFirst = false;
            }
          }

          if (!bCurrent && !patterns.Any(m => line.Contains(m)))
          {
            continue;
          }

          var iStart = line.IndexOf(DELIMCHAR);
          if (iStart == -1)
          {
            if (bCurrent)
            {
              break;
            }

            bCurrent = true;
            continue;
          }

          iStart = line.IndexOf(DELIMCHAR, iStart + 1);
          if (iStart == -1)
          {
            continue;
          }

          iStart++;

          var iEnd = line.IndexOf('.', iStart);
          if (iEnd == -1)
          {
            iEnd = line.IndexOf(DELIMCHAR, iStart);
            if (iEnd == -1)
            {
              break;
            }
            iEnd--;
          }

          result = line.Substring(iStart, iEnd - iStart).Trim();
          break;
        }
      }
      return result;
    }

    /// <summary>
    /// Parsing chip type from CEL file. Only uncompressed file can be parsed.
    /// </summary>
    /// <param name="fileName"></param>
    /// <returns></returns>
    public static string GetChipType(string fileName)
    {
      try
      {
        StreamReader sr;

        if (fileName.ToLower().EndsWith(".gz"))
        {
          sr = new StreamReader(new GZipStream(new FileStream(fileName, FileMode.Open), CompressionMode.Decompress));
        }
        else
        {
          sr = new StreamReader(fileName);
        }

        return DoGetChipType(sr);
      }
      catch (Exception ex)
      {
        Console.WriteLine("Error to parse chip type from " + fileName);
        throw ex;
      }
    }
  }
}
