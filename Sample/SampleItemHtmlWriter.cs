using RCPA;
using System;
using System.Collections.Generic;
using System.IO;

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
        sw.WriteLine(@"<!DOCTYPE html PUBLIC ""-//W3C//DTD XHTML 1.0 Transitional//EN"" ""http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd"">
<html xmlns=""http://www.w3.org/1999/xhtml"">
<head>
<meta http-equiv=""Content-Type"" content=""text/html; charset=utf-8"" />
<title>" + Path.GetFileName(fileName).StringBefore(".") + @" Sample Information</title>
<style type=""text/css"">
body
{
	line-height: 1.1em;
}
#newspaper-c
{
	font-family: ""Lucida Sans Unicode"", ""Lucida Grande"", Sans-Serif;
	font-size: 12px;
	margin: 45px;
	width: 480px;
	text-align: left;
	border-collapse: collapse;
	border: 1px solid #6cf;
}
#newspaper-c th
{
	padding: 20px;
	font-weight: normal;
	font-size: 13px;
	color: #039;
	border: 1px solid #6cf;
	background: lightblue;
}
#newspaper-c td
{
	padding: 10px 20px;
	color: #669;
	border: 1px solid #6cf;
  white-space: nowrap;
}
#newspaper-c tbody tr:hover td
{
	color: #339;
	background: #d0dafd;
}
</style>
</head>
<body>
<table id=""newspaper-c"">
<thead>

<tr>
<th>Index</td>
");
        foreach (var conv in converters)
        {
          sw.WriteLine("<th>{0}</td>", conv.Name);
        }
        sw.WriteLine(@"</tr>
</thead>
<tbody>
");

        for (int i = 0; i < t.Count; i++)
        {
          sw.WriteLine("<tr>");

          sw.WriteLine(nolinkstr, i + 1);
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
        sw.WriteLine(@"</tbody>
</table>
</body>
</html>");
      }
    }
  }
}
