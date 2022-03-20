using CQS.BreastCancer.parser;
using CQS.Sample;
using RCPA;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CQS.BreastCancer
{
  public class BreastCancerSampleInformationBuilder : AbstractThreadFileProcessor
  {
    public override IEnumerable<string> Process(string rootDirectory)
    {
      var subdirs = SampleUtils.GetDatasets(rootDirectory);

      var format = new BreastCancerSampleItemFormat();

      List<BreastCancerSampleItem> total = new List<BreastCancerSampleItem>();

      foreach (var subdir in subdirs)
      {
        var parser = ParserFactory.GetParserInDirectory(subdir);

        var dirname = Path.GetFileName(subdir);
        Console.WriteLine(dirname);

        Dictionary<string, BreastCancerSampleItem> flist = new Dictionary<string, BreastCancerSampleItem>();
        parser.ParseDataset(subdir, flist);

        total.AddRange(flist.Values);

        var sampleFile = subdir + @"\" + dirname + ".sample";
        format.WriteToFile(sampleFile, flist.Values.ToList());
      }

      var htmlFile = rootDirectory + "\\" + Path.GetFileNameWithoutExtension(rootDirectory) + "_SampleInformation.html";
      new BreastCancerSampleItemHtmlWriter().WriteToFile(htmlFile, total);

      var excelFile = Path.ChangeExtension(htmlFile, ".xls");
      new BreastCancerSampleItemExcelWriter().WriteToFile(excelFile, total);

      return new string[] { htmlFile, excelFile };
    }
  }
}
