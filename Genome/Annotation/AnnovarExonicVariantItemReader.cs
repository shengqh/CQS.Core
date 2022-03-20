using System;
using System.Collections.Generic;

namespace CQS.Genome.Annotation
{
  public class AnnovarExonicVariantItemReader : AbstractTableFile<AnnovarVariantItem>
  {
    public AnnovarExonicVariantItemReader() : base() { }

    public AnnovarExonicVariantItemReader(string filename) : base(filename) { }

    protected override Dictionary<int, Action<string, AnnovarVariantItem>> GetIndexActionMap()
    {
      Dictionary<int, Action<string, AnnovarVariantItem>> result = new Dictionary<int, Action<string, AnnovarVariantItem>>();

      result[1] = AnnovarVariantItemExtension.VariantTypeFunc;
      result[2] = AnnovarVariantItemExtension.VariantAnnotationFunc;
      result[3] = AnnovarVariantItemExtension.ChromFunc;
      result[4] = AnnovarVariantItemExtension.ChromStartFunc;
      result[5] = AnnovarVariantItemExtension.ChromEndFunc;

      return result;
    }
  }
}
