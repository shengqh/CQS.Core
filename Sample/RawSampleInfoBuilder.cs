using CQS.Ncbi.Geo;
using RCPA;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CQS.Sample
{
  public class RawSampleInfoBuilder : AbstractThreadProcessor
  {
    private RawSampleInfoBuilderOptions options;

    public RawSampleInfoBuilder(RawSampleInfoBuilderOptions options)
    {
      this.options = options;
    }

    public override IEnumerable<string> Process()
    {
      var sdata = new RawSampleInfoReader().ReadDescriptionFromDirectory(options.InputDirectory);
      var data = sdata.ToList().ToDictionary(m => m.Key.ToUpper(), m => m.Value);

      var files = GeoUtils.GetGsmNameFileMap(options.InputDirectory);
      var samples = (from k in files.Keys select k.ToUpper()).OrderBy(m => m).ToList();

      var columns = (from d in data.Values
                     from col in d.Keys
                     select col).Distinct().OrderBy(m => m).ToList();

      bool bError = false;
      var errorFile = options.OutputFile + ".error";
      using (var sw = new StreamWriter(options.OutputFile))
      using (var swErr = new StreamWriter(errorFile))
      {
        sw.WriteLine("Sample\t{0}", columns.Merge("\t"));
        foreach (var sample in samples)
        {
          if (!data.ContainsKey(sample))
          {
            var error = string.Format("Cannot find {0} in {1}", sample, options.InputDirectory);
            swErr.WriteLine(error);
            Progress.SetMessage(error);
            bError = true;
            continue;
          }

          var dic = data[sample];
          sw.Write(sample);
          foreach (var column in columns)
          {
            if (dic.ContainsKey(column))
            {
              sw.Write("\t{0}", dic[column].Merge(" ! "));
            }
            else
            {
              sw.Write("\t");
            }
          }
          sw.WriteLine();
        }
      }

      if (!bError)
      {
        File.Delete(errorFile);
      }

      return new string[] { options.OutputFile };
    }
  }
}
