using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using CQS.Genome.Sam;
using RCPA;

namespace CQS.Genome.Mapping
{
  public class ChromosomeCountXmFormat : IFileFormat<List<ChromosomeCountItem>>
  {
    public ChromosomeCountXmFormat()
    { }

    public List<ChromosomeCountItem> ReadFromFile(string fileName)
    {
      Console.WriteLine("read file {0} ...", fileName);
      var result = new List<ChromosomeCountItem>();

      XElement root = XElement.Load(fileName);

      //Console.WriteLine("read locations ...");
      var qmmap = root.ToSAMAlignedItemMap();

      //Console.WriteLine("read mapped items ...");
      foreach (XElement groupEle in root.Element("subjectResult").Elements("subjectGroup"))
      {
        var item = new ChromosomeCountItem();
        result.Add(item);

        foreach (XElement mirnaEle in groupEle.Elements("subject"))
        {
          item.Names.Add(mirnaEle.Attribute("name").Value);
        }

        foreach (XElement queryEle in groupEle.Elements("query"))
        {
          item.Queries.Add(qmmap[queryEle.Attribute("qname").Value]);
        }
      }
      qmmap.Clear();

      return result;
    }

    public void WriteToFile(string fileName, List<ChromosomeCountItem> groups)
    {
      List<SAMAlignedItem> queries = groups.GetQueries();

      var xml = new XElement("root",
        queries.ToXElement(),
        new XElement("subjectResult",
          from itemgroup in groups
          select new XElement("subjectGroup",
            from item in itemgroup.Names select new XElement("subject", new XAttribute("name", item)),
            from loc in itemgroup.Queries select new XElement("query", new XAttribute("qname", loc.Qname)))));
      xml.Save(fileName);
    }
  }
}