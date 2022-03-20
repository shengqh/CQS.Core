using System;

namespace CQS.TCGA
{
  public interface IParticipantFinder
  {
    string FindParticipant(string fileName);
  }

  public class DefaultParticipantFinder : IParticipantFinder
  {
    private IParticipantFinder finder;
    private string defaultValue;

    public DefaultParticipantFinder(IParticipantFinder finder, string defaultValue)
    {
      this.finder = finder;
      this.defaultValue = defaultValue;
    }


    public string FindParticipant(string fileName)
    {
      try
      {
        return finder.FindParticipant(fileName);
      }
      catch (Exception)
      {
        return defaultValue;
      }
    }
  }
}
