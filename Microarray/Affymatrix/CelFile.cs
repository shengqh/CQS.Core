using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using System.IO.Compression;
using RCPA;
using RCPA.R;

namespace CQS.Microarray.Affymatrix
{
  public static class CelFile
  {
    public static Dictionary<string, string> GetChipTypes(string rExecute, string directory, bool includingSubDirectory, string outputFile)
    {
      var cels = GetCelFiles(directory);
      foreach (var dir in Directory.GetDirectories(directory))
      {
        cels.AddRange(GetCelFiles(dir));
      }

      if (cels.Count == 0)
      {
        return new Dictionary<string, string>();
      }

      var inputfile = Path.Combine(directory, "celfiles.tsv");
      using (var sw = new StreamWriter(inputfile))
      {
        foreach (var cel in cels)
        {
          sw.WriteLine(FileUtils.ToLinuxFormat(cel));
        }
      }

      var roptions = new RTemplateProcessorOptions();
      roptions.RExecute = rExecute;
      roptions.InputFile = inputfile;
      roptions.OutputFile = outputFile;
      roptions.RTemplate = FileUtils.GetTemplateDir() + "/getceltypes.r";
      new RTemplateProcessor(roptions).Process();
      return new MapReader(0, 1).ReadFromFile(roptions.OutputFile);
    }

    public static List<string> GetCelFiles(string directory, bool drillDown = true)
    {
      return FileUtils.GetFiles(directory, "*.cel.gz", drillDown).Union(FileUtils.GetFiles(directory, "*.cel", drillDown)).ToList();
    }

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
      return result.Trim();
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
        using (var sr = StreamUtils.GetReader(fileName))
        {
          var result = DoGetChipType(sr);
          if (string.IsNullOrEmpty(result))
          {
            Console.Error.WriteLine("No chip type found in {0}", fileName);
          }
          else if (!Char.IsLetter(result[0]))
          {
            Console.Error.WriteLine("Wrong chip type {0} found in {1}", result, fileName);
          }

          return result;
        }
      }
      catch (Exception ex)
      {
        Console.WriteLine("Error to parse chip type from " + fileName);
        throw ex;
      }
    }
  }
}
