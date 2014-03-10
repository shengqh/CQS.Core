//using System;
//using System.Collections.Generic;
//using System.Diagnostics.CodeAnalysis;
//using System.Text;
//using Bio.IO.SAM;
//using Bio.Util;
//using CQS.Genome.Sam;
//using Microsoft.Office.Interop.Excel;
//using RCPA.Seq;

//namespace CQS.Genome.Fastq
//{
//  /// <summary>
//  ///   This class is used for extract fastq information only.
//  /// </summary>
//  public class FastqItemSAMParser : IFastqItemParser, IDisposable
//  {
//    public HashSet<string> IgnoreQuery { get; private set; }

//    /// <summary>
//    ///   The default constructor which chooses the default encoding based on the alphabet.
//    /// </summary>
//    public FastqItemSAMParser(string samFileName)
//    {
//      IgnoreQuery = new HashSet<string>();
//    }

//    public static bool IsPaired(string samFile)
//    {
//      using (var sr = new FastqItemSAMParser(samFile))
//      {
//        FastqItem item;
//        while ((item = sr.ParseNext()) != null)
//        {
//          return item.Flags.HasFlag(SAMFlags.PairedRead);
//        }
//      }
//      return false;
//    }

//    public FastqItem ParseNext()
//    {
//      throw new NotImplementedException();
//    }
//  }
//}