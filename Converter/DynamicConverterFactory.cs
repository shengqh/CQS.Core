using RCPA;
using RCPA.Converter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CQS.Converter
{
  public class DynamicConverterFactory<T> : IPropertyConverterFactory<T> where T : IAnnotation, new()
  {
    private PropertyInfo[] properties;

    public DynamicConverterFactory()
    {
      Type t = typeof(T);
      properties = t.GetProperties();
    }

    public T Allocate()
    {
      return new T();
    }

    public IPropertyConverter<T> FindConverter(string name)
    {
      return ConverterUtils.GetPropertConverters<T>(properties, name);
    }

    public IPropertyConverter<T> FindConverter(string name, string version)
    {
      return FindConverter(name);
    }

    public IPropertyConverter<T> GetConverters(string header, char delimiter)
    {
      return new CompositePropertyConverter<T>(
        from p in header.Split(delimiter)
        select FindConverter(p), delimiter);
    }

    public IPropertyConverter<T> GetConverters(string header, char delimiter, string version)
    {
      return GetConverters(header, delimiter);
    }

    public IPropertyConverter<T> GetConverters(string header, char delimiter, string version, List<T> items)
    {
      return GetConverters(header, delimiter);
    }
  }
}
