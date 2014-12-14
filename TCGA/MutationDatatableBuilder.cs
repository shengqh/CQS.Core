using CQS.Genome;
using RCPA;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CQS.TCGA
{
  public class MutationDatatableBuilder : AbstractThreadProcessor, ITCGADatatableBuilder
  {
    private TCGADatatableBuilderOptions _options;

    public MutationDatatableBuilder(TCGADatatableBuilderOptions options)
    {
      this._options = options;
    }

    class MutationItem
    {
      public string Tumor { get; set; }
      public string Platform { get; set; }
      public string Name { get; set; }
      public string NcbiBuild { get; set; }
      public string Chromosome { get; set; }
      public string Start { get; set; }
      public string End { get; set; }
      public string Strand { get; set; }
      public string VariantClassification { get; set; }
      public string VariantType { get; set; }
      public string TumorBarcode { get; set; }
      public string Locus { get; private set; }
      public void InitLocus()
      {
        this.Locus = string.Format("{0}:{1}-{2}", Chromosome, Start, End);
      }
      public string Paticipant { get; private set; }
      public void InitPaticipant()
      {
        this.Paticipant = BarInfo.GetPaticipant(this.TumorBarcode);
      }
    }

    public override IEnumerable<string> Process()
    {
      if (!_options.PrepareOptions())
      {
        throw new Exception(_options.ParsingErrors.Merge("\n"));
      }

      HashSet<int> sampleCodes = new HashSet<int>(_options.GetTCGASampleCodes().ToList().ConvertAll(m => m.Code));
      Func<string, bool> acceptBarcode = m => sampleCodes.Contains(new BarInfo(m, null).Sample);
      var tec = _options.GetTechnology();

      var items = new List<MutationItem>();
      foreach (var tumor in _options.TumorTypes)
      {
        var dir = Path.Combine(_options.TCGADirectory, tumor);
        if (!Directory.Exists(dir))
        {
          continue;
        }

        var tecdir = tec.GetTechnologyDirectory(dir);

        if (!Directory.Exists(tecdir))
        {
          continue;
        }

        foreach (var platform in _options.Platforms)
        {
          var platdir = Path.Combine(tecdir, platform);

          var datadirs = Directory.GetDirectories(platdir, "*Level_2*");
          foreach (var datadir in datadirs)
          {
            var maffiles = Directory.GetFiles(datadir, "*.somatic.maf");
            if (maffiles.Length == 0)
            {
              continue;
            }

            foreach (var maffile in maffiles)
            {
              using (var sr = new StreamReader(maffile))
              {
                string line;

                //skip comments
                while ((line = sr.ReadLine()) != null && line.StartsWith("#")) { }

                if (string.IsNullOrEmpty(line))
                {
                  continue;
                }

                //read header
                var headers = line.Split('\t');
                var nameIndex = Array.IndexOf(headers, "Hugo_Symbol");
                var ncbiIndex = Array.IndexOf(headers, "NCBI_Build");
                var chromosomeIndex = Array.IndexOf(headers, "Chromosome");
                var startIndex = Array.IndexOf(headers, "Start_position");
                var endIndex = Array.IndexOf(headers, "End_position");
                var strandIndex = Array.IndexOf(headers, "Strand");
                var variantClassificationIndex = Array.IndexOf(headers, "Variant_Classification");
                var variantTypeIndex = Array.IndexOf(headers, "Variant_Type");
                var barcodeIndex = Array.IndexOf(headers, "Tumor_Sample_Barcode");

                while ((line = sr.ReadLine()) != null)
                {
                  var parts = line.Split('\t');
                  var item = new MutationItem()
                  {
                    Tumor = tumor,
                    Platform = platform,
                    Name = parts[nameIndex],
                    NcbiBuild = parts[ncbiIndex],
                    Chromosome = parts[chromosomeIndex],
                    Start = parts[startIndex],
                    End = parts[endIndex],
                    Strand = parts[strandIndex],
                    VariantClassification = parts[variantClassificationIndex],
                    VariantType = parts[variantTypeIndex],
                    TumorBarcode = parts[barcodeIndex]
                  };
                  item.InitLocus();
                  item.InitPaticipant();

                  items.Add(item);
                }
              }
            }
          }
        }
      }

      using (var sw = new StreamWriter(_options.OutputFile))
      {
        var paticipants = (from item in items
                           select item.Paticipant).Distinct().OrderBy(m => m).ToList();
        var itemMap = items.ToDoubleDictionaryGroup(m => m.Locus, m => m.Paticipant);
        var locusList = itemMap.Keys.ToList();
        GenomeUtils.SortChromosome(locusList, m => m.StringBefore(":"), m => int.Parse(m.StringAfter(":").StringBefore("-")));

        sw.WriteLine("Hugo_Symbol\tNCBI_Build\tChromosome\tStart_position\tEnd_position\tStrand\tVariant_Classification\tVariant_Type\t{0}", paticipants.Merge("\t"));
        foreach (var locus in locusList)
        {
          var dic = itemMap[locus];
          var item = dic.Values.First().First();
          sw.Write("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}\t{7}",
            item.Name,
            item.NcbiBuild,
            item.Chromosome,
            item.Start,
            item.End,
            item.Start,
            item.VariantClassification,
            item.VariantType);

          foreach (var paticipant in paticipants)
          {
            if (dic.ContainsKey(paticipant))
            {
              sw.Write("\t1");
            }
            else
            {
              sw.Write("\t0");
            }
          }
          sw.WriteLine();
        }
      }

      var genefile = FileUtils.ChangeExtension(_options.OutputFile, ".gene.tsv");
      using (var sw = new StreamWriter(genefile))
      {
        var paticipants = (from item in items
                           select item.Paticipant).Distinct().OrderBy(m => m).ToList();
        var itemMap = items.ToDoubleDictionaryGroup(m => m.Name, m => m.Paticipant);
        var nameList = itemMap.Keys.OrderBy(m => m).ToList();

        sw.WriteLine("Hugo_Symbol\t{0}", paticipants.Merge("\t"));
        foreach (var name in nameList)
        {
          var dic = itemMap[name];
          var item = dic.Values.First().First();
          sw.Write("{0}", item.Name);

          foreach (var paticipant in paticipants)
          {
            if (dic.ContainsKey(paticipant))
            {
              sw.Write("\t1");
            }
            else
            {
              sw.Write("\t0");
            }
          }
          sw.WriteLine();
        }
      }

      return new[] { _options.OutputFile, genefile };
    }
  }
}
