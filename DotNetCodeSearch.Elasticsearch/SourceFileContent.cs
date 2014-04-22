using Nest;

namespace DotNetCodeSearch.Elasticsearch
{
  /// <summary>
  /// Represents a source file document in the search index.
  /// </summary>
  public class SourceFileContent
  {
    /// <summary>
    /// Initialises a new source file instance.
    /// </summary>
    /// <param name="fileName">Name of the file.</param>
    /// <param name="branch">Branch the file is in.</param>
    /// <param name="repo">Repository containing the file.</param>
    /// <param name="contents">Contents of the file.</param>
    public SourceFileContent(string fileName, string branch, string repo, string contents)
    {
      FileName = fileName;
      Branch = branch;
      Repository = repo;
      FileContents = contents;
    }

    /// <summary>
    /// Name of the file.
    /// </summary>
    /// <returns></returns>
    [ElasticProperty(Name = "file_name")]
    public string FileName
    {
      get;
      private set;
    }

    /// <summary>
    /// Branch this file exists on.
    /// </summary>
    /// <returns></returns>
    [ElasticProperty(Name = "branch")]
    public string Branch
    {
      get;
      private set;
    }

    /// <summary>
    /// Repository this file is in.
    /// </summary>
    /// <returns></returns>
    [ElasticProperty(Name = "repository")]
    public string Repository
    {
      get;
      private set;
    }

    /// <summary>
    /// Contents of the file.
    /// </summary>
    /// <returns></returns>
    [ElasticProperty(Name = "contents")]
    public string FileContents
    {
      get;
      private set;
    }
  }
}