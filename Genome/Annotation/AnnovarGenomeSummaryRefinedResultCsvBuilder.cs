using CQS.Genome.Affymetrix;
using CQS.Genome.Statistics;
using LumenWorks.Framework.IO.Csv;
using NPOI.HSSF.UserModel;
using NPOI.HSSF.Util;
using NPOI.SS.UserModel;
using RCPA;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace CQS.Genome.Annotation
{

  public class AnnovarGenomeSummaryRefinedResultCsvBuilder : AbstractThreadFileProcessor
  {
    private string targetFile;
    private string affyAnnotationFile;
    private Regex mutectRegex = new Regex(@"[^:]+:(\d+),(\d+):[^:]+:\d+:[^:]+:[^:]+");

    public AnnovarGenomeSummaryRefinedResultCsvBuilder(string affyAnnotationFile, string targetFile)
    {
      this.affyAnnotationFile = affyAnnotationFile;
      this.targetFile = targetFile;
    }

    public override IEnumerable<string> Process(string fileName)
    {
      //Write the stream data of workbook to the root directory
      using (FileStream file = new FileStream(this.targetFile, FileMode.Create))
      {
        var book = new HSSFWorkbook();

        //cell style for hyperlinks
        //by default hyperlinks are blue and underlined
        var hlink_style = book.CreateCellStyle();
        IFont hlink_font = book.CreateFont();
        hlink_font.Underline = (byte)FontUnderlineType.SINGLE;
        hlink_font.Color = HSSFColor.BLUE.index;
        hlink_style.SetFont(hlink_font);
        hlink_style.WrapText = true;

        var wrap_style = book.CreateCellStyle();
        wrap_style.WrapText = true;

        var numeric_style = book.CreateCellStyle();
        numeric_style.DataFormat = 0xb;

        Dictionary<string, string> genenames = File.Exists(this.affyAnnotationFile) ? AnnotationFile.GetGeneSymbolDescriptionMap(this.affyAnnotationFile) : new Dictionary<string, string>();
        ISheet all = book.CreateSheet("all");

        AnnovarGenomeSummaryItem item = new AnnovarGenomeSummaryItem();

        var sr = new StreamReader(fileName);
        var headers = sr.ReadLine().Split(',').ToList();

        var geneIndex = headers.FindIndex(m => m.Equals("Gene") || m.Equals("Gene.refGene"));
        var funcIndex = FindIndex(geneIndex, headers.FindIndex(m => m.Equals("Func") || m.Equals("Func.refGene")));
        var exonicIndex = FindIndex(geneIndex, headers.FindIndex(m => m.Equals("ExonicFunc") || m.Equals("ExonicFunc.refGene")));
        var dbsnpIndex = FindIndex(geneIndex, headers.FindIndex(m => m.ToLower().StartsWith("dbsnp") || m.ToLower().StartsWith("snp")));
        var chrIndex = headers.IndexOf("Chr");
        var startIndex = headers.IndexOf("Start");
        var endIndex = headers.IndexOf("End");

        var otherInfoIndex = headers.IndexOf("Otherinfo");

        //handle the headers. The length of headers may less than the data.
        var firstrow = all.CreateRow(0);
        for (int i = 0; i <= geneIndex; i++)
        {
          firstrow.CreateCell(i).SetCellValue(headers[i]);
        }
        firstrow.CreateCell(geneIndex + 1).SetCellValue("Description");
        for (int i = geneIndex + 1; i < otherInfoIndex; i++)
        {
          firstrow.CreateCell(i + 1).SetCellValue(headers[i]);
        }
        firstrow.CreateCell(otherInfoIndex + 1).SetCellValue("Location");

        bool? isMuTect = null;
        bool isTableVersion = false; //using table_annovar.pl or summarize_annovar.pl
        bool hasLOD = false;
        double lod = 0.0;
        //handle data
        using (CsvReader csv = new CsvReader(sr, false))
        {
          int nRow = 0;
          while (csv.ReadNextRecord())
          {
            if (!isMuTect.HasValue)
            {
              isTableVersion = csv.FieldCount == headers.Count;
              isMuTect = mutectRegex.Match(csv[csv.FieldCount - 2]).Success;
              hasLOD = double.TryParse(csv[csv.FieldCount - 1], out lod);
              if (isMuTect.Value)
              {
                firstrow.CreateCell(otherInfoIndex + 2).SetCellValue("Normal");
                firstrow.CreateCell(otherInfoIndex + 3).SetCellValue("Tumor");
                firstrow.CreateCell(otherInfoIndex + 4).SetCellValue("FisherExactTest");
                all.SetDefaultColumnStyle(otherInfoIndex + 4, numeric_style);
                if (hasLOD)
                {
                  firstrow.CreateCell(otherInfoIndex + 5).SetCellValue("LOD_FStar");
                  all.SetDefaultColumnStyle(otherInfoIndex + 5, numeric_style);
                }
              }
              else
              {
                for (int i = otherInfoIndex; i < headers.Count; i++)
                {
                  firstrow.CreateCell(i + 2).SetCellValue(headers[i]);
                }
              }
            }

            nRow++;
            var row = all.CreateRow(nRow);
            for (int i = 0; i < geneIndex; i++)
            {
              row.CreateCell(i).SetCellValue(csv[i]);
            }

            //add link for gene symbol
            item.GeneString = csv[geneIndex];
            var cell = row.CreateCell(geneIndex);
            cell.Hyperlink = new HSSFHyperlink(HyperlinkType.URL)
            {
              Address = string.Format("http://www.genecards.org/cgi-bin/carddisp.pl?gene={0}", item.Genes[0].Name)
            };
            cell.CellStyle = hlink_style;
            cell.SetCellValue((from g in item.Genes select g.Name).Merge("\n"));

            //gene description
            var desCell = row.CreateCell(geneIndex + 1);
            desCell.CellStyle = wrap_style;
            desCell.SetCellValue((from gene in item.Genes
                                  let description = genenames.ContainsKey(gene.Name) ? genenames[gene.Name] : " "
                                  select description).Merge("\n"));

            //add location information
            for (int i = geneIndex + 1; i < otherInfoIndex; i++)
            {
              row.CreateCell(i + 1).SetCellValue(csv[i]);
            }
            var locationCell = row.CreateCell(otherInfoIndex + 1);
            locationCell.SetCellValue(string.Format("{0}:{1}-{2}", csv[chrIndex], csv[startIndex], csv[endIndex]));

            if (isMuTect.Value)
            {
              Match normal, tumor;
              if (isTableVersion)
              {
                var parts = csv[csv.FieldCount - 1].Split('\t');
                if (hasLOD)
                {
                  normal = mutectRegex.Match(parts[parts.Length - 3]);
                  tumor = mutectRegex.Match(parts[parts.Length - 2]);
                  lod = double.Parse(parts[parts.Length - 1]);
                }
                else
                {
                  normal = mutectRegex.Match(parts[parts.Length - 2]);
                  tumor = mutectRegex.Match(parts[parts.Length - 1]);
                }
              }
              else
              {
                if (hasLOD)
                {
                  tumor = mutectRegex.Match(csv[csv.FieldCount - 3]);
                  normal = mutectRegex.Match(csv[csv.FieldCount - 2]);
                  lod = double.Parse(csv[csv.FieldCount - 1]);
                }
                else
                {
                  tumor = mutectRegex.Match(csv[csv.FieldCount - 2]);
                  normal = mutectRegex.Match(csv[csv.FieldCount - 1]);
                }
              }
              FisherExactTestResult fetr = new FisherExactTestResult();
              fetr.Sample1.Succeed = int.Parse(normal.Groups[1].Value);
              fetr.Sample1.Failed = int.Parse(normal.Groups[2].Value);
              fetr.Sample2.Succeed = int.Parse(tumor.Groups[1].Value);
              fetr.Sample2.Failed = int.Parse(tumor.Groups[2].Value);

              row.CreateCell(otherInfoIndex + 2).SetCellValue(string.Format("{0}:{1}", fetr.Sample1.Succeed, fetr.Sample1.Failed));
              row.CreateCell(otherInfoIndex + 3).SetCellValue(string.Format("{0}:{1}", fetr.Sample2.Succeed, fetr.Sample2.Failed));
              row.CreateCell(otherInfoIndex + 4).SetCellValue(fetr.CalculateTwoTailPValue());
              if (hasLOD)
              {
                row.CreateCell(otherInfoIndex + 5).SetCellValue(lod);
              }
            }
            else
            {
              for (int i = otherInfoIndex; i < csv.FieldCount; i++)
              {
                row.CreateCell(i + 2).SetCellValue(csv[i]);
              }
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
                dbsnpcell.CellStyle = (hlink_style);
              }
            }
          }
        }

        all.SetColumnWidth(chrIndex, 5 * 256);
        all.SetColumnWidth(startIndex, 13 * 256);
        all.SetColumnWidth(endIndex, 13 * 256);
        all.SetColumnWidth(funcIndex, 15 * 256);
        all.SetColumnWidth(geneIndex, 13 * 256);
        all.SetColumnWidth(geneIndex + 1, 60 * 256);
        all.SetColumnWidth(exonicIndex, 20 * 256);
        all.SetColumnWidth(dbsnpIndex, 15 * 256);
        all.SetColumnWidth(otherInfoIndex + 1, 22 * 256);

        if (isMuTect.Value)
        {
          all.SetColumnWidth(otherInfoIndex + 2, 10 * 256);
          all.SetColumnWidth(otherInfoIndex + 3, 10 * 256);
          all.SetColumnWidth(otherInfoIndex + 4, 10 * 256);
          if (hasLOD)
          {
            all.SetColumnWidth(otherInfoIndex + 5, 10 * 256);
          }
        }

        book.Write(file);
      }

      return new string[] { targetFile };
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
