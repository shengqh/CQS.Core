using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RCPA;
using System.IO;

namespace CQS.TCGA.Microarray
{
  public class MicroarrayDataSummaryBuilder : AbstractThreadFileProcessor
  {
    private string dataDir;

    private string sdrfFile;

    public MicroarrayDataSummaryBuilder(string dataDir, string sdrfFile)
    {
      this.dataDir = dataDir;
      this.sdrfFile = sdrfFile;
    }

    public override IEnumerable<string> Process(string targetfile)
    {
      var dirs = Directory.GetDirectories(dataDir, "*.Level_3*");

      Progress.SetMessage("Total {0} directories", dirs.Length);

      var reader = new ExpressionDataRawReader(2, 1, 2);
      var finder = TCGATechnology.Microarray.GetFinder(null, dataDir);

      var datas = new List<ExpressionData>();

      int dircount = 0;
      foreach (var dir in dirs)
      {
        dircount++;
        Progress.SetMessage("{0}/{1} : {2}", dircount, dirs.Length, dir);

        var files = Directory.GetFiles(dir, "*level3.data.txt");

        Progress.SetRange(1, files.Length);
        foreach (var file in files)
        {
          Progress.Increment(1);
          var data = reader.ReadFromFile(file);
          data.SampleBarcode = finder.FindParticipant(Path.GetFileName(file));
          datas.Add(data);
        }
      }

      Progress.SetMessage("Sorting barcodes ...");
      datas.SortBarcode();

      Progress.SetMessage("Filling and sorting genes ...");
      datas.FillAndSortGenes();

      Progress.SetMessage("Saving result ...");
      new ExpressionDataFastFormat<ExpressionData>().WriteToFile(targetfile, datas);

      Progress.SetMessage("Finished!");

      return new string[] { targetfile };
    }
  }
}
