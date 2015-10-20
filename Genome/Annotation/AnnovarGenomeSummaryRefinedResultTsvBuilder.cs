using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RCPA;
using LumenWorks.Framework.IO.Csv;
using System.IO;
using CQS.Genome.Affymetrix;
using System.Drawing;
using System.Security.Policy;
using NPOI.SS.UserModel;
using NPOI.HSSF.Util;
using NPOI.HSSF.UserModel;
using NPOI.HPSF;
using System.Text.RegularExpressions;
using RCPA.Commandline;
using CommandLine;
using CQS.Genome.Statistics;
using CQS.Genome.SomaticMutation;

namespace CQS.Genome.Annotation
{
  public class AnnovarGenomeSummaryRefinedResultTsvBuilder : AbstractThreadProcessor
  {
    private AnnovarGenomeSummaryRefinedResultBuilderOptions options;

    public AnnovarGenomeSummaryRefinedResultTsvBuilder(AnnovarGenomeSummaryRefinedResultBuilderOptions options)
    {
      this.options = options;
    }

    public override IEnumerable<string> Process()
    {
      //Write the stream data of workbook to the root directory
      using (FileStream file = new FileStream(options.OutputFile, FileMode.Create))
      {
        var book = new HSSFWorkbook();

        //cell style for hyperlinks
        //by default hyperlinks are blue and underlined
        var hlinkStyle = book.CreateCellStyle();
        var hlinkFont = book.CreateFont();
        hlinkFont.Underline = (byte)FontUnderlineType.SINGLE;
        hlinkFont.Color = HSSFColor.BLUE.index;
        hlinkStyle.SetFont(hlinkFont);
        hlinkStyle.WrapText = true;

        var wrapStyle = book.CreateCellStyle();
        wrapStyle.WrapText = true;

        var numericStyle = book.CreateCellStyle();
        numericStyle.DataFormat = 0xb;

        var genenames = File.Exists(options.AffyAnnotationFile) ? AnnotationFile.GetGeneSymbolDescriptionMap(options.AffyAnnotationFile) : new Dictionary<string, string>();
        var all = book.CreateSheet("all");

        var item = new AnnovarGenomeSummaryItem();

        using (var sr = new StreamReader(options.InputFile))
        {
          int nRow = 0;

          bool isMutect = false;
          bool hasLod = false;
          string tumorSampleName = "", normalSampleName = "";
          string line;
          //ignore the comments
          while ((line = sr.ReadLine()) != null)
          {
            if (!line.StartsWith("#"))
            {
              break;
            }

            var row = all.CreateRow(nRow++);
            row.CreateCell(0).SetCellValue(line);

            if (line.StartsWith("##INFO=<ID=LOD"))
            {
              hasLod = true;
            }

            if (line.StartsWith("##MuTect"))
            {
              isMutect = true;
              tumorSampleName = line.StringAfter("tumor_sample_name=").StringBefore(" ");
              normalSampleName = line.StringAfter("normal_sample_name=").StringBefore(" ");
            }
          }

          if (line == null)
          {
            throw new Exception("No entries in file " + options.InputFile);
          }

          var headers = line.Split('\t').ToList();

          //original index
          var geneIndex = headers.FindIndex(m => m.Equals("Gene") || m.Equals("Gene.refGene"));
          var oldInfoIndex = headers.FindIndex(m => m.ToLower().StartsWith("info"));
          var newInfoIndex = FindIndex(oldInfoIndex, geneIndex);

          //relative index
          var funcIndex = FindIndex(geneIndex, headers.FindIndex(m => m.Equals("Func") || m.Equals("Func.refGene")));
          var exonicIndex = FindIndex(geneIndex, headers.FindIndex(m => m.Equals("ExonicFunc") || m.Equals("ExonicFunc.refGene")));
          var dbsnpIndex = FindIndex(geneIndex, headers.FindIndex(m => m.ToLower().StartsWith("dbsnp") || m.ToLower().StartsWith("snp")));
          var chrIndex = headers.IndexOf("Chr");
          var startIndex = headers.IndexOf("Start");
          var endIndex = headers.IndexOf("End");
          var tumorIndex = headers.IndexOf(tumorSampleName);
          var normalIndex = headers.IndexOf(normalSampleName);

          hasLod = hasLod && oldInfoIndex != -1;

          //handle the headers. The length of headers may less than the data.
          var firstrow = all.CreateRow(nRow++);
          for (int i = 0; i <= geneIndex; i++)
          {
            firstrow.CreateCell(i).SetCellValue(headers[i]);
          }
          firstrow.CreateCell(geneIndex + 1).SetCellValue("Description");
          for (int i = geneIndex + 1; i < headers.Count; i++)
          {
            if (isMutect)
            {
              if (i == tumorIndex)
              {
                firstrow.CreateCell(i + 1).SetCellValue("Tumor:" + tumorSampleName);
                continue;
              }

              if (i == normalIndex)
              {
                firstrow.CreateCell(i + 1).SetCellValue("Normal:" + normalSampleName);
                continue;
              }
            }
            firstrow.CreateCell(i + 1).SetCellValue(headers[i]);
          }

          var lastcol = headers.Count + 1;
          if (hasLod)
          {
            firstrow.CreateCell(lastcol++).SetCellValue("TLodFstar");
          }

          if (isMutect)
          {
            firstrow.CreateCell(lastcol++).SetCellValue("NormalAlleles");
            firstrow.CreateCell(lastcol++).SetCellValue("TumorAlleles");
            firstrow.CreateCell(lastcol++).SetCellValue("FisherExactTest");
          }

          while ((line = sr.ReadLine()) != null)
          {
            var parts = line.Split('\t');
            if (parts.Length < geneIndex)
            {
              break;
            }

            var row = all.CreateRow(nRow++);
            for (int i = 0; i < geneIndex; i++)
            {
              row.CreateCell(i).SetCellValue(parts[i]);
            }

            item.GeneString = parts[geneIndex];
            if (item.Genes.Count > 0)
            {
              //add link for gene symbol
              var cell = row.CreateCell(geneIndex);
              cell.Hyperlink = new HSSFHyperlink(HyperlinkType.URL)
              {
                Address = string.Format("http://www.genecards.org/cgi-bin/carddisp.pl?gene={0}", item.Genes[0].Name)
              };
              cell.CellStyle = hlinkStyle;
              cell.SetCellValue((from g in item.Genes select g.Name).Merge("\n"));

              //gene description
              var desCell = row.CreateCell(geneIndex + 1);
              desCell.CellStyle = wrapStyle;
              desCell.SetCellValue((from gene in item.Genes
                                    let description = genenames.ContainsKey(gene.Name) ? genenames[gene.Name] : " "
                                    select description).Merge("\n"));
            }

            //add other information
            for (int i = geneIndex + 1; i < headers.Count; i++)
            {
              row.CreateCell(i + 1).SetCellValue(parts[i]);
            }

            lastcol = headers.Count + 1;
            if (hasLod)
            {
              row.CreateCell(lastcol++).SetCellValue(parts[oldInfoIndex].StringAfter("LOD=").StringBefore(";"));
            }

            if (isMutect)
            {
              Match normal = SomaticMutationUtils.MutectPattern.Match(parts[normalIndex]);
              Match tumor = SomaticMutationUtils.MutectPattern.Match(parts[tumorIndex]);

              var fetr = new FisherExactTestResult();
              fetr.Sample1.Succeed = int.Parse(normal.Groups[1].Value);
              fetr.Sample1.Failed = int.Parse(normal.Groups[2].Value);
              fetr.Sample2.Succeed = int.Parse(tumor.Groups[1].Value);
              fetr.Sample2.Failed = int.Parse(tumor.Groups[2].Value);

              row.CreateCell(lastcol++).SetCellValue(string.Format("{0}:{1}", fetr.Sample1.Succeed, fetr.Sample1.Failed));
              row.CreateCell(lastcol++).SetCellValue(string.Format("{0}:{1}", fetr.Sample2.Succeed, fetr.Sample2.Failed));
              row.CreateCell(lastcol).SetCellValue(fetr.CalculateTwoTailPValue());
            }

            if (dbsnpIndex > 0)
            {
              var dbsnpcell = row.GetCell(dbsnpIndex);
              var dbsnp = dbsnpcell.StringCellValue;
              if (!string.IsNullOrEmpty(dbsnp))
              {
                dbsnpcell.Hyperlink = new HSSFHyperlink(HyperlinkType.URL)
                {
                  Address = string.Format("http://www.ncbi.nlm.nih.gov/projects/SNP/snp_ref.cgi?rs={0}", dbsnp.Substring(2))
                };
                dbsnpcell.CellStyle = (hlinkStyle);
              }
            }
          }


          all.SetColumnWidth(chrIndex, 5 * 256);
          all.SetColumnWidth(startIndex, 11 * 256);
          all.SetColumnWidth(endIndex, 11 * 256);
          all.SetColumnWidth(funcIndex, 15 * 256);
          all.SetColumnWidth(geneIndex, 15 * 256);
          all.SetColumnWidth(geneIndex + 1, 60 * 256);
          all.SetColumnWidth(exonicIndex, 20 * 256);
          all.SetColumnWidth(dbsnpIndex, 15 * 256);

          lastcol = headers.Count + 1;
          if (hasLod)
          {
            all.SetColumnWidth(newInfoIndex, 15 * 256);
            all.SetColumnWidth(lastcol++, 10 * 256);
          }

          if (isMutect)
          {
            all.SetColumnWidth(lastcol++, 10 * 256);
            all.SetColumnWidth(lastcol++, 10 * 256);
            all.SetColumnWidth(lastcol, 10 * 256);
          }
        }
        book.Write(file);
      }
      return new string[] { options.OutputFile };
    }

    private static int FindIndex(int geneIndex, int funcIndex)
    {
      funcIndex = funcIndex > geneIndex ? funcIndex + 1 : funcIndex;
      return funcIndex;
    }

    private static void SetAutoSizeColumn(ISheet all, string[] headers, string[] names, string[] patterns)
    {
      foreach (var name in names)
      {
        var index = System.Array.IndexOf(headers, name);
        if (index >= 0)
        {
          all.AutoSizeColumn(index);
        }
      }

      foreach (var pattern in patterns)
      {
        for (int index = 0; index < headers.Length; index++)
        {
          if (Regex.Match(headers[index], pattern).Success)
          {
            all.AutoSizeColumn(index);
          }
        }
      }
    }

    private static void SetSizeColumn(ISheet all, string[] headers, string name, int width, bool isRegex)
    {
      if (!isRegex)
      {
        var index = System.Array.IndexOf(headers, name);
        if (index >= 0)
        {
          all.SetColumnWidth(index, width * 256);
        }
      }
      else
      {
        for (int index = 0; index < headers.Length; index++)
        {
          if (Regex.Match(headers[index], name).Success)
          {
            all.SetColumnWidth(index, width * 256);
          }
        }
      }
    }
  }
}
