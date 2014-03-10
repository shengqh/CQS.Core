using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CQS.Genome.Sam;
using System.Threading;
using RCPA.Gui;
using Bio.IO.SAM;
using RCPA;

namespace CQS.Genome.Mapping
{
  public class SAMAlignedItemBestScoreCandidateBuilder : SAMAlignedItemCandidateBuilder
  {
    public SAMAlignedItemBestScoreCandidateBuilder(int engineType) : base(engineType) { }

    public SAMAlignedItemBestScoreCandidateBuilder(CountProcessorOptions options) : base(options) { }

    protected override List<T> DoAddCompleted<T>(List<T> samlist)
    {
      if (options.GetSAMFormat().HasAlternativeHits || samlist.Count == 0)
      {
        return samlist;
      }

      Progress.SetMessage("Sorting mapped reads by name and score...");
      samlist.SortByNameAndScore(options.GetSAMFormat());
      Progress.SetMessage("Sorting mapped reads by name and score finished...");

      var result = new List<T> {samlist[0]};
      var last = samlist[0];

      for (var i = 1; i < samlist.Count; i++)
      {
        var sam = samlist[i];
        if (!last.Qname.Equals(sam.Qname))
        {
          last = sam;
          result.Add(last);
        }
        else if (last.AlignmentScore == sam.AlignmentScore)
        {
          last.AddLocations(sam.Locations);
        }
      }

      samlist.Clear();

      return result;
    }
  }
}
