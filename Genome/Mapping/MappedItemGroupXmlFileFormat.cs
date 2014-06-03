using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using CQS.Genome.Sam;
using RCPA;

namespace CQS.Genome.Mapping
{
  public class MappedItemGroupXmlFileFormat : IFileFormat<List<MappedItemGroup>>
  {
    public List<MappedItemGroup> ReadFromFile(string fileName)
    {
      Console.WriteLine("read file {0} ...", fileName);
      var result = new List<MappedItemGroup>();

      XElement root = XElement.Load(fileName);

      //Console.WriteLine("read locations ...");
      Dictionary<string, SAMAlignedLocation> qmmap = root.ToSAMAlignedLocationMap();

      //Console.WriteLine("read mapped items ...");
      foreach (XElement groupEle in root.Element("subjectResult").Elements("subjectGroup"))
      {
        var group = new MappedItemGroup();
        result.Add(group);

        foreach (XElement mirnaEle in groupEle.Elements("subject"))
        {
          var mirna = new MappedItem();
          group.Add(mirna);
          mirna.Name = mirnaEle.Attribute("name").Value;

          foreach (XElement regionEle in mirnaEle.Elements("region"))
          {
            var region = new SequenceRegionMapped();
            mirna.MappedRegions.Add(region);

            region.Region.Name = mirna.Name;
            region.Region.ParseLocation(regionEle);

            if (regionEle.Attribute("sequence") != null)
            {
              region.Region.Sequence = regionEle.Attribute("sequence").Value;
            }

            foreach (XElement queryEle in regionEle.Elements("query"))
            {
              string qname = queryEle.Attribute("qname").Value;
              string loc = queryEle.Attribute("loc").Value;
              string key = SAMAlignedLocation.GetKey(qname, loc);
              SAMAlignedLocation query = qmmap[key];
              region.AlignedLocations.Add(query);
              query.Features.Add(region.Region);
            }
          }
        }
      }
      qmmap.Clear();

      return result;
    }

    public void WriteToFile(string fileName, List<MappedItemGroup> groups)
    {
      List<SAMAlignedItem> queries = groups.GetQueries();

      var xml = new XElement("root",
        queries.ToXElement(),
        new XElement("subjectResult",
          from itemgroup in groups
          select new XElement("subjectGroup",
            from item in itemgroup
            select new XElement("subject",
              new XAttribute("name", item.Name),
              from region in item.MappedRegions
              select new XElement("region",
                new XAttribute("seqname", region.Region.Seqname),
                new XAttribute("start", region.Region.Start),
                new XAttribute("end", region.Region.End),
                new XAttribute("strand", region.Region.Strand),
                new XAttribute("sequence", XmlUtils.ToXml(region.Region.Sequence)),
                from loc in region.AlignedLocations
                select new XElement("query",
                  new XAttribute("qname", loc.Parent.Qname),
                  new XAttribute("loc", loc.GetLocation())))))));
      xml.Save(fileName);
    }
  }
}