using Nest;

namespace DotNetCodeSearch.Elasticsearch
{
  /// <summary>
  /// Represent a fragment of interesting text from within a source file.
  /// </summary>
  public class SourceFileTokenFragment
  {
    /// <summary>
    /// Creates a new instance initialised with the specified field values.
    /// </summary>
    /// <param name="fragmentText">The fragment text.</param>
    /// <param name="lineNumber">Starting line number of the fragment.</param>
    public SourceFileTokenFragment(string fragmentText, int lineNumber)
    {
      FragmentText = fragmentText.Trim();
      LineNumber = lineNumber;
    }
    
    /// <summary>
    /// The text which makes up this fragment.
    /// </summary>
    /// <returns></returns>
    [ElasticProperty(Name = "fragment_text")]
    public string FragmentText
    {
      get;
      private set;
    }

    /// <summary>
    /// The starting line number of the fragment.
    /// </summary>
    /// <returns></returns>
    [ElasticProperty(Name = "line_number")]
    public int LineNumber
    {
      get;
      private set;
    }
  }
}
