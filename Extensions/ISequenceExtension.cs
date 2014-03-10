using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bio
{
  public static class ISequenceExtension
  {
    public static string GetSequenceString(this ISequence seq)
    {
      return new string(seq.Select(a => (char)a).ToArray());
    }
  }
}
