using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CQS.Genome.Sam;
using RCPA.Gui;

namespace CQS.Genome.Mapping
{
  public interface ICandidateBuilder : IProgress
  {
    List<T> Build<T>(string fileName, out HashSet<string> totalQueryNames) where T : SAMAlignedItem, new();
  }
}
