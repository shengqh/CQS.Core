using RCPA;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CQS.BreastCancer
{
  public class BreastCancerSampleItemHtmlWriter : IFileWriter<List<BreastCancerSampleItem>>
  {
    private static string linkstr = "<td><a href=\"{0}\" target=\"_blank\">{1}</a></td>";
    private static string nolinkstr = "<td>{0}</td>";

    public void WriteToFile(string fileName, List<BreastCancerSampleItem> t)
    {
      var headers = BreastCancerSampleItemFormat.DefaultHeader.Split('\t');
      var factory = new BreastCancerSampleItemPropertyFactory();
      var converters = (from header in headers
                        select factory.FindConverter(header)).ToArray();
      using (StreamWriter sw = new StreamWriter(fileName))
      {
        sw.WriteLine("<table border=\"1\">");
        sw.WriteLine("<tr>");
        sw.WriteLine("<td>Index</td>");
        foreach (var header in headers)
        {
          sw.WriteLine("<td>{0}</td>", header);
        }
        sw.WriteLine("</tr>");

        for (int i = 0; i < t.Count; i++)
        {
          sw.WriteLine("<tr>");

          sw.WriteLine(nolinkstr, i + 1);
          for (int j = 0; j < headers.Length; j++)
          {

            if (headers[j].Equals("Dataset"))
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
            else if (headers[j].Equals("Sample"))
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
