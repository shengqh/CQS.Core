using RCPA;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CQS.Genome.Bacteria.Rockhopper
{
  public class RockhopperSummaryBuilder : AbstractThreadProcessor
  {
    private RockhopperSummaryBuilderOptions _options;

    public RockhopperSummaryBuilder(RockhopperSummaryBuilderOptions options)
    {
      this._options = options;
    }

    private Dictionary<string, Tuple<string, string>> ReadFastqSampleGroupMap()
    {
      var result = new Dictionary<string, Tuple<string, string>>();
      foreach (var line in File.ReadAllLines(_options.MapFile).Skip(1))
      {
        var parts = line.Split('\t');
        if (parts.Length > 2)
        {
          result[parts[0]] = new Tuple<string, string>(parts[1], parts[2]);
        }
      }
      return result;
    }

    private RockhopperSummary ParseRockhopper(string comparison, string summaryfile)
    {
      var result = new RockhopperSummaryReader().ReadFromFile(summaryfile);
      result.ComparisonName = comparison;
      return result;
    }

    private static void WriteToFile(string fileName, List<RockhopperTranscriptResult> data, List<string> genes, Func<RockhopperTranscript, bool> significantFilter)
    {
      using (var sw = new StreamWriter(fileName))
      {
        sw.Write("Gene,Product");
        foreach (var d in data)
        {
          sw.Write(",{0}_log2(FoldChange),{0}_qValue", d.ComparisonName);
        }
        sw.WriteLine(",Significant");

        foreach (var gene in genes)
        {
          sw.Write(gene);

          var synonym = gene.StringBefore(",");
          List<string> significant = new List<string>();
          foreach (var d in data)
          {
            if (d.UnpredictedMap.ContainsKey(synonym))
            {
              var entry = d.UnpredictedMap[synonym];
              sw.Write(",{0:0.00},{1}", entry.FoldChange, entry.Qvalue);
              if (significantFilter(entry))
              {
                significant.Add(d.ComparisonName);
              }
            }
            else
            {
              sw.Write(",0,1");
            }
          }

          sw.WriteLine(",{0}", significant.Merge(";"));
        }
      }
    }

    public override IEnumerable<string> Process()
    {
      List<string> result = new List<string>();

      var fsgMap = ReadFastqSampleGroupMap();

      var dirs = _options.ComparisonDirs;

      var summaryData = (from dir in dirs
                         let comparison = Path.GetFileName(dir)
                         let summaryfile = dir + "/summary.txt"
                         select ParseRockhopper(comparison, summaryfile)).OrderBy(m => m.ComparisonName).ToList();

      string comparisonfile = _options.TargetDir + "/" + _options.Prefix + "summary_comparison.csv";
      result.Add(comparisonfile);

      using (var sw = new StreamWriter(comparisonfile))
      {
        sw.WriteLine("Comparison,DiffGenes");

        foreach (var map in summaryData)
        {
          sw.WriteLine("{0},{1}", map.ComparisonName, map.DifferentialGenes);
        }
      }

      var mappings = (from d in summaryData
                      from f in d.MappingResults
                      select f).GroupBy(m => m.FileName).OrderBy(m => fsgMap[m.Key].Item1).ToList();

      string mappingfile = _options.TargetDir + "/" + _options.Prefix + "summary_mapping.csv";
      result.Add(mappingfile);
      using (var sw = new StreamWriter(mappingfile))
      {
        sw.WriteLine("Group,File,TotalReads,AlignedReads,AlignedReadsPercentage,ProteinSenseReads,ProteinAntisenseReads,RibosomalRNASenseReads,UnannotatedReads");
        foreach (var map in mappings)
        {
          var fr = map.First();
          sw.WriteLine("{0},{1},{2},{3},{4},{5},{6},{7},{8}",
            fsgMap[fr.FileName].Item2,
            fsgMap[fr.FileName].Item1,
            fr.TotalReads,
            fr.AlignedReads,
            fr.AlignedReadsPercentage,
            fr.ProteinReadsSense,
            fr.ProteinReadsAntisense,
            fr.RibosomalRNAReadsSense,
            fr.UnannotatedRead);
        }
      }

      var transcripts = (from dir in dirs
                         let comparison = Path.GetFileName(dir)
                         let transcriptFile = Directory.GetFiles(dir).Where(m => m.EndsWith("_transcripts.txt")).First()
                         let alllines = File.ReadAllLines(transcriptFile)
                         let headers = alllines[0].Split('\t').ToList()
                         let rpkm1 = headers.FindIndex(m => m.StartsWith("RPKM"))
                         let group1 = headers[rpkm1].StringAfter("RPKM").Trim()
                         let rpkm2 = headers.FindIndex(rpkm1 + 1, m => m.StartsWith("RPKM"))
                         let group2 = headers[rpkm2].StringAfter("RPKM").Trim()
                         let qvalue = headers.FindIndex(m => m.StartsWith("qValue"))
                         let lines = alllines.Skip(1)
                         let list = (from l in lines
                                     let parts = l.Split('\t')
                                     where parts.Length > 8
                                     let rpkm1value = double.Parse(parts[rpkm1])
                                     let rpkm2value = double.Parse(parts[rpkm2])
                                     let foldchange = rpkm2value == 0.0 ? (rpkm1value == 0.0 ? 0 : 50) : (rpkm1value == 0.0 ? -50 : Math.Log(rpkm1value / rpkm2value, 2))
                                     select new RockhopperTranscript()
                                     {
                                       TranscriptionStart = parts[0],
                                       TranslationStart = parts[1],
                                       TranslationEnd = parts[2],
                                       TranscriptionEnd = parts[3],
                                       Strand = parts[4],
                                       Name = parts[5],
                                       Synonym = parts[6],
                                       Product = parts[7],
                                       RPKM1 = rpkm1value,
                                       RPKM2 = rpkm2value,
                                       FoldChange = foldchange,
                                       Qvalue = parts[qvalue]
                                     }).ToList()
                         select new RockhopperTranscriptResult()
                         {
                           ComparisonName = comparison,
                           Group1 = group1,
                           Group2 = group2,
                           DataList = list,
                           UnpredictedMap = (from l in list
                                             where !l.Synonym.StartsWith("predicted")
                                             select l).ToDictionary(m => m.Synonym)
                         }).ToList();

      var genes = (from d in transcripts
                   from k in d.UnpredictedMap.Values
                   select k.Synonym + ",\"" + k.Product + "\"").Distinct().OrderBy(m => m).ToList();

      var minLog2FoldChange = Math.Log(_options.MinFoldChange, 2);

      var tanscriptsummaryfile = _options.TargetDir + "/" + _options.Prefix + "summary_transcripts.csv";
      WriteToFile(tanscriptsummaryfile, transcripts, genes, entry => Math.Abs(entry.FoldChange) >= minLog2FoldChange && double.Parse(entry.Qvalue) <= _options.MaxQvalue);
      result.Add(tanscriptsummaryfile);

      foreach (var d in transcripts)
      {
        var file = _options.TargetDir + "/" + _options.Prefix + "transcripts_" + d.ComparisonName + ".csv";
        result.Add(file);
        using (var sw = new StreamWriter(file))
        {
          sw.WriteLine("TranscriptionStart,TranslationStart,TranslationStop,TranscriptionStop,Strand,Name,Synonym,Product");
          foreach (var l in d.DataList)
          {
            sw.WriteLine("{0},{1},{2},{3},{4},{5},{6},{7}",
              l.TranscriptionStart,
              l.TranslationStart,
              l.TranslationEnd,
              l.TranscriptionEnd,
              l.Strand,
              l.Name,
              l.Synonym,
              l.Product);
          }
        }
      }

      foreach (var dir in dirs)
      {
        var comparison = Path.GetFileName(dir);
        var operons = File.ReadAllLines(Directory.GetFiles(dir).Where(m => m.EndsWith("_operons.txt")).First());
        var file = _options.TargetDir + "/" + _options.Prefix + "operons_" + comparison + ".csv";
        result.Add(file);
        using (var sw = new StreamWriter(file))
        {
          foreach (var operon in operons)
          {
            sw.WriteLine(operon.Split('\t').ToList().ConvertAll(m => "\"" + m + "\"").Merge(","));
          }
        }
      }

      return result;
    }
  }
}
