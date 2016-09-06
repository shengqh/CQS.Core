﻿using System.Linq;
using System.Collections.Generic;
using System;
using System.IO;
using RCPA;

namespace CQS.Genome.SmallRNA
{
  public class SmallRNASequence
  {
    public string Sample { get; set; }
    public string Sequence { get; set; }
    public int Count { get; set; }
    public override string ToString()
    {
      return string.Format("{0} : {1} : {2}", Sample, Sequence, Count);
    }
  }

  public class SmallRNASequenceContig
  {
    public SmallRNASequenceContig()
    {
      ContigSequence = string.Empty;
      ContigCount = 0;
      Sequences = new List<SmallRNASequence>();
    }
    public string ContigSequence { get; set; }
    public int ContigCount { get; set; }
    public List<SmallRNASequence> Sequences { get; private set; }
    public override string ToString()
    {
      return string.Format("{0} : {1}", ContigSequence, ContigCount);
    }
  }

  public class SmallRNASequenceContigFormat : IFileWriter<List<SmallRNASequenceContig>>
  {
    public void WriteToFile(string fileName, List<SmallRNASequenceContig> items)
    {
      var samples = (from item in items
                     from seq in item.Sequences
                     select seq.Sample).Distinct().OrderBy(m => m).ToList();

      using (var sw = new StreamWriter(fileName))
      {
        sw.WriteLine("Sequence\t{0}", samples.Merge("\t"));
        foreach (var sc in items)
        {
          sw.WriteLine("{0}\t{1}", sc.ContigSequence, (from sample in samples
                                                       select (from seq in sc.Sequences.Where(l => l.Sample.Equals(sample))
                                                               select seq.Count).Sum().ToString()).Merge("\t"));
        }
      }
    }
  }

  public class SmallRNASequenceContigDetailFormat : IFileWriter<List<SmallRNASequenceContig>>
  {
    public void WriteToFile(string fileName, List<SmallRNASequenceContig> items)
    {
      var samples = (from item in items
                     from seq in item.Sequences
                     select seq.Sample).Distinct().OrderBy(m => m).ToList();

      using (var sw = new StreamWriter(fileName))
      {
        sw.WriteLine("ContigSequence\tSequence\t{0}", samples.Merge("\t"));
        foreach (var sc in items)
        {
          var uniqseqs = (from s in sc.Sequences
                          select s.Sequence).Distinct().OrderBy(m => sc.ContigSequence.IndexOf(m)).ThenBy(m => m.Length).ToArray();
          foreach (var uniqseq in uniqseqs)
          {
            var index = sc.ContigSequence.IndexOf(uniqseq);
            var curseq = new string(' ', index) + uniqseq + new string(' ', sc.ContigSequence.Length - uniqseq.Length - index);
            sw.WriteLine("{0}\t{1}\t{2}", sc.ContigSequence, curseq, (from sample in samples
                                                                      select (from seq in sc.Sequences.Where(l => l.Sample.Equals(sample) && l.Sequence.Equals(uniqseq))
                                                                              select seq.Count).Sum().ToString()).Merge("\t"));
          }
        }
      }
    }
  }

  public class SmallRNASequenceContigFastaFormat : IFileWriter<List<SmallRNASequenceContig>>
  {
    private int fastaCount;

    public SmallRNASequenceContigFastaFormat(int fastaCount)
    {
      this.fastaCount = fastaCount;
    }

    public void WriteToFile(string fileName, List<SmallRNASequenceContig> items)
    {
      using (var sw = new StreamWriter(fileName))
      {
        int number = 0;
        foreach (var sc in items)
        {
          sw.WriteLine(">{0}_{1}", sc.ContigSequence, sc.ContigCount);
          sw.WriteLine("{0}", sc.ContigSequence);
          number++;
          if (number == fastaCount)
          {
            break;
          }
        }
      }
    }
  }
}
