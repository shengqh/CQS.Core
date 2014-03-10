using System.Text.RegularExpressions;

namespace CQS.Genome.SomaticMutation
{
  public static class SomaticMutationUtils
  {
    public static readonly Regex MutectPattern = new Regex(@"[^:]+:(\d+),(\d+)");

    public static readonly Regex Varscan2Pattern = new Regex(@"^.+?:.+?:\d+:(\d+):(\d+)");
  }
}