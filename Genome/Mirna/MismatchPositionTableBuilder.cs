using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using RCPA;
using RCPA.Commandline;
using CommandLine;
using System.IO;
using CQS.Genome.Sam;

namespace CQS.Genome.Mirna
{
  public class MismatchPositionTableBuilderOptions : AbstractOptions
  {
    public MismatchPositionTableBuilderOptions() { }

    [Option('i', "input", Required = true, MetaValue = "FILE", HelpText = "Mapped miRNA xml file")]
    public string InputFile { get; set; }

    [Option('o', "output", Required = true, MetaValue = "FILE", HelpText = "Output tab-delimited file")]
    public string OutputFile { get; set; }

    public override bool PrepareOptions()
    {
      if (!File.Exists(this.InputFile))
      {
        ParsingErrors.Add(string.Format("File not exists {0}.", this.InputFile));
        return false;
      }

      return true;
    }
  }

  public class MismatchPositionTableBuilder : AbstractThreadFileProcessor
  {
    private Regex reg5 = new Regex(@"^0\D|^1\D");
    private Regex reg3 = new Regex(@"^0\D|^1\D|^2\D");

    private MismatchPositionTableBuilderOptions options;

    public MismatchPositionTableBuilder(MismatchPositionTableBuilderOptions options)
    {
      this.options = options;
    }

    public override IEnumerable<string> Process(string useless)
    {
      var result = new MappedMirnaGroupXmlFileFormat().ReadFromFile(options.InputFile);

      using (StreamWriter sw = new StreamWriter(options.OutputFile))
      {
        sw.WriteLine("miRNA\tLocation\tTotalCount\tPerfectMatch\tMiss5_2\tMiss3_3\tMissInternal");
        foreach (var res in result)
        {
          var items = res.GetAlignedLocations();

          if(res.DisplayName.Equals("hsa-mir-486-5p:TCCTGTACTGAGCTGCCCCGAG")){
            items.ForEach(m => Console.WriteLine(m.Parent.Qname + "\t" + m.Strand + "\t" + m.MismatchPositions));
          }
          var pmcount = items.Count(m => m.NumberOfMismatch == 0);
          var mis5 = items.Count(m =>
          {
            SAMAlignedLocation loc = m;

            if (loc.NumberOfMismatch == 0)
            {
              return false;
            }

            var mp = loc.MismatchPositions;
            if (loc.Strand == '-')
            {
              mp = new string(mp.Reverse().ToArray());
            }

            return reg5.Match(mp).Success;
          });

          var mis3 = items.Count(m =>
          {
            var loc = m;
            if (loc.NumberOfMismatch == 0)
            {
              return false;
            }

            var mp = loc.MismatchPositions;
            if (loc.Strand == '+')
            {
              mp = new string(mp.Reverse().ToArray());
            }

            return reg3.Match(mp).Success;
          });
          sw.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}", res.DisplayName, res.DisplayLocation, items.Count, pmcount, mis5, mis3, items.Count - pmcount - mis5 - mis3);
        }
      }
      return new string[] { options.OutputFile };
    }

    private void FindLocation(List<SAMAlignedLocation> list, List<MappedMirnaRegion> list_2, out SAMAlignedLocation loc, out MappedMirnaRegion reg)
    {
      throw new NotImplementedException();
    }
  }
}
