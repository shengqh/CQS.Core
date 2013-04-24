using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;

namespace CQS.TCGA
{
  public class SpiderTreeNode
  {
    public SpiderTreeNode()
    {
      this.Name = string.Empty;
      this.LastModified = string.Empty;
      this.Uri = string.Empty;
      this.Depth = 0;
      this.Parent = null;
      this.Nodes = new List<SpiderTreeNode>();
      this.IsPreviousVersion = false;
    }

    public string Name { get; set; }
    public string LastModified { get; set; }
    public string Uri { get; set; }
    public int Depth { get; set; }
    public SpiderTreeNode Parent { get; set; }
    public List<SpiderTreeNode> Nodes { get; private set; }
    public bool IsPreviousVersion { get; set; }

    public bool NameContains(string substr)
    {
      return this.Name.Contains(substr) || this.Nodes.Any(m => m.NameContains(substr));
    }

    public bool NameEquals(string name)
    {
      return this.Name.Equals(name) || this.Nodes.Any(m => m.NameEquals(name));
    }

    public void AssignParent()
    {
      this.Nodes.ForEach(m =>
      {
        m.Parent = this;
        m.AssignParent();
      });
    }

    public List<SpiderTreeNode> FindNode(Func<SpiderTreeNode, bool> filter)
    {
      List<SpiderTreeNode> result = new List<SpiderTreeNode>();

      if (filter(this))
      {
        result.Add(this);
      }

      foreach (var node in this.Nodes)
      {
        result.AddRange(node.FindNode(filter));
      }

      return result;
    }

    public List<SpiderTreeNode> FindDeepestNode(Func<SpiderTreeNode, bool> filter)
    {
      List<SpiderTreeNode> result = new List<SpiderTreeNode>();

      bool bFound = false;
      foreach (var node in this.Nodes)
      {
        var found = node.FindDeepestNode(filter);
        if (found.Count > 0)
        {
          bFound = true;
          result.AddRange(found);
        }
      }

      if (filter(this) && !bFound)
      {
        result.Add(this);
      }

      return result;
    }

    /// <summary>
    /// Mark highest version, used to filter deepest directory which contains actual data
    /// </summary>
    /// <param name="nodes"></param>
    public void MarkHighestVersionNodes()
    {
      var pattern = new Regex(@"(.+\.\d+)\.(\d+)\.\d+$");
      if (!this.Nodes.All(m => pattern.Match(m.Name).Success))
      {
        return;
      }

      var grp = this.Nodes.GroupBy(m => pattern.Match(m.Name).Groups[1].Value).ToList();
      var kept = new HashSet<SpiderTreeNode>();
      foreach (var g in grp)
      {
        kept.Add((from v in g
                  orderby int.Parse(pattern.Match(v.Name).Groups[2].Value) descending
                  select v).First());
      }

      this.Nodes.ForEach(m => m.IsPreviousVersion = !kept.Contains(m));
    }

    public override string ToString()
    {
      return this.Name;
    }
  }

  public static class SpliderTreeNodeExtension
  {
    public static void Print(this SpiderTreeNode node, TextWriter writer)
    {
      for (int i = 1; i < node.Depth; i++)
      {
        writer.Write("->");
      }
      writer.WriteLine("{0}:[{1}]", node.Name, node.LastModified);
      foreach (var subnode in node.Nodes)
      {
        subnode.Print(writer);
      }
    }

    public static void PrintToFile(this SpiderTreeNode node, string fileName)
    {
      using (StreamWriter sw = new StreamWriter(fileName))
      {
        node.Print(sw);
      }
    }
  }

}
