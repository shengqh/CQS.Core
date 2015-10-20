using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using CQS.Genome.Sam;
using RCPA;
using System.Xml;
using System.Text;

namespace CQS.Genome.Mapping
{
  public class ChromosomeCountSlimXmlReader : IFileReader<List<ChromosomeCountSlimItem>>
  {
    public ChromosomeCountSlimXmlReader()
    { }

    public List<ChromosomeCountSlimItem> ReadFromFile(string fileName)
    {
      var result = new List<ChromosomeCountSlimItem>();

      XElement root = XElement.Load(fileName);

      Console.WriteLine("read queries ...");

      var queries = new List<SAMChromosomeItem>();
      foreach (XElement qEle in root.Element("queries").Elements("query"))
      {
        var query = new SAMChromosomeItem();
        queries.Add(query);

        query.Qname = qEle.Attribute("name").Value;
        query.QueryCount = int.Parse(qEle.Attribute("count").Value);

        foreach (var loc in qEle.Elements("location"))
        {
          var seqname = loc.Attribute("seqname").Value;
          if (!query.Chromosomes.Contains(seqname))
          {
            query.Chromosomes.Add(seqname);
          }
        }
      }

      Console.WriteLine("{0} queries read.", queries.Count);

      var qmmap = queries.ToDictionary(m => m.Qname);
      queries.Clear();

      Console.WriteLine("read chromosomes ...");
      foreach (XElement groupEle in root.Element("subjectResult").Elements("subjectGroup"))
      {
        var item = new ChromosomeCountSlimItem();
        result.Add(item);

        foreach (XElement mirnaEle in groupEle.Elements("subject"))
        {
          item.Names.Add(mirnaEle.Attribute("name").Value);
        }

        foreach (XElement queryEle in groupEle.Elements("query"))
        {
          var q = qmmap[queryEle.Attribute("qname").Value];
          //var slim = new SAMChromosomeItem()
          //{
          //  Qname = q.Qname,
          //  QueryCount = q.QueryCount,
          //  Chromosomes = (from loc in q.Locations
          //                 select loc.Seqname).Distinct().OrderBy(m => m).ToList()
          //};
          //item.Queries.Add(slim);
          item.Queries.Add(q);
        }
      }
      qmmap.Clear();

      Console.WriteLine("{0} subject group read.", result.Count);

      return result;
    }
  }
}