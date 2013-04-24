using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RCPA;
using System.Reflection;
using RCPA.Converter;
using System.Xml.Linq;
using RCPA.Format;
using CQS.Sample;
using CQS.BreastCancer;

namespace CQS.Converter
{
  public static class ConverterUtils
  {
    public static bool PropertyHasAttribute<TAttribute>(PropertyInfo pi)
    {
      foreach (Attribute attr in pi.GetCustomAttributes(true))
      {
        if (attr is TAttribute)
        {
          return true;
        }
      }

      return false;
    }

    public static List<FileDefinitionItem> GetItems<TClass, TAttribute>()
    {
      List<FileDefinitionItem> result = new List<FileDefinitionItem>();
      Type t = typeof(TClass);
      var properties = t.GetProperties();
      foreach (var pi in properties)
      {
        if (pi.CanRead && pi.CanWrite)
        {
          foreach (Attribute attr in pi.GetCustomAttributes(true))
          {
            if (attr is TAttribute)
            {
              var item = GetFileDefinitionItem<TClass>(pi);
              if (item != null)
              {
                result.Add(item);
              }
            }
          }
        }
      }

      return result;
    }

    public static List<FileDefinitionItem> GetItems<T>()
    {
      List<FileDefinitionItem> result = new List<FileDefinitionItem>();
      Type t = typeof(T);
      var properties = t.GetProperties();
      foreach (var pi in properties)
      {
        if (pi.CanRead && pi.CanWrite)
        {
          var converter = GetFileDefinitionItem<T>(pi);
          if (converter != null)
          {
            result.Add(converter);
          }
        }
      }

      return result;
    }

    private static FileDefinitionItem GetFileDefinitionItem<T>(PropertyInfo pi)
    {
      if (pi.PropertyType == typeof(string))
      {
        return new FileDefinitionItem()
        {
          PropertyName = pi.Name,
          ValueType = "string"
        };
      }
      else if (pi.PropertyType == typeof(int))
      {
        return new FileDefinitionItem()
        {
          PropertyName = pi.Name,
          ValueType = "int"
        };
      }
      else if (pi.PropertyType == typeof(double))
      {
        return new FileDefinitionItem()
        {
          PropertyName = pi.Name,
          ValueType = "double"
        };
      }
      return null;
    }

    public static IPropertyConverter<T> GetPropertConverters<T>(PropertyInfo[] pis, string propertyName)
    {
      foreach (var pi in pis)
      {
        if (pi.Name.Equals(propertyName))
        {
          return GetPropertyConverter<T>(pi);
        }
      }

      return null;
    }

    private static IPropertyConverter<T> GetPropertyConverter<T>(PropertyInfo pi)
    {
      if (pi.PropertyType == typeof(string))
      {
        if (PropertyHasAttribute<StatusInfo>(pi))
        {
          return new StatusConverter<T>(pi.Name);
        }
        else
        {
          return new StringConverter<T>(pi.Name);
        }
      }
      else if (pi.PropertyType == typeof(int))
      {
        return new IntegerConverter<T>(pi.Name);
      }
      else if (pi.PropertyType == typeof(double))
      {
        return new DoubleConverter<T>(pi.Name);
      }
      return null;
    }

    public static List<IPropertyConverter<T>> GetPropertConverters<T>()
    {
      List<IPropertyConverter<T>> result = new List<IPropertyConverter<T>>();
      Type t = typeof(T);
      var properties = t.GetProperties();
      foreach (var pi in properties)
      {
        if (pi.CanRead && pi.CanWrite)
        {
          var converter = GetPropertyConverter<T>(pi);
          if (converter != null)
          {
            result.Add(converter);
          }
        }
      }

      return result;
    }

    public static void WriteToFile<T>(string fileName, Dictionary<string, IPropertyConverter<T>> map)
    {
      XElement root = new XElement("Config",
        from e in map
        select new XElement("Property",
          new XElement("Header", e.Key),
          new XElement("Property", e.Value.Name)));
      root.Save(fileName);
    }

    public static Dictionary<string, IPropertyConverter<T>> ReadFromFile<T>(string fileName)
    {
      Dictionary<string, IPropertyConverter<T>> result = new Dictionary<string, IPropertyConverter<T>>();
      XElement root = XElement.Load(fileName);

      var properties = typeof(T).GetProperties();
      foreach (XElement ele in root.Elements())
      {
        var header = ele.Element("Header").Value;
        var propertyName = ele.Element("Property").Value;
        result[header] = GetPropertConverters<T>(properties, propertyName);
      }

      return result;
    }
  }
}
