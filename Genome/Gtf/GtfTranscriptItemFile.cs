using System;
using System.IO;

namespace CQS.Genome.Gtf
{
  public class GtfTranscriptItemFile
  {
    private GtfItemFile file;
    private GtfItem last;

    public GtfTranscriptItemFile()
    {
      file = null;
      last = null;
    }

    public GtfTranscriptItemFile(string filename)
      : this()
    {
      Open(filename);
    }

    public virtual void Open(string filename)
    {
      if (file == null)
      {
        file = new GtfItemFile();
      }
      file.Open(filename);
    }

    public virtual void Close()
    {
      if (file != null)
      {
        file.Close();
        file = null;
      }
    }

    public virtual void Reset()
    {
      CheckFileOpened();

      if (file != null)
      {
        file.Reset();
      }
    }

    private void CheckFileOpened()
    {
      if (file == null)
      {
        throw new FileNotFoundException("Open file first.");
      }
    }

    public virtual GtfTranscriptItem Next(bool exonOnly = true)
    {
      CheckFileOpened();

      GtfTranscriptItem result = new GtfTranscriptItem();
      if (this.last != null)
      {
        result.Add(last);
      }

      Func<GtfItem> getItem;
      if (exonOnly)
      {
        getItem = () => file.NextExon();
      }
      else
      {
        getItem = () => file.Next();
      }

      while ((last = getItem()) != null)
      {
        if (result.IsSameTranscript(last))
        {
          if (last.Strand == '-')
          {
            result.Insert(0, last);
          }
          else
          {
            result.Add(last);
          }
        }
        else
        {
          break;
        }
      }

      if (result.Count > 0)
      {
        return result;
      }
      else
      {
        return null;
      }
    }
  }
}
