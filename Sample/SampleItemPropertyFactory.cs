using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RCPA.Converter;
using RCPA;

namespace CQS.Sample
{
  public abstract class SampleItemPropertyFactory<T> : PropertyConverterFactory<T> where T : SampleItem
  {
    public virtual void Initialize()
    {
      this.RegisterConverter(new PropertyConverter<T>("Dataset", (m, n) => m.Dataset = n, m => m.Dataset));
      this.RegisterConverter(new PropertyConverter<T>("Sample", (m, n) => m.Sample = n, m => m.Sample));
      this.RegisterConverter(new PropertyConverter<T>("SourceName", (m, n) => m.SourceName = n, m => m.SourceName), "Source Name");
      this.RegisterConverter(new PropertyConverter<T>("SampleTitle", (m, n) => m.SampleTitle = n, m => m.SampleTitle));
    }
  }
}
