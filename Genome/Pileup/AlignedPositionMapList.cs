using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CQS.Genome.Sam;
using CQS.Genome.Statistics;

namespace CQS.Genome.Pileup
{
  public class AlignedPositionMapList
  {
    public AlignedPositionMapList()
    {
      this.Positions = new List<AlignedPositionMap>();
      this.PositionMap = new Dictionary<long, AlignedPositionMap>();
    }

    public void Clear()
    {
      this.Positions = new List<AlignedPositionMap>();
      this.PositionMap = new Dictionary<long, AlignedPositionMap>();
    }

    public string Chromosome
    {
      get
      {
        if (this.Positions.Count > 0)
        {
          return this.Positions[0].Chromosome;
        }
        else
        {
          return string.Empty;
        }
      }
    }

    public long Position
    {
      get
      {
        if (this.Positions.Count > 0)
        {
          return this.Positions[0].Position;
        }
        else
        {
          return -1;
        }
      }
    }

    /// <summary>
    /// Current uncompleted positions
    /// </summary>
    public List<AlignedPositionMap> Positions { get; private set; }

    private Dictionary<long, AlignedPositionMap> PositionMap { get; set; }

    /// <summary>
    /// Add alignment result and return the completed positions
    /// </summary>
    /// <param name="item">alignment result</param>
    /// <returns>completed positions</returns>
    public List<AlignedPositionMap> Add(SAMAlignedItem item)
    {
      List<AlignedPositionMap> result = null;

      //if the alignment result moves to another chromosome, all uncompleted positions
      //will be completed.
      if (!item.Locations[0].Seqname.Equals(this.Chromosome))
      {
        result = Positions;
        Positions = new List<AlignedPositionMap>();
        PositionMap = new Dictionary<long, AlignedPositionMap>();
      }
      else if (this.Position != -1)
      {
        //if the alignment result position is larger than the last position in the uncompleted positions,
        //all uncompleted positions will be completed.
        if (item.Pos > this.Positions.Last().Position)
        {
          result = Positions;
          Positions = new List<AlignedPositionMap>();
          PositionMap = new Dictionary<long, AlignedPositionMap>();
        }
        else
        {
          //set up the completed list
          result = new List<AlignedPositionMap>();
          while (Positions[0].Position < item.Pos)
          {
            result.Add(Positions[0]);
            PositionMap.Remove(Positions[0].Position);
            Positions.RemoveAt(0);
          }
        }
      }

      List<AlignedPosition> align = item.GetAlignedPositions();
      foreach (var asp in align)
      {
        AlignedPositionMap dic;
        if (!PositionMap.TryGetValue(asp.Position, out dic))
        {
          dic = new AlignedPositionMap()
          {
            Chromosome = item.Locations[0].Seqname,
            Position = asp.Position
          };
          Positions.Add(dic);
          PositionMap[dic.Position] = dic;
        }

        List<AlignedPosition> curcount;
        if (!dic.TryGetValue(asp.AlignedEvent, out curcount))
        {
          curcount = new List<AlignedPosition>();
          dic[asp.AlignedEvent] = curcount;
        }
        curcount.Add(asp);
      }

      return result;
    }
  }
}
