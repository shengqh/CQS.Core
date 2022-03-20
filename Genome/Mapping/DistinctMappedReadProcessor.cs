using RCPA;
using System.Collections.Generic;
using System.Linq;

namespace CQS.Genome.Mapping
{
  public class DistinctMappedReadProcessor : AbstractThreadProcessor
  {
    private readonly DistinctMappedReadProcessorOptions _options;

    public DistinctMappedReadProcessor(DistinctMappedReadProcessorOptions options)
    {
      _options = options;
    }

    public override IEnumerable<string> Process()
    {
      var result = new List<string>();
      var samformat = _options.GetEngineFormat();

      var format = new MappedItemGroupXmlFileFormat();

      Progress.SetMessage("reading mapped reads from " + _options.InputFile1 + " ...");
      var items1 = format.ReadFromFile(_options.InputFile1);

      Progress.SetMessage("reading mapped reads from " + _options.InputFile2 + " ...");
      var items2 = format.ReadFromFile(_options.InputFile2);

      var reads1 = items1.GetQueries().ToDictionary(m => m.Qname);
      var reads2 = items2.GetQueries().ToDictionary(m => m.Qname);

      var qnames = reads1.Keys.Union(reads2.Keys).Distinct().ToList();
      foreach (var qname in qnames)
      {
        if (!reads1.ContainsKey(qname) || !reads2.ContainsKey(qname))
          continue;

        var r1 = reads1[qname];
        var r2 = reads2[qname];
        var res = samformat.CompareScore(r1.AlignmentScore, r2.AlignmentScore);
        if (res == 0)
        {
          items1.RemoveRead(qname);
          items2.RemoveRead(qname);
        }
        else if (res < 0)
        {
          items2.RemoveRead(qname);
        }
        else
        {
          items1.RemoveRead(qname);
        }
      }

      var writer = new MappedItemGroupSequenceWriter();

      SaveItems(items1, _options.OutputFile1, writer, format, result);
      SaveItems(items2, _options.OutputFile2, writer, format, result);

      return result;
    }

    private static void SaveItems(List<MappedItemGroup> items1, string outputFile, MappedItemGroupSequenceWriter writer,
      MappedItemGroupXmlFileFormat format, List<string> result)
    {
      items1.RemoveAll(m => m.QueryCount == 0);
      var xml1 = outputFile + ".xml";

      if (items1.Any(m => m.Name.Contains(".tRNA")))
      {
        items1.SortTRna();
      }

      writer.WriteToFile(outputFile, items1);
      format.WriteToFile(xml1, items1);
      result.Add(outputFile);
      result.Add(xml1);
    }
  }
}