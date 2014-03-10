using RCPA;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CQS.Genome.QC
{
  public class SamToolsStatItemReader : IFileReader<SamToolsStatItem>
  {
    class Function
    {
      public Function(Func<string, bool> Accept, Action<int, SamToolsStatItem> AssignValue)
      {
        this.Accept = Accept;
        this.AssignValue = AssignValue;
      }

      public Func<string, bool> Accept { get; set; }

      public Action<int, SamToolsStatItem> AssignValue { get; set; }

      public bool Assign(string line, SamToolsStatItem item)
      {
        if (Accept(line))
        {
          var count = int.Parse(line.StringBefore("+").Trim());
          AssignValue(count, item);
          return true;
        }
        return false;
      }
    }

    private static List<Function> actions;

    static SamToolsStatItemReader()
    {
      actions = new List<Function>();
      actions.Add(new Function(m => m.Contains("in total"), (m, n) => n.Total = m));
      actions.Add(new Function(m => m.Contains("duplicates"), (m, n) => n.Duplicates = m));
      actions.Add(new Function(m => m.Contains("mapped ("), (m, n) => n.Mapped = m));
      actions.Add(new Function(m => m.Contains("paired in sequencing"), (m, n) => n.PairedInSequencing = m));
      actions.Add(new Function(m => m.Contains("read1"), (m, n) => n.Read1 = m));
      actions.Add(new Function(m => m.Contains("read2"), (m, n) => n.Read2 = m));
      actions.Add(new Function(m => m.Contains("properly paired"), (m, n) => n.ProperlyPaired = m));
      actions.Add(new Function(m => m.Contains("with itself and mate mapped"), (m, n) => n.WithItselfAndMateMapped = m));
      actions.Add(new Function(m => m.Contains("singletons"), (m, n) => n.Singletons = m));
      actions.Add(new Function(m => m.EndsWith("with mate mapped to a different chr"), (m, n) => n.WithMateMappedToADifferentChr = m));
      actions.Add(new Function(m => m.Contains("with mate mapped to a different chr (mapQ"), (m, n) => n.WithMateMappedToADifferentChrMapQ = m));
    }

    public SamToolsStatItem ReadFromFile(string fileName)
    {
      var result = new SamToolsStatItem();

      using (var sr = new StreamReader(fileName))
      {
        string line;
        while ((line = sr.ReadLine()) != null)
        {
          foreach (var act in actions)
          {
            if (act.Assign(line, result))
            {
              break;
            }
          }
        }
      }

      return result;
    }
  }
}
