using RCPA;
using System;
using System.Collections.Generic;
using System.IO;

namespace CQS.Genome.GroSeq
{
  public class PeakAnnotationItemFormat : IFileFormat<List<PeakAnnotationItem>>
  {
    public static string DefaultHeader = "PeakID\tChr\tStart\tEnd\tStrand\tAnnotation\tDetailed Annotation";

    private LineFormat<PeakAnnotationItem> _format { get; set; }

    public LineFormat<PeakAnnotationItem> Format
    {
      get
      {
        if (_format == null)
        {
          _format = new LineFormat<PeakAnnotationItem>(PeakAnnotationItemPropertyFactory.GetInstance(), DefaultHeader);
        }
        return _format;
      }
      set
      {
        _format = value;
      }
    }

    private void InitializeLineFormat(string header)
    {
      _format = new LineFormat<PeakAnnotationItem>(PeakAnnotationItemPropertyFactory.GetInstance(), header);
    }

    public List<PeakAnnotationItem> ReadFromFile(string fileName)
    {
      var result = new List<PeakAnnotationItem>();

      using (StreamReader sr = new StreamReader(fileName))
      {
        string line = sr.ReadLine();

        InitializeLineFormat(line);

        while ((line = sr.ReadLine()) != null)
        {
          if (line.Trim() == string.Empty)
          {
            break;
          }

          result.Add(Format.ParseString(line));
        }
      }

      return result;
    }

    public void WriteToFile(string fileName, List<PeakAnnotationItem> t)
    {
      throw new NotImplementedException();
    }
  }
}
