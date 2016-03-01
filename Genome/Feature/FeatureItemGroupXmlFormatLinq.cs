using CQS.Genome.Sam;
using RCPA;
using RCPA.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace CQS.Genome.Feature
{
  public class FeatureItemGroupXmlFormatLinq : IFileFormat<List<FeatureItemGroup>>
  {
    private bool exportPValue;

    public FeatureItemGroupXmlFormatLinq(bool exportPValue = false)
    {
      this.exportPValue = exportPValue;
    }

    public List<FeatureItemGroup> ReadFromFile(string fileName)
    {
      Console.WriteLine("read file {0} ...", fileName);
      var result = new List<FeatureItemGroup>();

      XElement root = XElement.Load(fileName);

      //Console.WriteLine("read locations ...");
      Dictionary<string, SAMAlignedLocation> qmmap = root.ToSAMAlignedItems().ToSAMAlignedLocationMap();

      //Console.WriteLine("read mapped items ...");
      foreach (XElement groupEle in root.Element("subjectResult").Elements("subjectGroup"))
      {
        var group = new FeatureItemGroup();
        result.Add(group);

        foreach (XElement featureEle in groupEle.Elements("subject"))
        {
          var item = new FeatureItem();
          group.Add(item);
          item.Name = featureEle.Attribute("name").Value;

          foreach (XElement locEle in featureEle.Elements("region"))
          {
            var fl = new FeatureLocation();
            item.Locations.Add(fl);

            fl.Name = item.Name;
            fl.ParseLocation(locEle);

            if (locEle.Attribute("sequence") != null)
            {
              fl.Sequence = locEle.Attribute("sequence").Value;
            }

            if (locEle.Attribute("query_count_before_filter") != null)
            {
              fl.QueryCountBeforeFilter = int.Parse(locEle.Attribute("query_count_before_filter").Value);
            }

            if (locEle.Attribute("pvalue") != null)
            {
              fl.PValue = double.Parse(locEle.Attribute("pvalue").Value);
            }

            foreach (XElement queryEle in locEle.Elements("query"))
            {
              string qname = queryEle.Attribute("qname").Value;
              string loc = queryEle.Attribute("loc").Value;
              string key = SAMAlignedLocation.GetKey(qname, loc);
              SAMAlignedLocation query = qmmap[key];

              FeatureSamLocation fsl = new FeatureSamLocation(fl);
              fsl.SamLocation = query;
              var attr = queryEle.FindAttribute("overlap");
              if (attr == null)
              {
                fsl.OverlapPercentage = query.OverlapPercentage(fl);
              }
              else
              {
                fsl.OverlapPercentage = double.Parse(attr.Value);
              }

              var nnpm = queryEle.FindAttribute("nnpm");
              if (nnpm == null)
              {
                nnpm = queryEle.FindAttribute("nnmp");
              }
              if (nnpm != null)
              {
                fsl.NumberOfNoPenaltyMutation = int.Parse(nnpm.Value);
              }

              var nmi = queryEle.FindAttribute("nmi");
              if (nmi != null)
              {
                fsl.NumberOfMismatch = int.Parse(nmi.Value);
              }
            }
          }
        }
      }
      qmmap.Clear();

      return result;
    }

    public void WriteToFile(string fileName, List<FeatureItemGroup> groups)
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
              from region in item.Locations
              select new XElement("region",
                new XAttribute("seqname", region.Seqname),
                new XAttribute("start", region.Start),
                new XAttribute("end", region.End),
                new XAttribute("strand", region.Strand),
                new XAttribute("sequence", XmlUtils.ToXml(region.Sequence)),
                this.exportPValue ? new XAttribute("query_count_before_filter", region.QueryCountBeforeFilter) : null,
                this.exportPValue ? new XAttribute("query_count", region.QueryCount) : null,
                this.exportPValue ? new XAttribute("pvalue", region.PValue) : null,
                new XAttribute("size", region.Length),
                from sl in region.SamLocations
                let loc = sl.SamLocation
                select new XElement("query",
                  new XAttribute("qname", loc.Parent.Qname),
                  new XAttribute("loc", loc.GetLocation()),
                  new XAttribute("overlap", string.Format("{0:0.##}", sl.OverlapPercentage)),
                  new XAttribute("offset", sl.Offset),
                  new XAttribute("query_count", loc.Parent.QueryCount),
                  new XAttribute("seq_len", loc.Parent.Sequence.Length),
                  new XAttribute("nmi", loc.NumberOfMismatch),
                  new XAttribute("nnpm", loc.NumberOfNoPenaltyMutation)
                  ))))));

      using (var sw = new StreamWriter(fileName))
      {
        sw.NewLine = Environment.NewLine;
        xml.Save(sw);
      }
    }
  }
}