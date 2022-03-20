using RCPA;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CQS.Genome.Mirna
{
  /// <summary>
  /// Build overlap result between two miRNA mapped file
  /// For example, the same fastq were used to search both human and mouse database, then we need to 
  /// found out the match pattern between those two database.
  /// </summary>
  public class MirnaMappedOverlapBuilder : AbstractThreadProcessor
  {
    private MirnaMappedOverlapBuilderOptions options;

    public MirnaMappedOverlapBuilderOptions Options { get { return options; } }

    public MirnaMappedOverlapBuilder(MirnaMappedOverlapBuilderOptions options)
    {
      this.options = options;
    }

    public override IEnumerable<string> Process()
    {
      var format = new MappedMirnaGroupXmlFileFormat();

      Progress.SetMessage("reading mapped reads from " + options.ReferenceFile + " ...");
      var refitems = format.ReadFromFile(options.ReferenceFile);
      var refSpecies = refitems[0][0].Name.StringBefore("-");

      Progress.SetMessage("reading mapped reads from " + options.SampleFile + " ...");
      var samitems = format.ReadFromFile(options.SampleFile);
      var samSpecies = samitems[0][0].Name.StringBefore("-");

      var paired = GetPairedMiRNA(refitems, samitems);

      //using (StreamWriter sw = new StreamWriter(targetFile))
      //{
      //  sw.WriteLine("microRNA\t{0}_sequence\t{1}_sequence\tbp_difference\tquery_sequence\t{0}_count\t{0}_estimate_count\t{1}_count\t{1}_estimate_count", refName, samName);



      //  var keys = refitems.Keys.Union(samitems.Keys).Distinct().OrderBy(m => m).ToList();
      //  foreach (var key in keys)
      //  {
      //    sw.Write(key);
      //    string refseq = string.Empty;
      //    string samseq = string.Empty;
      //    MappedMiRNA refmirna = null;
      //    MappedMiRNA sammirna = null;
      //    Dictionary<string, List<QueryMapped>> refmirnamap = new Dictionary<string, List<QueryMapped>>();
      //    Dictionary<string, List<QueryMapped>> sammirnamap = new Dictionary<string, List<QueryMapped>>();

      //    if (refitems.ContainsKey(key))
      //    {
      //      refmirna = refitems[key];
      //      refseq = refmirna.Name.Contains(":") ? refmirna.Name.StringAfter(":") : string.Empty;
      //      refmirnamap = ConvertToMap(refmirna);
      //    }

      //    if (samitems.ContainsKey(key))
      //    {
      //      sammirna = samitems[key];
      //      samseq = sammirna.Name.Contains(":") ? sammirna.Name.StringAfter(":") : string.Empty;
      //      sammirnamap = ConvertToMap(sammirna);
      //    }

      //    var seqs = refmirnamap.Keys.Union(sammirnamap.Keys).OrderBy(m => m).ToList();

      //    CombinedSequence cs = null;
      //    if (!string.IsNullOrEmpty(refseq) && !string.IsNullOrEmpty(samseq))
      //    {
      //      cs = MirnaUtils.GetCombinedSequence(refseq, samseq);
      //      sw.Write("\t{0}\t{1}\t{2}", cs.GetAnnotatedSequence1(), cs.GetAnnotatedSequence2(), cs.MismatchPositions.Length);
      //    }
      //    else
      //    {
      //      sw.Write("\t{0}\t{1}\t-", refseq, samseq);
      //    }
      //    sw.WriteLine();

      //    foreach (var seq in seqs)
      //    {
      //      sw.Write("\t\t\t\t{0}", seq);
      //      if (refmirnamap.ContainsKey(seq))
      //      {
      //        sw.Write("\t{0:0.00}", refmirnamap[seq].Sum(m => m.QueryCount));
      //        sw.Write("\t{0:0.00}", refmirnamap[seq].Sum(m => m.EsminatedCount));
      //      }
      //      else
      //      {
      //        sw.Write("\t\t");
      //      }

      //      if (sammirnamap.ContainsKey(seq))
      //      {
      //        sw.Write("\t{0:0.00}", sammirnamap[seq].Sum(m => m.QueryCount));
      //        sw.Write("\t{0:0.00}", sammirnamap[seq].Sum(m => m.EsminatedCount));
      //      }
      //      else
      //      {
      //        sw.Write("\t\t");
      //      }
      //      sw.WriteLine();
      //    }
      //  }
      //}

      return new string[] { options.OutputFile };
    }

    class PairedMiRNAGroup
    {
      public HashSet<MappedMirnaGroup> RefItems { get; set; }
      public HashSet<MappedMirnaGroup> SamItems { get; set; }
      public int MismatchCount { get; set; }
    }

    private List<PairedMiRNAGroup> GetPairedMiRNA(List<MappedMirnaGroup> refitems, List<MappedMirnaGroup> samitems)
    {
      List<PairedMiRNAGroup> result = new List<PairedMiRNAGroup>();

      Dictionary<MappedMirnaGroup, PairedMiRNAGroup> refMostSimilar = FindSimilar(refitems, samitems);
      Dictionary<MappedMirnaGroup, PairedMiRNAGroup> samMostSimilar = FindSimilar(samitems, refitems);

      var paired = new HashSet<MappedMirnaGroup>();
      var refs = refMostSimilar.Keys.ToList();
      foreach (var refitem in refs)
      {
        var mappedsams = refMostSimilar[refitem].SamItems;
        if (mappedsams.Count == 0)
        {
          continue;
        }

        foreach (var samitem in mappedsams)
        {
          //if both are most similar, pair them
          if (samMostSimilar[samitem].SamItems.Contains(refitem))
          {
            Console.WriteLine("{0}\t{1}\t{2}", refitem.DisplayName, samitem.DisplayName, refMostSimilar[refitem].MismatchCount);
            result.Add(refMostSimilar[refitem]);

            paired.Add(refitem);
            paired.Add(samitem);
          }
        }
      }

      foreach (var item in paired)
      {
        if (refMostSimilar.ContainsKey(item))
        {
          refMostSimilar.Remove(item);
        }
        if (samMostSimilar.ContainsKey(item))
        {
          samMostSimilar.Remove(item);
        }
      }

      Console.WriteLine("ref...");
      refMostSimilar.ToList().ForEach(m => Console.WriteLine(m.Key.DisplayName + "\t" + (m.Value.SamItems.Count > 0 ? m.Value.MismatchCount.ToString() : "") + "\t" + (from it in m.Value.SamItems select it.DisplayName).Merge(",")));
      Console.WriteLine("sam...");
      samMostSimilar.ToList().ForEach(m => Console.WriteLine(m.Key.DisplayName + "\t" + (m.Value.SamItems.Count > 0 ? m.Value.MismatchCount.ToString() : "") + "\t" + (from it in m.Value.SamItems select it.DisplayName).Merge(",")));

      return result;
    }

    private Dictionary<MappedMirnaGroup, PairedMiRNAGroup> FindSimilar(List<MappedMirnaGroup> refmap, List<MappedMirnaGroup> sammap)
    {
      var result = new Dictionary<MappedMirnaGroup, PairedMiRNAGroup>();
      foreach (var refitem in refmap)
      {
        var refseq = refitem[0].Sequence;

        var samples = (from samitem in sammap
                       let combined = MirnaUtils.GetCombinedSequence(refseq, samitem[0].Sequence)
                       select new { Combined = combined, Item = samitem })
                   .GroupBy(m => m.Combined.MismatchPositions.Length)
                   .ToList()
                   .OrderBy(m => m.Key)
                   .First().ToList();

        if (samples.First().Combined.MismatchPositions.Length > refseq.Length * 0.4)
        {
          result[refitem] = new PairedMiRNAGroup()
          {
            RefItems = new HashSet<MappedMirnaGroup>(new[] { refitem }),
            SamItems = new HashSet<MappedMirnaGroup>(),
            MismatchCount = int.MaxValue
          };
        }
        else
        {
          result[refitem] = new PairedMiRNAGroup()
          {
            RefItems = new HashSet<MappedMirnaGroup>(new[] { refitem }),
            SamItems = new HashSet<MappedMirnaGroup>(samples.ConvertAll(m => m.Item)),
            MismatchCount = samples.First().Combined.MismatchPositions.Length
          };
        }
      }
      return result;
    }

    //private static Dictionary<string, List<SAMAlignedItem>> ConvertToMap(MappedMiRNAGroup refmirna)
    //{
    //  var result = (from mirna in refmirna.MappedRegions
    //                from mapped in mirna.Mapped
    //                from query in mapped.Value.AlignedItems
    //                select new { Query = query, Position = mapped.Value.Position })
    //                .GroupBy(m => new string('_', m.Position) +
    //                  m.Query.Sequence, m => m.Query)
    //                .ToDictionary(m => m.Key, m => m.ToList());

    //  return result;
    //}

    //private static Dictionary<string, MappedMiRNAGroup> ReadDictionary(MappedMiRNAGroupXmlFileFormat format, string file)
    //{
    //  return format.ReadFromFile(file).ToDictionary(m => m[0].Name.StringAfter("-").StringBefore(":"));
    //}
  }
}
