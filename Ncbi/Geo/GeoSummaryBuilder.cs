using Microsoft.Office.Interop.Excel;
using RCPA;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;

namespace CQS.Ncbi.Geo
{
  public class GeoSummaryBuilder : AbstractThreadProcessor
  {
    private GeoSummaryBuilderOptions options;

    public GeoSummaryBuilder(GeoSummaryBuilderOptions options)
    {
      this.options = options;
    }

    public override IEnumerable<string> Process()
    {
      var sql = string.Format(@"
select DISTINCT gse.gse, count(gsm.gsm) as gsm_count, gse.submission_date, gse.title, gse.summary, gse.type, gse.overall_design
from gse 
	JOIN gse_gsm ON gse.gse=gse_gsm.gse   
	JOIN gsm ON gse_gsm.gsm=gsm.gsm
	JOIN gse_gpl ON gse_gpl.gse=gse.gse   
	JOIN gpl ON gse_gpl.gpl=gpl.gpl 
{0} 
GROUP BY gse.gse
order by gse.ID", options.GetFilterSql());

      File.WriteAllText(Path.ChangeExtension(options.OutputFile, ".sql"), sql);

      SQLiteDBHelper sqlite = new SQLiteDBHelper(options.GeoMetaDatabase);

      var data = sqlite.ExecuteReader(sql, null);

      var xlApp = new Microsoft.Office.Interop.Excel.Application();
      try
      {
        Workbook workbook = xlApp.Workbooks.Add();
        try
        {
          Worksheet workSheet = workbook.Worksheets[1];
          for (int i = 0; i < data.FieldCount; i++)
          {
            workSheet.Cells[1, i + 1] = data.GetName(i);
          }

          var row = 1;
          while (data.Read())
          {
            var gsmCount = data.GetInt32(1);
            if (gsmCount >= options.MininumGsmPerGse)
            {
              row++;

              for (int i = 0; i < data.FieldCount; i++)
              {
                workSheet.Cells[row, i + 1] = data.GetValue(i);
              }

              var position = "A" + row.ToString();
              Range range = workSheet.Range[position];
              var link = string.Format(@"http://www.ncbi.nlm.nih.gov/geo/query/acc.cgi?acc={0}", data.GetString(0));
              workSheet.Hyperlinks.Add(range, link);
            }
          }
          workSheet.Range["A:A"].EntireColumn.ColumnWidth = 10;
          workSheet.Range["A:A"].EntireColumn.VerticalAlignment = XlVAlign.xlVAlignTop;
          workSheet.Range["B:B"].EntireColumn.ColumnWidth = 10;
          workSheet.Range["B:B"].EntireColumn.VerticalAlignment = XlVAlign.xlVAlignTop;
          workSheet.Range["C:C"].EntireColumn.ColumnWidth = 15;
          workSheet.Range["C:C"].EntireColumn.VerticalAlignment = XlVAlign.xlVAlignTop;
          for (int i = 3; i < data.FieldCount; i++)
          {
            var col = (char)('A' + i);
            var range = string.Format("{0}:{0}", col);
            workSheet.Range[range].EntireColumn.ColumnWidth = 60;
            workSheet.Range[range].EntireColumn.WrapText = true;
            workSheet.Range[range].EntireColumn.VerticalAlignment = XlVAlign.xlVAlignTop;
          }
          xlApp.DisplayAlerts = false;
          workbook.SaveAs(options.OutputFile);
        }
        finally
        {
          workbook.Close();
        }
      }
      finally
      {
        xlApp.Quit();
      }

      return new string[] { options.OutputFile };
    }
  }
}
