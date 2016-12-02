using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CQS.Genome.Fastq;
using CQS.Genome.Sam;
using CQS.Genome.Mirna;
using CQS.Genome.Feature;
using CQS.Genome.Mapping;
using RCPA.Utils;

namespace CQS.Genome.SmallRNA
{
  public class SmallRNACountProcessor : AbstractSmallRNACountProcessor<SmallRNACountProcessorOptions>
  {
    protected Dictionary<char, char> noPaneltyMutations = new Dictionary<char, char>();

    public SmallRNACountProcessor(SmallRNACountProcessorOptions options)
      : base(options)
    {
      noPaneltyMutations['C'] = 'T';
      noPaneltyMutations['G'] = 'A';
    }

    //public void WriteToFile(string fileName, List<FeatureItemGroup> groups, bool exportPValue)
    //{
    //  //var utf8 = new UTF8Encoding(false);

    //  Progress.SetMessage("Ready for writing ...");

    //  using (var ft = new FileStream(fileName, FileMode.Create))
    //  //using (var xw = new StreamWriter(ft, utf8))
    //  using (var xw = new StreamWriter(ft))
    //  {
    //    //Console.WriteLine("Start writing ... ");
    //    Progress.SetMessage("Start writing ... ");
    //    //xw.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
    //    xw.WriteLine("<?xml version=\"1.0\"?>");
    //    xw.WriteLine("<root>");

    //    Progress.SetMessage("Getting queries ... ");
    //    var queries = groups.GetQueries();

    //    Progress.SetMessage("Writing {0} queries ...", queries.Count);
    //    xw.WriteLine("  <queries>");
    //    foreach (var query in queries)
    //    {
    //      xw.WriteLine(@"    <query name=""{0}"" sequence=""{1}"" count=""{2}"">", query.Qname, query.Sequence, query.QueryCount);
    //      foreach (var loc in query.Locations)
    //      {
    //        xw.WriteLine(@"      <location seqname=""{0}"" start=""{1}"" end=""{2}"" strand=""{3}"" cigar=""{4}"" score=""{5}"" mdz=""{6}"" nmi=""{7}"" nnpm=""{8}"" />",
    //          loc.Seqname,
    //          loc.Start,
    //          loc.End,
    //          loc.Strand,
    //          loc.Cigar,
    //          loc.AlignmentScore,
    //          loc.MismatchPositions,
    //          loc.NumberOfMismatch,
    //          loc.NumberOfNoPenaltyMutation);
    //      }
    //      xw.WriteLine("    </query>");
    //    }
    //    xw.WriteLine("  </queries>");

    //    Progress.SetMessage("Writing {0} subjects ...", groups.Count);
    //    xw.WriteLine("  <subjectResult>");
    //    foreach (var itemgroup in groups)
    //    {
    //      xw.WriteLine("    <subjectGroup>");
    //      foreach (var item in itemgroup)
    //      {
    //        xw.WriteLine("      <subject name=\"{0}\">", item.Name);
    //        foreach (var region in item.Locations)
    //        {
    //          xw.Write("        <region seqname=\"{0}\" start=\"{1}\" end=\"{2}\" strand=\"{3}\" sequence=\"{4}\"",
    //            region.Seqname,
    //            region.Start,
    //            region.End,
    //            region.Strand,
    //            XmlUtils.ToXml(region.Sequence));
    //          if (exportPValue)
    //          {
    //            xw.Write(" query_count_before_filter=\"{0}\"", region.QueryCountBeforeFilter);
    //            xw.Write(" query_count=\"{0}\"", region.QueryCount);
    //            xw.Write(" pvalue=\"{0}\"", region.PValue);
    //          }
    //          xw.WriteLine(" size=\"{0}\">", region.Length);
    //          foreach (var sl in region.SamLocations)
    //          {
    //            var loc = sl.SamLocation;
    //            xw.Write("          <query qname=\"{0}\"", loc.Parent.Qname);
    //            xw.Write(" loc=\"{0}\"", loc.GetLocation());
    //            xw.Write(" overlap=\"{0}\"", string.Format("{0:0.##}", sl.OverlapPercentage));
    //            xw.Write(" offset=\"{0}\"", sl.Offset);
    //            xw.Write(" query_count=\"{0}\"", loc.Parent.QueryCount);
    //            xw.Write(" seq_len=\"{0}\"", loc.Parent.Sequence.Length);
    //            xw.Write(" nmi=\"{0}\"", loc.NumberOfMismatch);
    //            xw.WriteLine(" nnpm=\"{0}\" />", loc.NumberOfNoPenaltyMutation);
    //          }
    //          xw.WriteLine("        </region>");
    //        }
    //        xw.WriteLine("      </subject>");
    //      }
    //      xw.WriteLine("    </subjectGroup>");
    //    }
    //    xw.WriteLine("  </subjectResult>");
    //    xw.Write("</root>");
    //  }
    //  Progress.SetMessage("Writing xml file finished.");
    //}


