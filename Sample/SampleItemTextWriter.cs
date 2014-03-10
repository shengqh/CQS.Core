using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RCPA;
using System.IO;
using RCPA.Converter;
using CQS.Converter;
using CQS.Sample;

namespace CQS.Sample
{
  public class SampleItemTextWriter : IFileWriter<List<SampleItem>>
  {
    public SampleItemTextWriter(IEnumerable<string> properties)
    {
      var headers = SampleUtils.GetColumns(properties).Merge("\t");
      _format = new LineFormat<SampleItem>(new SampleItemPropertyFactory<SampleItem>(), headers);
    }

    private LineFormat<SampleItem> _format { get; set; }

    public void WriteToFile(string fileName, List<SampleItem> t)
    {
      using (StreamWriter sw = new StreamWriter(fileName))
      {
        sw.WriteLine(this._format.GetHeader());
        foreach (var item in t)
        {
          sw.WriteLine(this._format.GetString(item));
        }
      }
    }
  }
}
