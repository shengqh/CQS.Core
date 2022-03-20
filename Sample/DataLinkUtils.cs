namespace CQS.Sample
{
  public static class DataLinkUtils
  {
    public static string GetDatasetLink(string dataset)
    {
      if (dataset.ToUpper().StartsWith("GSE"))
      {
        return LinkUtils.GetGeoLink(dataset);
      }

      if (dataset.ToUpper().StartsWith("E-"))
      {
        return LinkUtils.GetEbiArrayLink(dataset);
      }

      return LinkUtils.GetGoogleLink(dataset);
    }

    public static bool IsDataLinkSupported(string dataset)
    {
      return dataset.ToUpper().StartsWith("GSE");
    }

    public static string GetDataLink(string datafile)
    {
      if (datafile.ToUpper().StartsWith("GSM"))
      {
        return LinkUtils.GetGeoLink(datafile);
      }
      return LinkUtils.GetGoogleLink(datafile);
    }
  }
}