    public override IEnumerable<string> Process()
    {
      var result = new List<string>();

      //read regions      
      var featureLocations = options.GetSequenceRegions();

      Progress.SetMessage("There are {0} coordinate entries", featureLocations.Count);
      if (featureLocations.Count == 0)
      {
        throw new Exception(string.Format("No coordinate found in file {1}", options.CoordinateFile));
      }

      var featureChroms = new HashSet<string>(from feature in featureLocations
                                              select feature.Seqname);

      var resultFilename = options.OutputFile;
      result.Add(resultFilename);

      //var xmlFormat = new FeatureItemGroupXmlFormatHand();
      //var xmlFormat = new FeatureItemGroupXmlFormat();

      //parsing reads
      List<string> totalQueries;
      var reads = ParseCandidates(options.InputFiles, resultFilename, out totalQueries);
      int totalQueryCount;
      if (reads.Count == totalQueries.Count && File.Exists(options.CountFile))
      {
        totalQueryCount = Counts.GetTotalCount();
      }
      else
      {
        totalQueryCount = (from q in totalQueries select q.StringBefore(SmallRNAConsts.NTA_TAG)).Distinct().Sum(m => Counts.GetCount(m));
      }

      //First of all, draw mapping position graph
      var miRNAPositionFile = Path.ChangeExtension(options.OutputFile, SmallRNAConsts.miRNA + ".position");
      var tRNAPositionFile = Path.ChangeExtension(options.OutputFile, SmallRNAConsts.tRNA + ".position");
      if (!options.NotOverwrite || (!File.Exists(miRNAPositionFile) && !File.Exists(tRNAPositionFile)))
      {
        Progress.SetMessage("Drawing position pictures...");
        var notNTAreads = reads.Where(m => !m.Qname.Contains(SmallRNAConsts.NTA_TAG) || m.Qname.StringAfter(SmallRNAConsts.NTA_TAG).Length == 0).ToList();

        DrawPositionImage(notNTAreads, featureLocations.Where(m => m.Category.Equals(SmallRNAConsts.miRNA)).ToList(), "miRNA", miRNAPositionFile);
        DrawPositionImage(notNTAreads, featureLocations.Where(m => m.Category.Equals(SmallRNAConsts.tRNA)).ToList(), "tRNA", tRNAPositionFile);

        var snRNAPositionFile = Path.ChangeExtension(options.OutputFile, ".snRNA.position");
        var snoRNAPositionFile = Path.ChangeExtension(options.OutputFile, ".snoRNA.position");
        DrawPositionImage(notNTAreads, featureLocations.Where(m => m.Name.StartsWith("snRNA")).ToList(), "snRNA", snRNAPositionFile, true);
        DrawPositionImage(notNTAreads, featureLocations.Where(m => m.Name.StartsWith("snoRNA")).ToList(), "snoRNA", snoRNAPositionFile, true);
      }

      var allmapped = new List<FeatureItemGroup>();
      var mappedfile = resultFilename + ".mapped.xml";
      if (File.Exists(mappedfile) && options.NotOverwrite)
      {
        Progress.SetMessage("Reading mapped feature items...");
        allmapped = new FeatureItemGroupXmlFormat().ReadFromFile(mappedfile);

        WriteInfoFile(result, resultFilename, reads, totalQueryCount, allmapped);
      }
      else
      {
        Progress.SetMessage("Mapping feature items...");
        //mapping reads to features based on miRNA, tRNA and other smallRNA priority
        MapReadToSequenceRegion(featureLocations, reads);

        var featureMapped = featureLocations.GroupByName();
        featureMapped.RemoveAll(m => m.GetEstimatedCount() == 0);
        featureMapped.ForEach(m => m.CombineLocations());

        var mirnaGroups = featureMapped.Where(m => m.Name.StartsWith(SmallRNAConsts.miRNA)).GroupBySequence();
        if (mirnaGroups.Count > 0)
        {
          OrderFeatureItemGroup(mirnaGroups);

          Progress.SetMessage("writing miRNA count ...");

          var mirnaCountFile = Path.ChangeExtension(resultFilename, "." + SmallRNAConsts.miRNA + ".count");
          new SmallRNACountMicroRNAWriter(options.Offsets).WriteToFile(mirnaCountFile, mirnaGroups);
          result.Add(mirnaCountFile);
          allmapped.AddRange(mirnaGroups);

          //Progress.SetMessage("writing miRNA mapping details...");
          //WriteToFile(mirnaCountFile + ".xml", mirnaGroups, false);
          //Progress.SetMessage("writing miRNA mapping details done.");
        }
        mirnaGroups.Clear();

        var trnaCodeGroups = featureMapped.Where(m => m.Name.StartsWith(SmallRNAConsts.tRNA)).GroupByFunction(SmallRNAUtils.GetTRNACode, false);
        if (trnaCodeGroups.Count > 0)
        {
          OrderFeatureItemGroup(trnaCodeGroups);

          Progress.SetMessage("writing tRNA code count ...");
          var trnaCodeCountFile = Path.ChangeExtension(resultFilename, "." + SmallRNAConsts.tRNA + ".count");

          new FeatureItemGroupCountWriter().WriteToFile(trnaCodeCountFile, trnaCodeGroups);
          result.Add(trnaCodeCountFile);

          allmapped.AddRange(trnaCodeGroups);

          //Progress.SetMessage("writing tRNA mapping details...");
          //WriteToFile(trnaCodeCountFile + ".xml", trnaCodeGroups, false);
          //Progress.SetMessage("writing tRNA mapping details done.");

          /*
          var trnaGroups = featureMapped.Where(m => m.Name.StartsWith(SmallRNAConsts.tRNA)).GroupByIdenticalQuery();
          OrderFeatureItemGroup(trnaGroups);

          Progress.SetMessage("writing tRNA count ...");
          var trnaCountFile = Path.ChangeExtension(resultFilename, "." + SmallRNAConsts.tRNA + ".byquery.count");

          new FeatureItemGroupCountWriter().WriteToFile(trnaCountFile, trnaGroups);
          result.Add(trnaCountFile);

          allmapped.AddRange(trnaGroups);
          */
        }
        trnaCodeGroups.Clear();

        var otherGroups = featureMapped.Where(m => !m.Name.StartsWith(SmallRNAConsts.miRNA) && !m.Name.StartsWith(SmallRNAConsts.tRNA)).GroupByIdenticalQuery();
        if (otherGroups.Count > 0)
        {
          OrderFeatureItemGroup(otherGroups);

          if (mirnaGroups.Count > 0 || trnaCodeGroups.Count > 0)
          {
            Progress.SetMessage("writing other smallRNA count ...");
            var otherCountFile = Path.ChangeExtension(resultFilename, ".other.count");
            new FeatureItemGroupCountWriter().WriteToFile(otherCountFile, otherGroups);
            result.Add(otherCountFile);

            //Progress.SetMessage("writing other smallRNA mapping details...");
            //WriteToFile(otherCountFile + ".xml", otherGroups, false);
            //Progress.SetMessage("writing other smallRNA mapping details done.");
          }

          allmapped.AddRange(otherGroups);
        }
        otherGroups.Clear();

        Progress.SetMessage("writing all smallRNA count ...");
        new FeatureItemGroupCountWriter().WriteToFile(resultFilename, allmapped);
        result.Add(resultFilename);

        WriteInfoFile(result, resultFilename, reads, totalQueryCount, allmapped);

        Progress.SetMessage("writing mapping details...");
        new FeatureItemGroupXmlFormatHand().WriteToFile(mappedfile, allmapped);
        //new FeatureItemGroupXmlFormat().WriteToFile(mappedfile, allmapped);
        Progress.SetMessage("writing mapping details done.");

        result.Add(mappedfile);
      }

      Progress.End();

      return result;
    }

