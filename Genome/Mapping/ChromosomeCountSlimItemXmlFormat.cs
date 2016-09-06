using RCPA;
using RCPA.Gui;
using RCPA.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace CQS.Genome.Mapping
{
  public class ChromosomeCountSlimItemXmlFormat : ProgressClass, IFileFormat<List<ChromosomeCountSlimItem>>
  {
    private bool outputSample;
    public ChromosomeCountSlimItemXmlFormat(bool outputSample = false, bool showProgress = true)
    {
      this.outputSample = outputSample;

      if (!showProgress)
      {
        this.Progress = new EmptyProgressCallback();
      }
    }

    public List<ChromosomeCountSlimItem> ReadFromFile(string fileName)
    {
      var result = new List<ChromosomeCountSlimItem>();

      using (XmlReader source = XmlReader.Create(fileName))
      {
        //Progress.SetMessage("read queries ...");
        var queries = new List<SAMChromosomeItem>();

        source.ReadToFollowing("queries");
        if (source.ReadToDescendant("query"))
        {
          do
          {
            var query = new SAMChromosomeItem();
            queries.Add(query);

            query.Qname = source.GetAttribute("name");
            query.QueryCount = int.Parse(source.GetAttribute("count"));
            var seqAtrr = source.GetAttribute("seq");
            if(seqAtrr != null)
            {
              query.Sequence = seqAtrr;
            }
            var sampleAtrr = source.GetAttribute("sample");
            if (sampleAtrr != null)
            {
              query.Sample = sampleAtrr;
            }
            if (source.ReadToDescendant("location"))
            {
              do
              {
                var seqname = source.GetAttribute("seqname");
                if (!query.Chromosomes.Contains(seqname))
                {
                  query.Chromosomes.Add(seqname);
                }
              } while (source.ReadToNextSibling("location"));
            }
          } while (source.ReadToNextSibling("query"));
        }

        Progress.SetMessage("{0} queries read.", queries.Count);

        var qmmap = queries.ToDictionary(m => m.Qname);
        queries.Clear();

        //Progress.SetMessage("read chromosomes ...");
        source.ReadToFollowing("subjectResult");
        ChromosomeCountSlimItem item = null;
        while (source.Read())
        {
          if (source.NodeType == XmlNodeType.Element)
          {
            if (source.Name.Equals("subjectGroup"))
            {
              item = new ChromosomeCountSlimItem();
              result.Add(item);
            }
            else if (source.Name.Equals("subject"))
            {
              item.Names.Add(source.GetAttribute("name"));
            }
            else if (source.Name.Equals("query"))
            {
              var q = qmmap[source.GetAttribute("qname")];
              item.Queries.Add(q);
            }
          }
        }
        qmmap.Clear();

        Progress.SetMessage("{0} subject groups read.", result.Count);

        result.ForEach(l =>
        {
          if (l.Names.Count > 1)
          {
            l.Names = l.Names.Distinct().ToList();
          }
        });

        return result;
      }
    }

    public void WriteToFile(string fileName, List<ChromosomeCountSlimItem> items)
    {
      using (var xw = XmlUtils.CreateWriter(fileName))
      {
        xw.WriteStartDocument();

        xw.WriteStartElement("root");

        var queries = items.GetQueries();

        xw.WriteStartElement("queries");
        foreach (var q in queries)
        {
          xw.WriteStartElement("query");
          xw.WriteAttributeString("name", q.Qname);
          if (!string.IsNullOrEmpty(q.Sequence))
          {
            xw.WriteAttributeString("seq", q.Sequence);
          }
          if (this.outputSample)
          {
            xw.WriteAttributeString("sample", q.Sample);
          }
          xw.WriteAttributeString("count", q.QueryCount.ToString());
          foreach (var l in q.Chromosomes)
          {
            xw.WriteStartElement("location");
            xw.WriteAttributeString("seqname", l);
            xw.WriteEndElement();
          }
          xw.WriteEndElement();
        }
        xw.WriteEndElement();

        xw.WriteStartElement("subjectResult");
        foreach (var itemgroup in items)
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