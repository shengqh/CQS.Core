using CQS.Genome.Sam;
using RCPA;
using RCPA.Gui;
using RCPA.Utils;
using System.Collections.Generic;
using System.Xml;

namespace CQS.Genome.Feature
{
  public class FeatureItemGroupXmlFormat : ProgressClass, IFileFormat<List<FeatureItemGroup>>
  {
    private bool exportPValue;

    public FeatureItemGroupXmlFormat(bool exportPValue = false)
    {
      this.exportPValue = exportPValue;
    }

    public List<FeatureItemGroup> ReadFromFile(string fileName)
    {
      var result = new List<FeatureItemGroup>();

      using (XmlReader source = XmlReader.Create(fileName))
      {
        Progress.SetMessage("reading queries ...");

        List<SAMAlignedItem> queries = SAMAlignedItemUtils.ReadFrom(source);

        Progress.SetMessage("{0} queries read.", queries.Count);

        var qmmap = queries.ToSAMAlignedLocationMap();
        queries.Clear();

        Progress.SetMessage("reading subjects ...");
        string value;
        source.ReadToFollowing("subjectResult");
        if (source.ReadToDescendant("subjectGroup"))
        {
          do
          {
            var featureGroup = new FeatureItemGroup();
            result.Add(featureGroup);

            if (source.ReadToDescendant("subject"))
            {
              do
              {
                var item = new FeatureItem();
                featureGroup.Add(item);
                item.Name = source.GetAttribute("name");

                if (source.ReadToDescendant("region"))
                {
                  do
                  {
                    var fl = new FeatureLocation();
                    item.Locations.Add(fl);

                    fl.Name = item.Name;
                    fl.Seqname = source.GetAttribute("seqname");
                    fl.Start = long.Parse(source.GetAttribute("start"));
                    fl.End = long.Parse(source.GetAttribute("end"));
                    fl.Strand = source.GetAttribute("strand")[0];
                    fl.Sequence = source.GetAttribute("sequence");

                    value = source.GetAttribute("query_count_before_filter");
                    if (value != null)
                    {
                      fl.QueryCountBeforeFilter = int.Parse(value);
                    }

                    value = source.GetAttribute("pvalue");
                    if (value != null)
                    {
                      fl.PValue = double.Parse(value);
                    }

                    if (source.ReadToDescendant("query"))
                    {
                      do
                      {
                        string qname = source.GetAttribute("qname");
                        string loc = source.GetAttribute("loc");
                        string key = SAMAlignedLocation.GetKey(qname, loc);
                        SAMAlignedLocation query = qmmap[key];

                        FeatureSamLocation fsl = new FeatureSamLocation(fl);
                        fsl.SamLocation = query;

                        fsl.Offset = int.Parse(source.GetAttribute("offset"));

                        var attr = source.GetAttribute("overlap");
                        if (attr == null)
                        {
                          fsl.OverlapPercentage = query.OverlapPercentage(fl);
                        }
                        else
                        {
                          fsl.OverlapPercentage = double.Parse(attr);
                        }

                        var nmi = source.GetAttribute("nmi");
                        if (nmi != null)
                        {
                          fsl.NumberOfMismatch = int.Parse(nmi);
                        }

                        var nnpm = source.GetAttribute("nnpm");
                        if (nnpm != null)
                        {
                          fsl.NumberOfNoPenaltyMutation = int.Parse(nnpm);
                        }
                      } while (source.ReadToNextSibling("query"));
                    }
                  } while (source.ReadToNextSibling("region"));
                }
              } while (source.ReadToNextSibling("subject"));
            }
          } while (source.ReadToNextSibling("subjectGroup"));
        }
        qmmap.Clear();
      }

      Progress.SetMessage("{0} subjects read.", result.Count);
      return result;
    }


    public void WriteToFile(string fileName, List<FeatureItemGroup> groups)
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
          foreach (var item in itemgroup)
          {
            xw.WriteStartElement("subject");
            xw.WriteAttribute("name", item.Name);
            foreach (var region in item.Locations)
            {
              xw.WriteStartElement("region");
              xw.WriteAttribute("seqname", region.Seqname);
              xw.WriteAttribute("start", region.Start);
              xw.WriteAttribute("end", region.End);
              xw.WriteAttribute("strand", region.Strand);
              xw.WriteAttribute("sequence", XmlUtils.ToXml(region.Sequence));
              if (this.exportPValue)
              {
                xw.WriteAttribute("query_count_before_filter", region.QueryCountBeforeFilter);
                xw.WriteAttribute("query_count", region.QueryCount);
                xw.WriteAttribute("pvalue", region.PValue);
              }
              xw.WriteAttribute("size", region.Length);
              foreach (var sl in region.SamLocations)
              {
                var loc = sl.SamLocation;
                xw.WriteStartElement("query");
                xw.WriteAttribute("qname", loc.Parent.Qname);
                xw.WriteAttribute("loc", loc.GetLocation());
                xw.WriteAttribute("overlap", string.Format("{0:0.##}", sl.OverlapPercentage));
                xw.WriteAttribute("offset", sl.Offset);
                xw.WriteAttribute("query_count", loc.Parent.QueryCount);
                xw.WriteAttribute("seq_len", loc.Parent.Sequence.Length);
                xw.WriteAttribute("nmi", loc.NumberOfMismatch);
                xw.WriteAttribute("nnpm", loc.NumberOfNoPenaltyMutation);
                xw.WriteEndElement();
              }
              xw.WriteEndElement();
            }
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