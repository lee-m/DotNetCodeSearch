using DotNetCodeSearch.Elasticsearch;

namespace DotNetCodeSearch.Mercurial
{
  /// <summary>
  /// Base type for concrete classes which handle indexing a particular type of content.
  /// </summary>
  /// <typeparam name="TContentType">The content type this class will index.</typeparam>
  public abstract class MercurialRepositoryIndexerBase<TContentType> where TContentType : class
  {
    /// <summary>
    /// Elasticsearch client instance.
    /// </summary>
    private ElasticsearchClient<TContentType> mClient;

    /// <summary>
    /// Initialises this instance with the specified client.
    /// </summary>
    /// <param name="client">Elasticsearch client instance.</param>
    public MercurialRepositoryIndexerBase(ElasticsearchClient<TContentType> client)
    {
      mClient = client;
    }

    /// <summary>
    /// Indexes the contents of the specified repository.
    /// </summary>
    /// <param name="repoPath">Path of a Mercurial repository to index.</param>
    abstract public void IndexRepository(string repoPath);

    /// <summary>
    /// Accessor for the Elasticsearch client instance.
    /// </summary>
    /// <returns></returns>
    protected ElasticsearchClient<TContentType> ElasticClient
    {
      get { return mClient; }
    }
  }
}
