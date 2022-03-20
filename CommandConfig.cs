using RCPA.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Linq;

namespace CQS
{
  public class ParameterConfig
  {
    public ParameterConfig(string name)
    {
      Name = name;
      Parameters = new Dictionary<string, string>();
    }

    public string Name { get; set; }

    public Dictionary<string, string> Parameters { get; set; }

    public string GetParameter(string name, string value)
    {
      if (!string.IsNullOrEmpty(value))
      {
        Parameters[name] = value;
        return value;
      }

      if (Parameters.ContainsKey(name))
      {
        return Parameters[name];
      }
      return null;
    }
  }

  public class ProgramConfig
  {
    public ProgramConfig()
    {
      Name = string.Empty;
      Command = string.Empty;
      ParameterSet = new Dictionary<string, ParameterConfig>();
      DefaultParameterSet = string.Empty;
    }

    public ProgramConfig(string name, string command)
    {
      Name = name;
      Command = command;
      ParameterSet = new Dictionary<string, ParameterConfig>();
      DefaultParameterSet = string.Empty;
    }

    public string Name { get; set; }

    public string Command { get; set; }

    public string DefaultParameterSet { get; set; }

    public Dictionary<string, ParameterConfig> ParameterSet { get; set; }

    public ParameterConfig FindOrCreate(string name)
    {
      if (!ParameterSet.ContainsKey(name))
      {
        ParameterSet[name] = new ParameterConfig(name);
      }
      return ParameterSet[name];
    }

    public bool HasParameterSet(string name)
    {
      return ParameterSet.ContainsKey(name);
    }
  }

  public class CommandConfig
  {
    private string _configFilename;

    public CommandConfig()
    {
      Programs = new Dictionary<string, ProgramConfig>();
    }

    public string ConfigFilename
    {
      get
      {
        if (string.IsNullOrEmpty(_configFilename))
        {
          if (SystemUtils.IsLinux)
          {
            _configFilename = Path.ChangeExtension(Application.ExecutablePath, ".linux");
          }
          else
          {
            _configFilename = Path.ChangeExtension(Application.ExecutablePath, ".win");
          }
        }
        return _configFilename;
      }
      set { _configFilename = value; }
    }

    public Dictionary<string, ProgramConfig> Programs { get; private set; }

    public bool Load()
    {
      return Load(ConfigFilename);
    }

    public bool Load(string filename)
    {
      try
      {
        if (File.Exists(filename))
        {
          XElement root = XElement.Load(filename);
          Programs = (from pro in root.Elements("program")
                      select new ProgramConfig(pro.Attribute("name").Value, pro.Attribute("command").Value)
                      {
                        DefaultParameterSet =
                          pro.Element("defaultparameterset") == null ? string.Empty : pro.Element("defaultparameterset").Value,
                        ParameterSet = (from paramset in pro.Elements("parameterset")
                                        select new ParameterConfig(paramset.Attribute("name").Value)
                                        {
                                          Parameters = (from param in paramset.Elements("parameter")
                                                        select
                                                          new KeyValuePair<string, string>(param.Attribute("name").Value, param.Attribute("default").Value))
                                            .ToDictionary(m => m.Key, m => m.Value)
                                        }).ToDictionary(m => m.Name)
                      }).ToDictionary(m => m.Name);
        }
        else
        {
          Programs = new Dictionary<string, ProgramConfig>();
        }
        ConfigFilename = filename;
        return true;
      }
      catch (Exception ex)
      {
        Console.Error.WriteLine("read config file {0} error : {1}", filename, ex.Message);
        return false;
      }
    }

    public bool Save()
    {
      return Save(ConfigFilename);
    }

    public bool Save(string filename)
    {
      try
      {
        var root = new XElement("config",
          from proKey in Programs.Keys.OrderBy(m => m)
          let pro = Programs[proKey]
          select new XElement("program",
            new XAttribute("name", pro.Name),
            new XAttribute("command", pro.Command),
            new XAttribute("defaultparameterset", pro.DefaultParameterSet),
            from setkey in pro.ParameterSet.Keys.OrderBy(m => m)
            let paramconfig = pro.ParameterSet[setkey]
            select new XElement("parameterset",
              new XAttribute("name", setkey),
              from param in paramconfig.Parameters
              select new XElement("parameter",
                new XAttribute("name", param.Key),
                new XAttribute("default", param.Value)))));
        root.Save(filename);
        return true;
      }
      catch (Exception ex)
      {
        Console.Error.WriteLine("write config file {0} error : {1}", filename, ex.Message);
        return false;
      }
    }

    public ProgramConfig FindOrCreate(string name, string defaultCommand)
    {
      if (Programs.ContainsKey(name))
      {
        return Programs[name];
      }
      var result = new ProgramConfig(name, defaultCommand);
      Programs[name] = result;
      return result;
    }
  }
}