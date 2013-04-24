using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RCPA;
using System.Text.RegularExpressions;

namespace CQS.TCGA.TCGATechnologyImpl
{
  public class FindParticipantBySdrfFile : IParticipantFinder
  {
    private string sdrfFile;
    private Dictionary<string, string> maps;

    public FindParticipantBySdrfFile(string sdrfFile, string fileColumn, string barCodeColumn)
    {
      this.sdrfFile = sdrfFile;

      var anns = new MapReader(fileColumn, barCodeColumn).ReadFromFile(sdrfFile);
      maps = new Dictionary<string, string>();
      foreach (var ann in anns)
      {
        if (ann.Key.Equals("->"))
        {
          continue;
        }

        maps[ann.Key.ToLower()] = TCGAUtils.GetSampleBarCode(ann.Value);
      }
    }

    public string FindParticipant(string fileName)
    {
      var lFilename = fileName.ToLower();

      if (!maps.ContainsKey(lFilename))
      {
        throw new ArgumentException(string.Format("Cannot find file name {0} in sdrf file {1}", fileName, this.sdrfFile));
      }

      return maps[lFilename];
    }
  }
}
