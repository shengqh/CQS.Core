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
  public class ChromosomeCountXmlFormat : IFileFormat<List<ChromosomeCountItem>>
  {
    public ChromosomeCountXmlFormat()
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
      using (var xw = new XmlTextWriter(fileName, Encoding.UTF8))
      {
        xw.Formatting = Formatting.Indented;
        xw.WriteStartDocument();

        xw.WriteStartElement("root");
        groups.GetQueries().WriteTo(xw);
        xw.WriteStartElement("subjectResult");
        foreach (var itemgroup in groups)
        {
          xw.WriteStartElement("subjectGroup");

          foreach (var name in itemgroup.Names)
          {
            xw.WriteStartElement("subject");
            xw.WriteAttributeString("name", name);
            xw.WriteEndElement();
          }
          foreach (var loc in itemgroup.Queries)
          {
            xw.WriteStartElement("query");
            xw.WriteAttributeString("qname", loc.Qname);
            xw.WriteEndElement();
          }

          xw.WriteEndElement();
        }
        xw.WriteEndElement();
        xw.WriteEndElement();

        xw.WriteEndDocument();
        xw.Close();
      }
    }

    public void WriteToFileSlow(string fileName, List<ChromosomeCountItem> groups)
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