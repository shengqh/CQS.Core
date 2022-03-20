using RCPA.Converter;

namespace CQS.BreastCancer
{
  public class StatusConverter<T> : StringConverter<T>
  {
    public StatusConverter(string propertyName)
      : base(propertyName)
    { }

    public override void SetProperty(T t, string value)
    {
      pi.SetValue(t, StatusValue.TransferStatus(value), null);
    }
  }
}
