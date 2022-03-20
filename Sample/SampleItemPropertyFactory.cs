using RCPA;
using RCPA.Converter;

namespace CQS.Sample
{
  public class SampleItemPropertyFactory<T> : PropertyConverterFactory<T> where T : SampleItem, new()
  {
    public SampleItemPropertyFactory()
    {
      Initialize();
    }

    public virtual void Initialize()
    {
      this.RegisterConverter(new PropertyConverter<T>("Dataset", (m, n) => m.Dataset = n, m => m.Dataset));
      this.RegisterConverter(new PropertyConverter<T>("Sample", (m, n) => m.Sample = n, m => m.Sample));
      this.RegisterConverter(new PropertyConverter<T>("SourceName", (m, n) => m.SourceName = n, m => m.SourceName), "Source Name");
      this.RegisterConverter(new PropertyConverter<T>("SampleTitle", (m, n) => m.SampleTitle = n, m => m.SampleTitle));
      this.RegisterConverter(new PropertyConverter<T>("SampleFile", (m, n) => m.SampleFile = n, m => m.SampleFile.Replace('\\', '/')));
    }

    public override T Allocate()
    {
      return new T();
    }
  }
}
