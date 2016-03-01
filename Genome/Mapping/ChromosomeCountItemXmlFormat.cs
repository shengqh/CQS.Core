using CQS.Genome.Sam;
using RCPA;
using RCPA.Utils;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace CQS.Genome.Mapping
{
  public class ChromosomeCountItemXmlFormat : IFileFormat<List<ChromosomeCountItem>>
  {
    public ChromosomeCountItemXmlFormat()
    { }

    public List<ChromosomeCountItem> ReadFromFile(string fileName)
    {
      var result = new List<ChromosomeCountItem>();

      using (XmlReader source = XmlReader.Create(fileName))
      {
        var queries = SAMAlignedItemUtils.ReadFrom(source).ToDictionary(m => m.Qname);

        source.ReadToFollowing("subjectResult");
        if (source.ReadToDescendant("subjectGroup"))
        {
          do
          {
            var item = new ChromosomeCountItem();
            result.Add(item);

            while (source.Read())
            {
              if (source.NodeType == XmlNodeType.EndElement && source.Name.Equals("subjectGroup"))
              {
                break;
              }

              if (source.NodeType == XmlNodeType.Element)
              {
                if (source.Name.Equals("subject"))
                {
                  item.Names.Add(source.GetAttribute("name"));
                }
                else if (source.Name.Equals("query"))
                {
                  item.Queries.Add(queries[source.GetAttribute("qname")]);
                }
              }
            }
          } while (source.ReadToNextSibling("subjectGroup"));
        }
      }

      return result;
    }

    public void WriteToFile(string fileName, List<ChromosomeCountItem> groups)
    {
      using (var xw = XmlUtils.CreateWriter(fileName))
      {
        xw.WriteStartDocument();

        xw.WriteStartElement("root");

        SAMAlignedItemUtils.WriteTo(xw, groups.GetQueries());

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
      }
    }
  }
}