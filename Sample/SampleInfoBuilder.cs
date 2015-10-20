using RCPA;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CQS.Sample
{
  public class SampleInfoBuilder : AbstractThreadProcessor
  {
    private SampleInfoBuilderOptions options;

    public SampleInfoBuilder(SampleInfoBuilderOptions options)
    {
      this.options = options;
      this.AcceptGsmName = m => true;
    }

    public Func<string, bool> AcceptGsmName { get; set; }

    public override IEnumerable<string> Process()
    {
      var properties = SampleUtils.ReadPropertiesFromFile(options.PropertyFile);
      var format = new SampleItemTextWriter(properties);

      List<SampleItem> total = new List<SampleItem>();
      foreach (var subdir in options.SampleDirectories())
      {
        Progress.SetMessage("processing " + subdir + " ...");

        var dirname = Path.GetFileName(subdir);
        var sifile = Path.Combine(subdir, dirname + ".siformat");
        var parser = new SampleItemParser(sifile);

        var flist = parser.ParseDataset(subdir);
        var sampleFile = Path.ChangeExtension(sifile, ".sample");
        format.WriteToFile(sampleFile, flist.Values.ToList());

        total.AddRange(from l in flist
                       where AcceptGsmName(l.Key)
                       select l.Value);
      }

      format.WriteToFile(options.OutputFile, total);

      var htmlFile = Path.ChangeExtension(options.OutputFile, ".html");
      new SampleItemHtmlWriter(properties).WriteToFile(htmlFile, total);

      var excelFile = Path.ChangeExtension(htmlFile, ".xls");
      new SampleItemExcelWriter(properties).WriteToFile(excelFile, total);

      return new string[] { options.OutputFile, htmlFile, excelFile };
    }
  }
}
