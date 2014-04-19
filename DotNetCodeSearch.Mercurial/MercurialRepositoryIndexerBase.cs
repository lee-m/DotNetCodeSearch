using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DotNetCodeSearch.Elasticsearch;

namespace DotNetCodeSearch.Mercurial
{
  public abstract class MercurialRepositoryIndexerBase<TContentType> where TContentType : class
  {
    private ElasticsearchClient<TContentType> mClient;

    public MercurialRepositoryIndexerBase(ElasticsearchClient<TContentType> client)
    {
      mClient = client;
    }

    abstract public void IndexRepository(string repoPath);

    protected ElasticsearchClient<TContentType> ElasticClient
    {
      get { return mClient; }
    }
  }
}
