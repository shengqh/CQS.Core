using System;
using Bio.IO.SAM;

namespace CQS.Genome.Fastq
{
  public class FastqItem
  {
    private string _pairName;

    public string Qname { get; set; }

    public SAMFlags Flags { get; set; }

    public string PairName
    {
      get { return string.IsNullOrEmpty(_pairName) ? Qname : _pairName; }
      set { _pairName = value; }
    }

    public short PairIndex { get; set; }

    public string Sequence { get; set; }

    public string Qual { get; set; }

    private bool? _ispaired;
    public bool IsPaired
    {
      get
      {
        if (_ispaired.HasValue)
        {
          return _ispaired.Value;
        }

        if (Flags.HasFlag(SAMFlags.PairedRead))
        {
          return true;
        }

        return Qname.EndsWith("/1") || Qname.EndsWith("/2");
      }
      set
      {
        _ispaired = value;
      }
    }

    public void CheckPairedName()
    {
      if (Qname.EndsWith("/1"))
      {
        PairIndex = 1;
        PairName = Qname.Substring(0, Qname.Length - 2);
      }
      else if (Qname.EndsWith("/2"))
      {
        PairIndex = 2;
        PairName = Qname.Substring(0, Qname.Length - 2);
      }
      else if (Flags.HasFlag(SAMFlags.FirstReadInPair))
      {
        PairIndex = 1;
        PairName = Qname;
      }
      else if (Flags.HasFlag(SAMFlags.SecondReadInPair))
      {
        PairIndex = 2;
        PairName = Qname;
      }
      else
      {
        throw new ArgumentException(string.Format("Query name is not in pair format (should be end with /1 or /2): {0}", Qname));
      }

      if (PairName.EndsWith("#0"))
      {
        PairName = PairName.Substring(0, PairName.Length - 2);
      }
    }

    public void WriteFastq(System.IO.StreamWriter sw)
    {
      if (string.IsNullOrEmpty(Qname))
      {
        throw new Exception(string.Format("Pairname={0}", PairName));
      }
      if (PairIndex > 0)
      {
        sw.WriteLine("@{0}/{1}", PairName, PairIndex);
      }
      else
      {
        sw.WriteLine("@" + Qname);
      }
      sw.WriteLine(Sequence);
      sw.WriteLine("+");
      sw.WriteLine(Qual);
    }

  }
}