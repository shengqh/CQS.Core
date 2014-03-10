using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace CQS.FileTemplate
{
  public class HeaderDefinition
  {
    public HeaderDefinition()
    {
      this.Name = "";
      this.Properties = new List<string>();
    }

    public string Name { get; set; }

    public List<string> Properties { get; set; }

    public override string ToString()
    {
      return this.Name;
    }

    public void Load(string fileName)
    {
      var root = XElement.Load(fileName);
      this.Name = root.Element("Name").Value;
      var headers = new List<string>();
      foreach (var line in root.Elements("Property"))
      {
        var lt = line.Value.Trim();
        if (string.IsNullOrEmpty(lt) || headers.Contains(lt))
        {
          continue;
        }
        headers.Add(lt);
      }
      this.Properties = headers;
    }

    public void Save(string fileName)
    {
      var root = new XElement("FileTemplate",
        new XElement("Name", this.Name),
        from prop in this.Properties
        select new XElement("Property", prop));
      root.Save(fileName);
    }

    public static HeaderDefinition LoadFromFile(string fileName)
    {
      var result = new HeaderDefinition();
      result.Load(fileName);
      return result;
    }
  }
}
