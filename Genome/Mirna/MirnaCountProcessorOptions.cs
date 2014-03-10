using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CQS.Commandline;
using CommandLine;
using System.IO;
using CQS.Genome.Sam;
using CQS.Genome.Mapping;

namespace CQS.Genome.Mirna
{
  public class MirnaCountProcessorOptions : CountProcessorOptions
  {
    private static readonly List<int> DEFAULT_Offsets = new List<int>(new int[] { 0, 1, 2 });

    public MirnaCountProcessorOptions()
    {
      Offsets = DEFAULT_Offsets;
    }

    [OptionList("offsets", Required = false, Separator = ',', HelpText = "Allowed (prilority ordered) offsets from miRNA locus, default: 0,1,2")]
    public List<int> Offsets { get; set; }

    public override bool PrepareOptions()
    {
      
      if (this.Offsets == null || this.Offsets.Count == 0)
      {
        this.Offsets = DEFAULT_Offsets;
      }

      return base.PrepareOptions();
    }
  }
}
