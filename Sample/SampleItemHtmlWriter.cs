using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RCPA;
using System.IO;
using CQS.Converter;

namespace CQS.Sample
{
  public class SampleItemHtmlWriter : IFileWriter<List<SampleItem>>
  {
    private static string linkstr = "<td><a href=\"{0}\" target=\"_blank\">{1}</a></td>";
    private static string nolinkstr = "<td>{0}</td>";

    private IPropertyConverter<SampleItem>[] converters;

    public SampleItemHtmlWriter(IEnumerable<string> columns)
    {
      this.converters = SampleUtils.GetConverters(columns);
    }

    public void WriteToFile(string fileName, List<SampleItem> t)
    {
      using (StreamWriter sw = new StreamWriter(fileName))
      {
        sw.WriteLine("<table border=\"1\">");
        sw.WriteLine("<tr>");
        sw.WriteLine("<td>Index</td>");
        foreach (var conv in converters)
        {
          sw.WriteLine("<td>{0}</td>", conv.Name);
        }
        sw.WriteLine("</tr>");

        for (int i = 0; i < t.Count; i++)
        {
          sw.WriteLine("<tr>");
          
          sw.WriteLine(nolinkstr, i+1);
          for (int j = 0; j < converters.Length; j++)
          {

            if (converters[j].Name.Equals("Dataset"))
            {
              string link;
              if (t[i].Dataset.StartsWith("GSE"))
              {
                link = string.Format(@"http://www.ncbi.nlm.nih.gov/geo/query/acc.cgi?acc={0}", t[i].Dataset);
              }
              else if (t[i].Dataset.StartsWith("E-"))
              {
                link = string.Format(@"http://www.ebi.ac.uk/arrayexpress/experiments/{0}", t[i].Dataset);
              }
              else
              {
                link = string.Format(@"https://www.google.com/search?q={0}", t[i].Dataset);
              }
              sw.WriteLine(linkstr, link, t[i].Dataset);
            }
            else if (converters[j].Name.Equals("Sample"))
            {
              if (t[i].Sample.StartsWith("GSM"))
              {
                var link = string.Format(@"http://www.ncbi.nlm.nih.gov/geo/query/acc.cgi?acc={0}", t[i].Sample);
                sw.WriteLine(linkstr, link, t[i].Sample);
              }
              else
              {
                sw.WriteLine(nolinkstr, t[i].Sample);
              }
            }
            else
            {
              sw.WriteLine(nolinkstr, converters[j].GetProperty(t[i]));
            }
          }

          sw.WriteLine("<tr>");
        }
        sw.WriteLine("</table>");
      }
    }
  }
}
