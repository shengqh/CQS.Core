using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.ObjectModel;
using System.Xml.Linq;
using CQS.Genome.Pileup;
using CQS.Genome.Fastq;
using System.IO;

namespace CQS.Genome.Sam
{
  public class TrimedSAMAlignedItem : SAMAlignedItem
  {
    public TrimedSAMAlignedItem()
    { }

    public string OriginalSequence { get; set; }
  }

  public static class TrimedSAMAlignedItemExtension
  {
    public static void FillOriginalSequence(this IEnumerable<TrimedSAMAlignedItem> items, string fastqFile)
    {
      var map = items.ToDictionary(m => m.Qname);

      var reader = new FastqReader();
      using (var sr = StreamUtils.GetReader(fastqFile))
      {
        FastqSequence item;
        while ((item = reader.ParseOne(sr)) != null)
        {
          var name = item.Name.StringBefore(" ").StringBefore("\t");
          TrimedSAMAlignedItem titem;
          if (map.TryGetValue(name, out titem))
          {
            titem.OriginalSequence = item.SeqString;
          }
        }
      }
    }
  }
}
