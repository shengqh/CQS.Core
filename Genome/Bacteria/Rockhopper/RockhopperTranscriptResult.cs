﻿using System.Collections.Generic;

namespace CQS.Genome.Bacteria.Rockhopper
{
  public class RockhopperTranscriptResult
  {
    public string ComparisonName { get; set; }
    public string Group1 { get; set; }
    public string Group2 { get; set; }
    public List<RockhopperTranscript> DataList { get; set; }
    public Dictionary<string, RockhopperTranscript> UnpredictedMap { get; set; }
  }
}
