using System;
using System.Collections.Generic;
using System.IO;

namespace CQS.BreastCancer.parser
{
  public static class ParserFactory
  {
    private static Dictionary<string, IBreastCancerSampleInfoParser2> parsers;

    private static Dictionary<string, IBreastCancerSampleInfoParser2> Parsers()
    {
      if (parsers == null)
      {
        parsers = new Dictionary<string, IBreastCancerSampleInfoParser2>();
        parsers.Add("GSE1561", new GSE1561Parser());
        parsers.Add("GSE2990", new GSE2990Parser());
      }
      return parsers;
    }

    public static bool HasParserFor(string dataset)
    {
      return Parsers().ContainsKey(dataset);
    }

    public static IBreastCancerSampleInfoParser2 GetParser(string dataset)
    {
      if (Parsers().ContainsKey(dataset))
      {
        return Parsers()[dataset];
      }
      else
      {
        return null;
      }
    }

    public static IBreastCancerSampleInfoParser2 GetParserInDirectory(string directory)
    {
      var dirname = Path.GetFileName(directory);
      //if (!dirname.StartsWith("GSE"))
      //{
      //  return new EBIDatasetParser();
      //}

      CompositeParser2 parser = new CompositeParser2();
      var fixedParser = GetParser(dirname);
      if (fixedParser != null)
      {
        parser.Add(fixedParser);
      }

      var siformat = Directory.GetFiles(directory, "*.siformat");
      if (siformat.Length > 0)
      {
        var siParser = new PropertyMappingParser(siformat[0]);
        parser.Add(siParser);
      }

      if (parser.Count == 0)
      {
        throw new Exception("I don't know how to parse the information of " + dirname);
      }

      if (parser.Count == 1)
      {
        return parser[0];
      }

      return parser;
    }
  }
}
