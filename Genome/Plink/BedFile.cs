using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CQS.Genome.Plink
{
  public class BedFile
  {
    private BinaryReader reader;
    public BedFile(string fileName)
    {
      using (this.reader = new BinaryReader(new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
      {
        var magic = reader.ReadBytes(8);
        foreach (var b in magic)
        {
          Console.WriteLine(b);
        }
      }
    }
  }
}
