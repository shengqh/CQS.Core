using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RCPA.Utils;
using System.IO;

namespace CQS.Genome.Sam
{
  public static class SAMFactory
  {
    public static ISAMFile GetReader(string filename, string samtools = null, bool skipHeaders = false)
    {
      ISAMFile result = null;
      if (SAMUtils.IsBAMFile(filename))
      {
        if (SystemUtils.IsLinux)
        {
          if (!File.Exists(samtools))
          {
            throw new FileNotFoundException(string.Format("Samtools not found {0}", samtools));
          }
        }
        else
        {
          result = new BAMWindowReader(filename);
        }
      }

      if (null == result)
      {
        result = new SAMLinuxReader(samtools, filename);
      }

      if (skipHeaders)
      {
        result.ReadHeaders();
      }

      return result;
    }
  }
}
