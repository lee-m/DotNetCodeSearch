using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DotNetCodeSearch.ElasticSearch;

namespace DotNetCodeSearch.Mercurial
{
  public abstract class MercurialRepositoryIndexerBase<TContentType> where TContentType : class
  {
    private ElasticSearchClient<TContentType> mClient;

    public MercurialRepositoryIndexerBase(ElasticSearchClient<TContentType> client)
    {
      mClient = client;
    }

    abstract public void IndexRepository(string repoPath);

    protected ElasticSearchClient<TContentType> ElasticClient
    {
      get { return mClient; }
    }
  }
}
