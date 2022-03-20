using RCPA;
using System.Collections.Generic;
using System.IO;

namespace CQS.Genome.TNBC
{
  public class CallerResultReader : IFileReader<List<CallerResult>>
  {
    public List<CallerResult> ReadFromFile(string fileName)
    {
      var result = new List<CallerResult>();
      var lines = File.ReadAllLines(fileName);
      var headers = lines[0].Split(',');
      for (int i = 1; i < lines.Length; i++)
      {
        if (string.IsNullOrEmpty(lines[i]))
        {
          continue;
        }

        var parts = lines[i].Split(',');
        var ccr = new CallerResult();
        result.Add(ccr);
        ccr.Sample = parts[0];
        for (int j = 1; j < headers.Length; j++)
        {
          var tt = EnumUtils.StringToEnum(headers[j], TNBCSubtype.UNS);
          ccr.Items[tt] = new CallerResultValue() { Coef = double.Parse(parts[j]) };
        }
      }
      return result;
    }
  }
}
