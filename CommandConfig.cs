using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RCPA.Gui;
using RCPA.Utils;
using System.IO;
using System.Reflection;
using System.Xml.Linq;

namespace CQS
{
  public class ParameterConfig
  {
    public string Name { get; set; }

    public Dictionary<string, string> Parameters { get; set; }

    public ParameterConfig(string name)
    {
      this.Name = name;
      this.Parameters = new Dictionary<string, string>();
    }

    public string GetParameter(string name, string value)
    {
      if (!string.IsNullOrEmpty(value))
      {
        this.Parameters[name] = value;
        return value;
      }

      if (this.Parameters.ContainsKey(name))
      {
        return this.Parameters[name];
      }
      else
      {
        return null;
      }
    }
  }

  public class ProgramConfig
  {
    public ProgramConfig()
    {
      this.Name = string.Empty;
      this.Command = string.Empty;
      this.ParameterSet = new Dictionary<string, ParameterConfig>();
      this.DefaultParameterSet = string.Empty;
    }

    public ProgramConfig(string name, string command)
    {
      this.Name = name;
      this.Command = command;
      this.ParameterSet = new Dictionary<string, ParameterConfig>();
      this.DefaultParameterSet = string.Empty;
    }

    public string Name { get; set; }

    public string Command { get; set; }

    public string DefaultParameterSet { get; set; }

    public Dictionary<string, ParameterConfig> ParameterSet { get; set; }

    public ParameterConfig FindOrCreate(string name)
    {
      if (!this.ParameterSet.ContainsKey(name))
      {
        this.ParameterSet[name] = new ParameterConfig(name);
      }
      return this.ParameterSet[name];
    }

    public bool HasParameterSet(string name)
    {
      return this.ParameterSet.ContainsKey(name);
    }
  }

  public class CommandConfig
  {
    private string _configFilename;

    public string ConfigFilename
    {
      get
      {
        if (string.IsNullOrEmpty(this._configFilename))
        {
          if (SystemUtils.IsLinux)
          {
            this._configFilename = Path.ChangeExtension(System.Windows.Forms.Application.ExecutablePath, ".linux");
          }
          else
          {
            this._configFilename = Path.ChangeExtension(System.Windows.Forms.Application.ExecutablePath, ".win");
          }
        }
        return this._configFilename;
      }
      set
      {
        this._configFilename = value;
      }
    }

    public CommandConfig()
    {
      this.Programs = new Dictionary<string, ProgramConfig>();
    }

    public Dictionary<string, ProgramConfig> Programs { get; private set; }

    public bool Load()
    {
      return Load(this.ConfigFilename);
    }

    public bool Load(string filename)
    {
      try
      {
        if (File.Exists(filename))
        {
          XElement root = XElement.Load(filename);
          this.Programs = (from pro in root.Elements("program")
                           select new ProgramConfig(pro.Attribute("name").Value, pro.Attribute("command").Value)
                           {
                             DefaultParameterSet = pro.Element("defaultparameterset") == null ? string.Empty : pro.Element("defaultparameterset").Value,
                             ParameterSet = (from paramset in pro.Elements("parameterset")
                                             select new ParameterConfig(paramset.Attribute("name").Value)
                                             {
                                               Parameters = (from param in paramset.Elements("parameter")
                                                             select new KeyValuePair<string, string>(param.Attribute("name").Value, param.Attribute("default").Value)).ToDictionary(m => m.Key, m => m.Value)
                                             }).ToDictionary(m => m.Name)
                           }).ToDictionary(m => m.Name);
        }
        else
        {
          this.Programs = new Dictionary<string, ProgramConfig>();
        }
        this.ConfigFilename = filename;
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
      return Save(this.ConfigFilename);
    }

    public bool Save(string filename)
    {
      try
      {
        XElement root = new XElement("config",
          from proKey in this.Programs.Keys.OrderBy(m => m)
          let pro = this.Programs[proKey]
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
      if (this.Programs.ContainsKey(name))
      {
        return this.Programs[name];
      }
      else
      {
        ProgramConfig result = new ProgramConfig(name, defaultCommand);
        this.Programs[name] = result;
        return result;
      }
    }
  }
}
