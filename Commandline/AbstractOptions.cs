using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommandLine.Text;
using CommandLine;

namespace CQS.Commandline
{
  public abstract class AbstractOptions
  {
    public AbstractOptions()
    {
      this.IsPileup = false;
    }

    public bool IsPileup { get; set; }

    private List<string> parsingErrors = new List<string> ();

    public List<string> ParsingErrors
    {
      get
      {
        return parsingErrors;
      }
    }

    [ParserState]
    public IParserState LastParserState { get; set; }

    [HelpOption]
    public string GetUsage()
    {
      var result = HelpText.AutoBuild(this, (HelpText current) => HelpText.DefaultParsingErrorsHandler(this, current));

      if (parsingErrors.Count > 0)
      {
        result.AddPreOptionsLine("ERROR(S):");
        foreach (var line in parsingErrors)
        {
          result.AddPreOptionsLine("  " + line);
        }
      }

      return result;
    }

    public abstract bool PrepareOptions();
  }
}
