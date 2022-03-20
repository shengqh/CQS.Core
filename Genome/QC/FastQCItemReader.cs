using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CQS.Genome.QC
{
  public class FastQCItemReader
  {
    public List<FastQCItem> ReadFromRootDirectory(string rootDirectory)
    {
      return (from dir in Directory.GetDirectories(rootDirectory)
              let item = FastQCItem.ParseFromDirectory(dir)
              where item != null
              select item).ToList();
    }
  }
}
