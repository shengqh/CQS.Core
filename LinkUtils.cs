using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CQS.Ncbi.Geo;

namespace CQS
{
  public static class LinkUtils
  {
    public static string GetGoogleLink(string dataset)
    {
      return string.Format(@"https://www.google.com/search?q={0}", dataset);
    }

    public static string GetGeoLink(string gseOrGsmName)
    {
      return string.Format(@"http://www.ncbi.nlm.nih.gov/geo/query/acc.cgi?acc={0}", gseOrGsmName);
    }

    public static string GetEbiArrayLink(string dataset)
    {
      return string.Format(@"http://www.ebi.ac.uk/arrayexpress/experiments/{0}", dataset);
    }
  }
}
