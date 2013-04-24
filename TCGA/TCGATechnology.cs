using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CQS.TCGA.TCGATechnologyImpl;

namespace CQS.TCGA
{
  public static class TCGATechnology
  {
    public static readonly ITCGATechnology RNAseq_RPKM = new TCGATechnologyRNAseqV1();
    
    public static readonly ITCGATechnology RNAseq_RSEM = new TCGATechnologyRNAseqV2();

    public static readonly ITCGATechnology MirnaSeq = new TCGATechnologyMirnaSeq();

    public static readonly ITCGATechnology Microarray = new TCGATechnologyMicroarray();

    public static readonly ITCGATechnology Mirna = new TCGATechnologyMirna();
  }
}
