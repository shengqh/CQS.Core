using CQS.Genome.Feature;
using CQS.Genome.SmallRNA;
using RCPA;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CQS.Genome.Parclip
{
  public class ParclipSmallRNAT2CBuilder : AbstractThreadProcessor
  {
    private ParclipSmallRNAT2CBuilderOptions options;

    public ParclipSmallRNAT2CBuilder(ParclipSmallRNAT2CBuilderOptions options)
    {
      this.options = options;
    }
    public override IEnumerable<string> Process()
    {
      List<FeatureItemGroup> groups = new SmallRNAT2CMutationBuilder(options.ExpectRate)
      {
        Progress = this.Progress
      }.Build(options.InputFile);

      groups.Sort((m1, m2) => m1[0].Locations[0].PValue.CompareTo(m2[0].Locations[0].PValue));
      var unfiltered = Path.ChangeExtension(options.OutputFile, "") + "unfiltered" + Path.GetExtension(options.OutputFile);
      new FeatureItemGroupT2CWriter(options.ExpectRate).WriteToFile(unfiltered, groups);
      new FeatureItemGroupXmlFormat(true).WriteToFile(unfiltered + ".xml", groups);

      groups.RemoveByLocation(m => m.PValue > options.PValue);
      Progress.SetMessage("There are {0} groups containing T2C mutation with pValue < {1}", groups.Count, options.PValue);

      new FeatureItemGroupT2CWriter(options.ExpectRate).WriteToFile(options.OutputFile, groups);
      new FeatureItemGroupXmlFormat(true).WriteToFile(options.OutputFile + ".xml", groups);

      return new string[] { options.OutputFile, options.OutputFile + ".xml" };
    }
  }
}