    private void WriteInfoFile(List<string> result, string resultFilename, List<SAMAlignedItem> reads, int totalQueryCount, List<FeatureItemGroup> allmapped)
    {
      var totalMappedCount = (from q in reads select q.Qname.StringBefore(SmallRNAConsts.NTA_TAG)).Distinct().Sum(m => Counts.GetCount(m));

      var infoFile = Path.ChangeExtension(resultFilename, ".info");
      WriteSummaryFile(allmapped, totalQueryCount, totalMappedCount, infoFile);
      result.Add(infoFile);
    }

    protected void MapReadToSequenceRegion(List<FeatureLocation> mapped, List<SAMAlignedItem> reads)
    {
      var chrStrandMatchedMap = BuildStrandMap(reads);

      //First of all, mapping to miRNA
      var miRNAs = mapped.Where(m => m.Category.Equals(SmallRNAConsts.miRNA)).ToList();
      if (miRNAs.Count > 0)
      {
        Progress.SetMessage("Mapping reads to {0} miRNA entries.", miRNAs.Count);

        MapReadsToMiRNA(miRNAs, chrStrandMatchedMap);

        var mappedReads = GetMappedReads(miRNAs);
        Progress.SetMessage("There are {0} SAM entries mapped to miRNA entries.", mappedReads.Count);

        //remove reads mapped to miRNA
        RemoveReadsFromMap(chrStrandMatchedMap, mappedReads);
      }

      //remove reads with NTA
      foreach (var strandMap in chrStrandMatchedMap.Values)
      {
        foreach (var locList in strandMap.Values)
        {
          locList.RemoveAll(k => k.Parent.Qname.Contains(SmallRNAConsts.NTA_TAG) && k.Parent.Qname.StringAfter(SmallRNAConsts.NTA_TAG).Length > 0);
        }
      }

      //Secondly, mapping to tRNA 
      var tRNAs = mapped.Where(m => m.Category.Equals(SmallRNAConsts.tRNA)).ToList();
      if (tRNAs.Count > 0)
      {
        Progress.SetMessage("Mapping reads to {0} tRNA entries.", tRNAs.Count);

        MapReadsToSmallRNA(tRNAs, chrStrandMatchedMap);

        var mappedReads = GetMappedReads(tRNAs);
        Progress.SetMessage("There are {0} SAM entries mapped to tRNA entries.", mappedReads.Count);

        //remove reads mapped to tRNA
        RemoveReadsFromMap(chrStrandMatchedMap, mappedReads);
      }

      //mapping to other smallRNA
      var otherRNAs = mapped.Where(m => !m.Category.Equals(SmallRNAConsts.miRNA) && !m.Category.Equals(SmallRNAConsts.tRNA) && !m.Category.Equals(SmallRNAConsts.lincRNA) && !m.Name.Contains("SILVA_")).ToList();
      if (otherRNAs.Count > 0)
      {
        Progress.SetMessage("Mapping reads to {0} other smallRNA entries.", otherRNAs.Count);

        MapReadsToSmallRNA(otherRNAs, chrStrandMatchedMap);

        var otherReads = GetMappedReads(otherRNAs);
        Progress.SetMessage("There are {0} SAM entries mapped to other smallRNA entries.", otherReads.Count);

        //remove reads mapped to tRNA
        RemoveReadsFromMap(chrStrandMatchedMap, otherReads);
      }

      //mapping to lincRNA
      var lincRNAs = mapped.Where(m => m.Category.Equals(SmallRNAConsts.lincRNA) ||  m.Name.Contains("SILVA_")).ToList();
      if (lincRNAs.Count > 0)
      {
        Progress.SetMessage("Mapping reads to {0} lincRNA entries.", lincRNAs.Count);

        MapReadsToLincRNA(lincRNAs, chrStrandMatchedMap);

        var lincReads = GetMappedReads(lincRNAs);
        Progress.SetMessage("There are {0} SAM entries mapped to lincRNA entries.", lincReads.Count);

        //remove reads mapped to tRNA
        RemoveReadsFromMap(chrStrandMatchedMap, lincReads);
      }
    }
  }
}