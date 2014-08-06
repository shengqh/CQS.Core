using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CQS.Genome.Bacteria.Rockhopper
{
  public class RockhopperTranscript
  {
    public string TranscriptionStart { get; set; }
    public string TranslationStart { get; set; }
    public string TranslationEnd { get; set; }
    public string TranscriptionEnd { get; set; }
    public string Strand { get; set; }
    public string Name { get; set; }
    public string Synonym { get; set; }
    public string Product { get; set; }
    public string Group1 { get; set; }
    public double RPKM1 { get; set; }
    public string Group2 { get; set; }
    public double RPKM2 { get; set; }
    public double FoldChange { get; set; }
    public string Qvalue { get; set; }
  }
}
