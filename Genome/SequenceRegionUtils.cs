using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using CQS.Genome.Bed;
using CQS.Genome.Gtf;

namespace CQS.Genome
{
  public static class SequenceRegionUtils
  {
    private static readonly Regex Reg = new Regex("(.+?):(.+?)-(.+?):([+-])");
    private static readonly Regex Namereg = new Regex(@"\((.+?)\)(.+?):(.+?)-(.+?):([+-])");

    public static bool Overlap(this ISequenceRegion one, ISequenceRegion two, double minPercentage)
    {
      if (!one.Seqname.Equals(two.Seqname))
      {
        return false;
      }

      ISequenceRegion first, second;
      if (one.Start < two.Start || (one.Start == two.Start && one.End > two.End))
      {
        first = one;
        second = two;
      }
      else
      {
        first = two;
        second = one;
      }

      //no-overlap
      if (first.End < second.Start)
      {
        return false;
      }

      //contain
      if (first.End >= second.End)
      {
        return true;
      }

      //overlap
      var overlap = first.End - second.Start + 1;
      return (overlap >= first.Length * minPercentage || overlap >= second.Length * minPercentage);
    }

    public static string GetLocationWithoutStrand(this ISequenceRegion sr)
    {
      return string.Format("{0}:{1}-{2}", sr.Seqname, sr.Start, sr.End);
    }

    public static string GetLocation(this ISequenceRegion sr)
    {
      return string.Format("{0}:{1}-{2}:{3}", sr.Seqname, sr.Start, sr.End, sr.Strand);
    }

    public static string GetNameLocation(this ISequenceRegion sr)
    {
      return string.Format("({0}){1}", sr.Name, sr.GetLocation());
    }

    public static T ParseLocation<T>(string line) where T : ISequenceRegion, new()
    {
      var vm = Reg.Match(line);
      if (!vm.Success)
      {
        throw new ArgumentException("Cannot recognize location format (should be like 13:48633608-48633629:-) : " + line);
      }

      var result = new T
      {
        Seqname = vm.Groups[1].Value,
        Start = int.Parse(vm.Groups[2].Value),
        End = int.Parse(vm.Groups[3].Value),
        Strand = vm.Groups[4].Value[0]
      };
      return result;
    }

    public static string GetNameLocations<T>(this IEnumerable<T> items) where T : ISequenceRegion
    {
      return (from qg in items select qg.GetNameLocation()).Merge(",");
    }

    public static string GetLocations<T>(this IEnumerable<T> items) where T : ISequenceRegion
    {
      return (from qg in items select qg.GetLocation()).Merge(",");
    }

    public static List<T> ParseNameLocations<T>(string line) where T : ISequenceRegion, new()
    {
      if (string.IsNullOrEmpty(line))
      {
        throw new ArgumentNullException("line");
      }

      var vm = Namereg.Match(line);
      if (!vm.Success)
      {
        throw new ArgumentException(
          "Cannot recognize location format (should be like (hsa-let-7a-1-5p)9:96938244-96938265:+) : " + line);
      }

      var result = new List<T>();
      while (vm.Success)
      {
        var item = new T();
        item.Name = vm.Groups[1].Value;
        item.Seqname = vm.Groups[2].Value;
        item.Start = int.Parse(vm.Groups[3].Value);
        item.End = int.Parse(vm.Groups[4].Value);
        item.Strand = vm.Groups[5].Value[0];
        result.Add(item);
        vm = vm.NextMatch();
      }

      return result;
    }

    public static List<GtfItem> GetSequenceRegions(string coordinateFile, string gtfFeature = "", bool bedAsGtf = false)
    {
      bool isBedFormat = IsBedFormat(coordinateFile);

      List<GtfItem> result;
      if (isBedFormat)
      {
        //bed is zero-based, and the end is not included in the sequence region
        //https://genome.ucsc.edu/FAQ/FAQformat.html#format1
        //gtf is 1-based, and the end is included in the sequence region
        //http://useast.ensembl.org/info/website/upload/gff.html
        //since pos in sam format is 1-based, we need to convert beditem to gtfitem.
        //http://samtools.sourceforge.net/SAMv1.pdf
        var bedItems = new BedItemFile<BedItem>().ReadFromFile(coordinateFile);
        if (!bedAsGtf)
        {
          bedItems.ForEach(m => m.Start++);
        }
        result = bedItems.ConvertAll(m => new GtfItem(m));
      }
      else
      {
        result = GtfItemFile.ReadFromFile(coordinateFile).ToList();
        if (!string.IsNullOrEmpty(gtfFeature))
        {
          result.RemoveAll(m => !m.Feature.Equals(gtfFeature));
        }

        result.ForEach(m =>
        {
          if (m.Attributes.Contains("Name="))
          {
            m.GeneId = m.Attributes.StringAfter("Name=").StringBefore(";");
          }
          else if (m.Attributes.Contains("gene_id \""))
          {
            m.GeneId = m.Attributes.StringAfter("gene_id \"").StringBefore("\"");
          }
          else
          {
            m.GeneId = m.Attributes;
          }
        });
      }

      return result;
    }

    public static bool IsBedFormat(string coordinateFile)
    {
      var isBedFormat = false;
      using (var sr = new StreamReader(coordinateFile))
      {
        string line;

        while ((line = sr.ReadLine()) != null)
        {
          if (line.StartsWith("#"))
          {
            continue;
          }

          var parts = line.Split('\t');
          long start, end;
          if (long.TryParse(parts[1], out start) && long.TryParse(parts[2], out end))
          {
            isBedFormat = true;
            break;
          }
        }
      }
      return isBedFormat;
    }
  }
}