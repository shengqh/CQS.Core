using System;
using System.Collections.Generic;

namespace CQS.Genome.Annotation
{
  public class AnnovarVariantItemReader : AbstractTableFile<AnnovarVariantItem>
  {
    public AnnovarVariantItemReader() : base() { }

    public AnnovarVariantItemReader(string filename) : base(filename) { }

    protected override Dictionary<int, Action<string, AnnovarVariantItem>> GetIndexActionMap()
    {
      Dictionary<int, Action<string, AnnovarVariantItem>> result = new Dictionary<int, Action<string, AnnovarVariantItem>>();

      result[0] = AnnovarVariantItemExtension.VariantTypeFunc;
      result[1] = AnnovarVariantItemExtension.VariantAnnotationFunc;
      result[2] = AnnovarVariantItemExtension.ChromFunc;
      result[3] = AnnovarVariantItemExtension.ChromStartFunc;
      result[4] = AnnovarVariantItemExtension.ChromEndFunc;

      return result;
    }
  }
}
