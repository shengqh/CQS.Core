using CQS.Genome.Gtf;
using RCPA;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CQS.Genome.Annotation
{
  public class ParalyzerClusterAnnotator : AbstractThreadProcessor
  {
    private ParalyzerClusterAnnotatorOptions _options;

    public ParalyzerClusterAnnotator(ParalyzerClusterAnnotatorOptions options)
    {
      this._options = options;
    }

    public override IEnumerable<string> Process()
    {
      List<GtfItem> items = new List<GtfItem>();
      foreach (var corfile in _options.CoordinateFiles)
      {
        var curitems = SequenceRegionUtils.GetSequenceRegions(corfile);
        if (_options.Features != null && _options.Features.Count > 0)
        {
          items.RemoveAll(m => !_options.Features.Contains(m.Feature));
        }
        if (!string.IsNullOrEmpty(_options.NameKey))
        {
          var key = _options.NameKey + "=";
          items.ForEach(m =>
          {
            if (!string.IsNullOrEmpty(m.Attributes) && m.Attributes.Contains(key))
            {
              m.Name = m.Attributes.StringAfter(key).StringBefore(";").Trim();
            }
          });
        }
        items.AddRange(curitems);
      }

      var removechr = !items.All(m => m.Seqname.StartsWith("chr"));
      if (removechr)
      {
        items.ForEach(m => m.Seqname = m.Seqname.StringAfter("chr"));
      }

      var map = items.GroupBy(m => m.Seqname).ToDictionary(m => m.Key, m => m.ToList());

      using (StreamWriter sw = new StreamWriter(_options.OutputFile))
      {
        using (var sr = new StreamReader(_options.InputFile))
        {
          string line;
          var headers = sr.ReadLine().Split(',').ToList();
          headers.Add("ClosetFeature");
          headers.Add("ClosetFeatureLocus");
          headers.Add("ClosetFeatureDistance");

          sw.WriteLine(headers.Merge(','));

          List<GtfItem> mingtfs = new List<GtfItem>();
          while ((line = sr.ReadLine()) != null)
          {
            var parts = line.Split(',');
            var chr = parts[0];
            if (removechr)
            {
              chr = chr.StringAfter("chr");
            }
            var start = long.Parse(parts[2]);
            var end = long.Parse(parts[3]);
            var sequence = parts[5];
            var location = int.Parse(parts[7]);
            var t2c_feature = string.Empty;

            List<GtfItem> gtfs;
            if (!map.TryGetValue(chr, out gtfs))
            {
              sw.WriteLine("{0},,,", line);
              continue;
            }

            long mindist = int.MaxValue;
            mingtfs.Clear();

            foreach (var gtf in gtfs)
            {
              long dist;
              if (gtf.Start > end)
              {
                dist = gtf.Start - end;
              }
              else if (gtf.End < start)
              {
                dist = start - gtf.End;
              }
              else
              {
                dist = 0;
              }

              if (dist < mindist)
              {
                mingtfs.Clear();
                mingtfs.Add(gtf);
                mindist = dist;
              }
              else if (dist == mindist)
              {
                mingtfs.Add(gtf);
              }
            }


            sw.WriteLine("{0},{1},{2},{3}",
              line,
              (from m in mingtfs select m.Name).Merge(";"),
              (from m in mingtfs select m.GetLocation()).Merge(";"),
              mindist);
          }
        }
      }

      return new string[] { _options.OutputFile };
    }
  }
}

