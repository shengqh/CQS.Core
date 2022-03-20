using RCPA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace CQS.TCGA.TCGATechnologyImpl
{
  public class FindParticipantProtein : IParticipantFinder
  {
    private Dictionary<string, string> maps;

    public FindParticipantProtein(string clinicShipmentPortionFile)
    {
      var anns = new AnnotationFormat().ReadFromFile(clinicShipmentPortionFile);

      maps = anns.ToDictionary(m => m.Annotations["bcr_shipment_portion_uuid"].ToString().ToLower(),
        m => m.Annotations["bcr_sample_barcode"].ToString());
    }

    //mdanderson.org_BRCA.MDA_RPPA_Core.protein_expression.Level_3.0a7af48e-5b73-42e8-acf6-a0bca1372cc4.txt
    private static Regex reg = new Regex("Level_3.([^.]*)");
    public string FindParticipant(string fileName)
    {
      var m = reg.Match(fileName);
      if (!m.Success)
      {
        throw new ArgumentException(string.Format("It is not a valid TCGA protein file name : \n{0}", fileName));
      }

      var uuid = m.Groups[1].Value.ToLower();
      if (!maps.ContainsKey(uuid))
      {
        throw new ArgumentException(string.Format("Cannot find uuid {0} in clinic shipment portion file", uuid));
      }

      return maps[uuid];
    }
  }
}
