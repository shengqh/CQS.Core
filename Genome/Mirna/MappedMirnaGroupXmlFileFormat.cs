using CQS.Genome.Sam;
using RCPA;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace CQS.Genome.Mirna
{
  public class MappedMirnaGroupXmlFileFormat : IFileFormat<List<MappedMirnaGroup>>
  {
    public MappedMirnaGroupXmlFileFormat() { }

    public List<MappedMirnaGroup> ReadFromFile(string fileName)
    {
      List<MappedMirnaGroup> result = new List<MappedMirnaGroup>();

      XElement root = XElement.Load(fileName);
      var qmmap = root.ToSAMAlignedItems().ToSAMAlignedLocationMap();

      foreach (var mirnaGroupEle in root.Element("mirnas").Elements("mirnagroup"))
      {
        var group = new MappedMirnaGroup();
        result.Add(group);

        foreach (var mirnaEle in mirnaGroupEle.Elements("mirna"))
        {
          var mirna = new MappedMirna();
          group.Add(mirna);
          mirna.Name = mirnaEle.Attribute("name").Value;
          mirna.Sequence = mirnaEle.Attribute("sequence").Value;

          foreach (var regionEle in mirnaEle.Elements("region"))
          {
            var region = new MappedMirnaRegion();
            mirna.MappedRegions.Add(region);

            region.Region.Name = mirna.Name;
            ParseLocation(regionEle, region.Region);

            foreach (var posEle in regionEle.Elements("position"))
            {
              var position = new SequenceRegionMapped();
              position.Region = region.Region;
              position.Offset = int.Parse(posEle.Attribute("offset").Value);
              region.Mapped[position.Offset] = position;

              foreach (var queryEle in posEle.Elements("query"))
              {
                var qname = queryEle.Attribute("qname").Value;
                var loc = queryEle.Attribute("loc").Value;
                var key = GenerateKey(qname, loc);
                var query = qmmap[key];
                position.AlignedLocations.Add(query);
                query.Features.Add(region.Region);
              }
            }
          }
        }
      }
      qmmap.Clear();

      return result;
    }

    private string GenerateKey(string qname, string loc)
    {
      return string.Format("{0}-{1}", qname, loc);
    }

    private static void ParseLocation(XElement locEle, ISequenceRegion loc)
    {
      loc.Seqname = locEle.Attribute("seqname").Value;
      loc.Start = long.Parse(locEle.Attribute("start").Value);
      loc.End = long.Parse(locEle.Attribute("end").Value);
      loc.Strand = locEle.Attribute("strand").Value[0];
    }

    public void WriteToFile(string fileName, List<MappedMirnaGroup> mirnas)
    {
      var queries = (from mirna in mirnas
                     from item in mirna.GetAlignedLocations()
                     select item.Parent).Distinct().OrderBy(m => m.Qname).ToList();

      var xml = new XElement("root",
        queries.ToXElement(),
        new XElement("mirnas",
          from mirnagroup in mirnas
          select new XElement("mirnagroup",
            from mirna in mirnagroup
            select new XElement("mirna",
              new XAttribute("name", mirna.Name),
              new XAttribute("sequence", mirna.Sequence),
                from region in mirna.MappedRegions
                select new XElement("region",
                  new XAttribute("seqname", region.Region.Seqname),
                  new XAttribute("start", region.Region.Start),
                  new XAttribute("end", region.Region.End),
                  new XAttribute("strand", region.Region.Strand),
                  from mapped in region.Mapped.Values
                  where mapped.AlignedLocations.Count > 0
                  orderby mapped.Offset
                  select new XElement("position",
                    new XAttribute("offset", mapped.Offset),
                    from item in mapped.AlignedLocations
                    select new XElement("query",
                      new XAttribute("qname", item.Parent.Qname),
                      new XAttribute("loc", item.GetLocation()),
                      new XAttribute("querycount", item.Parent.QueryCount))))))));
      xml.Save(fileName);
    }
  }
}