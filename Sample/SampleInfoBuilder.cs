using RCPA;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CQS.Sample
{
  public class SampleInfoBuilder : AbstractThreadFileProcessor
  {
    private List<string> properties;

    public SampleInfoBuilder(string propertyFile)
    {
      this.properties = SampleUtils.ReadPropertiesFromFile(propertyFile);
    }

    public override IEnumerable<string> Process(string rootDirectory)
    {
      var subdirs = SampleUtils.GetDatasets(rootDirectory);

      List<SampleItem> total = new List<SampleItem>();

      foreach (var subdir in subdirs)
      {
        var sifile = subdir + "/" + Path.GetFileName(subdir) + ".siformat";
        if (!File.Exists(sifile))
        {
          throw new Exception("File is not exists:" + sifile);
        }
      }

      var format = new SampleItemTextWriter(this.properties);

      foreach (var subdir in subdirs)
      {
        Progress.SetMessage("processing " + subdir + " ...");

        var dirname = Path.GetFileName(subdir);
        var sifile = subdir + "/" + dirname + ".siformat";
        var parser = new SampleItemParser(sifile);

        //Console.WriteLine(dirname);

        var flist = parser.ParseDataset(subdir);

        total.AddRange(flist.Values);

        var sampleFile = subdir + @"/" + dirname + ".sample";
        format.WriteToFile(sampleFile, flist.Values.ToList());
      }

      var tsvFile = rootDirectory + "\\" + Path.GetFileNameWithoutExtension(rootDirectory) + "_SampleInformation.tsv";
      format.WriteToFile(tsvFile, total);

      var htmlFile = Path.ChangeExtension(tsvFile, ".html");
      new SampleItemHtmlWriter(this.properties).WriteToFile(htmlFile, total);

      var excelFile = Path.ChangeExtension(htmlFile, ".xls");
      new SampleItemExcelWriter(this.properties).WriteToFile(excelFile, total);

      return new string[] { tsvFile, htmlFile, excelFile };
    }
  }
}
