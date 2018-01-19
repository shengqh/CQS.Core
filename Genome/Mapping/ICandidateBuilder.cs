using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CQS.Genome.Sam;
using RCPA.Gui;

namespace CQS.Genome.Mapping
{
  public class QueryInfo
  {
    public QueryInfo(string name)
    {
      Name = name;
      Mismatch = -1;
      Length = -1;
    }

    public string Name { get; set; }
    public int Length { get; set; }
    public int Mismatch { get; set; }
  }

  public interface ICandidateBuilder : IProgress
  {
    List<T> Build<T>(string fileName, out List<QueryInfo> totalQueries) where T : SAMAlignedItem, new();
  }
}
