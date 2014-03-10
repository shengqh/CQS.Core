using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RCPA;
using System.IO;
using CQS.Commandline;
using CommandLine;

namespace CQS.Genome.Gtf
{
  public class GtfGeneIdGeneNameMapBuilder : AbstractThreadProcessor
  {
    private GtfGeneIdGeneNameMapBuilderOptions options;

    public GtfGeneIdGeneNameMapBuilder(GtfGeneIdGeneNameMapBuilderOptions options)
    {
      this.options = options;
    }

    public override IEnumerable<string> Process()
    {
      Dictionary<string, string> map = new Dictionary<string, string>();

      using (var gtf = new GtfItemFile(options.InputFile))
      {
        GtfItem item;
        int count = 0;
        while ((item = gtf.Next()) != null)
        {
          count++;
          if ((count % 100000) == 0)
          {
            Progress.SetMessage("{0} gtf item processed", count);
          }
          map[item.GeneId] = item.Attributes.StringAfter("gene_name \"").StringBefore("\"");
        }
      }

      var keys = (from key in map.Keys
                  orderby key
                  select key).ToList();

      using (StreamWriter sw = new StreamWriter(options.OutputFile))
      {
        sw.WriteLine("gene_id\tgene_name");
        foreach (var key in keys)
        {
          sw.WriteLine("{0}\t{1}", key, map[key]);
        }
      }

      return new string[] { options.OutputFile };
    }
  }
}
