using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RCPA;
using Microsoft.Office.Interop.Excel;

namespace CQS.BreastCancer
{
  public class BreastCancerSampleItemExcelWriter : IFileWriter<List<BreastCancerSampleItem>>
  {
    public void WriteToFile(string fileName, List<BreastCancerSampleItem> t)
    {
      var headers = BreastCancerSampleItemFormat.DefaultHeader.Split('\t');
      var factory = new BreastCancerSampleItemPropertyFactory();
      var converters = (from header in headers
                        select factory.FindConverter(header)).ToArray();

      var xlApp = new Microsoft.Office.Interop.Excel.Application();
      xlApp.Visible = false;
      try
      {
        Workbook workbook = xlApp.Workbooks.Add();
        try
        {
          Worksheet workSheet = workbook.Worksheets[1];

          for (int j = 0; j < headers.Length; j++)
          {
            workSheet.Cells[1, j + 1] = headers[j];
          }

          for (int i = 0; i < t.Count; i++)
          {
            var row = i + 2;
            try
            {
              for (int j = 0; j < headers.Length; j++)
              {
                workSheet.Cells[row, j + 1] = converters[j].GetProperty(t[i]);
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
