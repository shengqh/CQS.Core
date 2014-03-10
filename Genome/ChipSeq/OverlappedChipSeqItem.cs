using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RCPA;

namespace CQS.Genome.ChipSeq
{
  public class OverlappedChipSeqItem : List<ChipSeqItem>
  {
    public void CalculateFields()
    {
      _fileCount = (from item in this
                    select item.Filename).Distinct().Count();

      _start = this.Min(m => m.Start);
      _end = this.Max(m => m.End);
      _treatmentCount = this.Sum(m => m.TreatmentCount);
      _controlCount = this.Sum(m => m.ControlCount);

      if (_controlCount == 0)
      {
        if (_treatmentCount == 0)
        {
          _ratio = 1;
        }
        else
        {
          _ratio = 100;
        }
      }
      else
      {
        _ratio = _treatmentCount / _controlCount;
      }
    }

    public void InitializeDetails(Dictionary<string, string> filenameMap)
    {
      var keys = (from m in this
                  select m.Filename).Distinct().OrderBy(m => m).ToList();

      var dics = this.GroupBy(m => m.Filename).ToDictionary(m => m.Key);

      _details = (from key in keys
                  let items = dics[key]
                  let name = filenameMap == null ? key : filenameMap[key]
                  let exp = name + ":" + (from item in items select item.CounAndFactor).Merge("/")
                  select exp).Merge(";");
    }

    public static Dictionary<string, List<OverlappedChipSeqItem>> Build(List<ChipSeqItem> items)
    {
      var result = new Dictionary<string, List<OverlappedChipSeqItem>>();

      var grp = items.GroupBy(m => m.GeneSymbol);
      foreach (var g in grp)
      {
        var currResult = new List<OverlappedChipSeqItem>();
        var cs = g.OrderBy(m => m.Start).ToList();
        var r = new OverlappedChipSeqItem();
        r.Add(cs[0]);
        currResult.Add(r);

        for (int i = 1; i < cs.Count; i++)
        {
          AddToList(currResult, cs[i]);
        }

        result[g.Key] = currResult;
      }

      return result;
    }

    public static void AddToList(List<OverlappedChipSeqItem> currResult, ChipSeqItem chipSeqItem)
    {
      var maxpercentage = 0.0;
      OverlappedChipSeqItem maxcr = null;
      foreach (var cr in currResult)
      {
        foreach (var item in cr)
        {
          var oc = item.GetOverlapPercentage(chipSeqItem);
          if (oc > maxpercentage)
          {
            maxcr = cr;
            maxpercentage = oc;
          }
        }
      }

      if (maxcr != null)
      {
        maxcr.Add(chipSeqItem);
      }
      else
      {
        maxcr = new OverlappedChipSeqItem();
        maxcr.Add(chipSeqItem);
        currResult.Add(maxcr);
      }
    }

    public string Chromosome
    {
      get
      {
        if (this.Count == 0)
        {
          return string.Empty;
        }

        return this[0].Chromosome;
      }
    }

    private int _fileCount;

    public int FileCount
    {
      get
      {
        return _fileCount;
      }
    }

    private int _start;

    public int Start
    {
      get
      {
        return _start;
      }
    }

    private int _end;

    public int End
    {
      get
      {
        return _end;
      }
    }

    public int MappedLength
    {
      get
      {
        return End - Start + 1;
      }
    }

    public string GeneSymbol
    {
      get
      {
        if (this.Count == 0)
        {
          return string.Empty;
        }

        return this[0].GeneSymbol;
      }
    }


    public string OverlapType
    {
      get
      {
        if (this.Count == 0)
        {
          return string.Empty;
        }

        return this[0].OverlapType;
      }
    }

    private double _treatmentCount;

    public double TreatmentCount
    {
      get
      {
        return _treatmentCount;
      }
    }

    private double _controlCount;

    public double ControlCount
    {
      get
      {
        return _controlCount;
      }
    }

    private double _ratio;
    public double Ratio
    {
      get
      {
        return _ratio;
      }
    }

    private string _details;
    public string Details
    {
      get
      {
        return _details;
      }
    }
  }
}
