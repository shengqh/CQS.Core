using Microsoft.Office.Interop.Excel;
using RCPA;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CQS.BreastCancer
{
  public class BreastCancerSampleItemExcelWriter : IFileWriter<List<BreastCancerSampleItem>>
  {
    public void WriteToFile(string fileName, List<BreastCancerSampleItem> t)
    {
      var factory = new BreastCancerSampleItemPropertyFactory();
      var headers1 = BreastCancerSampleItemFormat.DefaultHeader.Split('\t');
      var converters1 = (from header in headers1
                         select factory.FindConverter(header)).ToArray();

      var filtered = (from index in Enumerable.Range(0, headers1.Length)
                      let oldheader = headers1[index]
                      let oldconverter = converters1[index]
                      where t.Any(m => !string.IsNullOrEmpty(oldconverter.GetProperty(m)))
                      select new { Header = oldheader, Converter = oldconverter }).ToArray();

      var xlApp = new Microsoft.Office.Interop.Excel.Application();
      xlApp.Visible = false;
      try
      {
        Workbook workbook = xlApp.Workbooks.Add();
        try
        {
          Worksheet workSheet = workbook.Worksheets[1];

          for (int j = 0; j < filtered.Length; j++)
          {
            workSheet.Cells[1, j + 1] = filtered[j].Header;
            Range r = (Microsoft.Office.Interop.Excel.Range)workSheet.Cells[1, j + 1];
            r.EntireColumn.NumberFormat = "@";
          }

          for (int i = 0; i < t.Count; i++)
          {
            var row = i + 2;
            try
            {
              for (int j = 0; j < filtered.Length; j++)
              {
                workSheet.Cells[row, j + 1] = filtered[j].Converter.GetProperty(t[i]);
              }
            }
            catch (Exception ex)
            {
              Console.WriteLine(ex.Message);
              break;
            }

            var position = "A" + row.ToString();
            Range range = workSheet.Range[position];
            var value = range.Value2;

            if (t[i].Dataset.StartsWith("GSE"))
            {
              var link = string.Format(@"http://www.ncbi.nlm.nih.gov/geo/query/acc.cgi?acc={0}", t[i].Dataset);
              workSheet.Hyperlinks.Add(range, link);
            }
            else if (t[i].Dataset.StartsWith("E-"))
            {
              var link = string.Format(@"http://www.ebi.ac.uk/arrayexpress/experiments/{0}", t[i].Dataset);
              workSheet.Hyperlinks.Add(range, link);
            }
            else
            {
              var link = string.Format(@"https://www.google.com/search?q={0}", t[i].Dataset);
              workSheet.Hyperlinks.Add(range, link);
            }

            if (t[i].Sample.StartsWith("GSM"))
            {
              position = "B" + row.ToString();
              range = workSheet.Range[position];
              var link = string.Format(@"http://www.ncbi.nlm.nih.gov/geo/query/acc.cgi?acc={0}", t[i].Sample);
              workSheet.Hyperlinks.Add(range, link);
            }
          }

          workbook.Saved = true;
          workbook.SaveAs(fileName, XlFileFormat.xlExcel12);
        }
        finally
        {
          workbook.Close(false);
        }
      }
      finally
      {
        xlApp.Quit();
      }
    }
  }
}
