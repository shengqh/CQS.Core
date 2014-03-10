using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RCPA;
using System.IO;

namespace CQS.TCGA
{
  public class TCGATreeBuilder : AbstractThreadProcessor
  {
    private TCGATreeBuilderOptions options;

    public TCGATreeBuilder(TCGATreeBuilderOptions options)
    {
      this.options = options;
    }

    public override IEnumerable<string> Process()
    {
      Progress.SetMessage("Getting tree from TCGA ...");

      var node = TCGASpider.GetDirectoryTree("tumor", TCGASpider.RootUrl, true);

      var name = string.Format("TCGA_directory_{0:yyyyMMdd}", DateTime.Now);

      var treeFile = Path.Combine(options.OutputDirectory, name + ".tree");
      node.PrintToFile(treeFile);

      var fileName = Path.Combine(options.OutputDirectory, name + ".xml");
      Progress.SetMessage("Saving data to " + fileName + " ...");
      new SpiderTreeNodeXmlFormat().WriteToFile(fileName, node);

      Progress.SetMessage("Done.");
      Progress.End();

      return new[] { fileName, treeFile };
    }
  }
}
