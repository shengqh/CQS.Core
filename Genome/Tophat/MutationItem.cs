using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CQS.Genome.Tophat
{
  public class MutationItem
  {
    public string Name { get; set; }
    public string Gene { get; set; }
    public string Line { get; set; }
    public string Chr { get; set; }
    public long Position { get; set; }
    public long JunctionDistance { get; set; }
    public JunctionItem JunctionItem { get; set; }
    public long TerminalDistance { get; set; }
    public JunctionItem TerminalItem { get; set; }
    public long MutationDistance { get; set; }
    public MutationItem NearestMutationItem { get; set; }

    public long InsertionDistance { get; set; }
    public InsertionDeletionItem InsertionItem { get; set; }
    public long DeletionDistance { get; set; }
    public InsertionDeletionItem DeletionItem { get; set; }

    public int GeneCount { get; set; }
  }
}
