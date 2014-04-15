using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DotNetCodeSearch.ElasticSearch;

namespace DotNetCodeSearch.Mercurial
{
  public interface IMercurialRepositoryIndexer<TContentType> where TContentType : class
  {
    void IndexRepository(string repoPath, ElasticSearchClient<TContentType> indexer);
  }
}
