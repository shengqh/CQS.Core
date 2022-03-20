namespace CQS.Genome.Annotation
{
  public interface IAnnotationTsvExporter
  {
    string GetHeader();

    string GetValue(string chrom, long start, long end);
  }
}
