using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RCPA;
using System.IO;
using System.Text.RegularExpressions;
using CQS.Genome.Gtf;
using CQS.Genome.Sam;
using System.Xml.Linq;

namespace CQS.Genome
{
  public class SequenceRegionMappedXmlFileFormat : IFileFormat<List<SequenceRegionMapped>>
  {
    public SequenceRegionMappedXmlFileFormat() { }

    public List<SequenceRegionMapped> ReadFromFile(string fileName)
    {
      List<SequenceRegionMapped> result = new List<SequenceRegionMapped>();

      XElement root = XElement.Load(fileName);

      var qmmap = root.ToSAMAlignedItems().ToSAMAlignedLocationMap();

      foreach (var regionEle in root.Element("regions").Elements("region"))
      {
        var position = new SequenceRegionMapped();
        result.Add(position);

        position.Region = new SequenceRegion();
        position.Region.Name = regionEle.Attribute("name").Value;
        position.Region.ParseLocation(regionEle);
        foreach (var queryEle in regionEle.Elements("query"))
        {
          var qname = queryEle.Attribute("qname").Value;
          var loc = queryEle.Attribute("loc").Value;
          var key = SAMAlignedLocation.GetKey(qname, loc);
          var query = qmmap[key];
          position.AlignedLocations.Add(query);
          query.Features.Add(position.Region);
        }
      }

      qmmap.Clear();

      return result;
    }

    public void WriteToFile(string fileName, List<SequenceRegionMapped> mapped)
    {
      var queries = (from curmapped in mapped
                     from item in curmapped.AlignedLocations
                     select item.Parent).Distinct().OrderBy(m => m.Qname).ToList();

      var xml = new XElement("root",
        SAMAlignedItemToElement(queries),
        new XElement("regions",
          from region in mapped
          where region.AlignedLocations.Count > 0
          select new XElement("region",
            new XAttribute("name", region.Region.Name),
            new XAttribute("seqname", region.Region.Seqname),
            new XAttribute("start", region.Region.Start),
            new XAttribute("end", region.Region.End),
            new XAttribute("strand", region.Region.Strand),
            from loc in region.AlignedLocations
            select new XElement("query",
              new XAttribute("qname", loc.Parent.Qname),
              new XAttribute("loc", loc.GetLocation())))));
      xml.Save(fileName);
    }

    private static XElement SAMAlignedItemToElement(List<SAMAlignedItem> queries)
    {
      return new XElement("queries",
                from q in queries
                select new XElement("query",
                  new XAttribute("name", q.Qname),
                  new XAttribute("sequence", q.Sequence),
                  new XAttribute("count", q.QueryCount),
                  from l in q.Locations
                  select new XElement("location",
                    new XAttribute("seqname", l.Seqname),
                    new XAttribute("start", l.Start),
                    new XAttribute("end", l.End),
                    new XAttribute("strand", l.Strand),
                    new XAttribute("cigar", l.Cigar),
                    new XAttribute("score", l.AlignmentScore),
                    new XAttribute("mdz", l.MismatchPositions),
                    new XAttribute("nmi", l.NumberOfMismatch))));
    }
  }
}