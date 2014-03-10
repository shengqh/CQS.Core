using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using CQS.Genome.Cuffdiff;
using LumenWorks.Framework.IO.Csv;
using CQS.Genome.Affymetrix;
using RCPA;

namespace CQS.Genome.Cuffdiff
{
  public class CuffdiffSignificantFileMerger : AbstractThreadFileProcessor
  {
    private string affyAnnotationFile;

    private IList<string> significantFiles;

    public CuffdiffSignificantFileMerger(string affyAnnotationFile, IList<string> significantFiles)
    {
      this.affyAnnotationFile = affyAnnotationFile;
      this.significantFiles = significantFiles;
    }

    private static Dictionary<string, CuffdiffItem> ReadGeneDirectionMap(string file)
    {
      var items = (from line in File.ReadAllLines(file).Skip(1)
                   select CuffdiffItem.Parse(line)).ToList();

      var result = items.ToDictionary(m => m.GeneId);

      return result;
    }

    public override IEnumerable<string> Process(string fileName)
    {
      Dictionary<string, string> genenames = File.Exists(this.affyAnnotationFile) ? AnnotationFile.GetGeneSymbolDescriptionMap(this.affyAnnotationFile) : new Dictionary<string, string>();

      var map = (from file in this.significantFiles
                 select ReadGeneDirectionMap(file)).ToDictionary(m => m.Values.First());

      using (StreamWriter sw = new StreamWriter(fileName))
      {
        sw.Write("test_id\tgene_id\tgene\tdescription\tlocus");
        foreach (var item in map.Keys)
        {
          var comp = string.Format("({0}/{1})", item.Sample2, item.Sample1);
          sw.Write("\t{0}\t{1}\tstatus{2}\tlog2FoldChange{2}\tpvalue{2}\tqvalue{2}\tsignificant{2}",
            item.Sample1, item.Sample2, comp);
        }
        sw.WriteLine();

        var keys = (from k in map.First().Value.Keys
                    orderby k
                    select k).ToList();

        foreach (var key in keys)
        {
          var v = map.First().Value[key];
          var titles = (from a in v.Gene.Split(',')
                        let b = a.Trim()
                        let t = genenames.ContainsKey(b) ? genenames[b] : string.Empty
                        select t).ToList();
          var tt = titles.All(l => l.Equals(string.Empty)) ? string.Empty : titles.Merge("/");


          sw.Write("{0}\t{1}\t{2}\t{3}\t{4}",
            v.TestId,
            v.GeneId,
            v.Gene,
            tt,
            v.Locus);

          foreach (var mv in map.Values)
          {
            var vv = mv[key];
            sw.Write("\t{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}",
              vv.Value1,
              vv.Value2,
              vv.Status,
              vv.Log2FoldChangeString,
              vv.PValue,
              vv.QValue,
              vv.SignificantString);
          }
          sw.WriteLine();
        }
      }
      return new string[] { fileName };
    }
  }
}
