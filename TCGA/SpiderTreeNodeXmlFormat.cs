using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RCPA;
using System.Xml.Linq;

namespace CQS.TCGA
{

  public class SpiderTreeNodeXmlFormat : IFileFormat<SpiderTreeNode>
  {
    public SpiderTreeNode ReadFromFile(string fileName)
    {
      XElement root = XElement.Load(fileName);

      var result = ElementToNode(root, 1);
      result.AssignParent();

      return result;
    }

    private XElement NodeToElement(SpiderTreeNode t)
    {
      return new XElement("Node",
        new XElement("Name", t.Name),
        new XElement("Uri", t.Uri),
        new XElement("LastModified", t.LastModified),
        from node in t.Nodes select NodeToElement(node));
    }

    private SpiderTreeNode ElementToNode(XElement t, int depth)
    {
      var result = new SpiderTreeNode();

      result.Name = t.Element("Name").Value;
      result.Uri = t.Element("Uri").Value;
      if (t.Element("LastModified") != null)
      {
        result.LastModified = t.Element("LastModified").Value;
      }
      result.Depth = depth;
      foreach (var ele in t.Elements("Node"))
      {
        result.Nodes.Add(ElementToNode(ele, depth + 1));
      }

      return result;
    }

    public void WriteToFile(string fileName, SpiderTreeNode t)
    {
      XElement root = NodeToElement(t);
      root.Save(fileName);
    }
  }
}
