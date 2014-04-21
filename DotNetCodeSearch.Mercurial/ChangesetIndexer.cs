using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

using DotNetCodeSearch.Elasticsearch;
using Mercurial;

namespace DotNetCodeSearch.Mercurial
{
  /// <summary>
  /// 
  /// </summary>
  public class ChangesetIndexer : MercurialRepositoryIndexerBase<DotNetCodeSearch.Elasticsearch.Changeset>
  {
    public ChangesetIndexer(ElasticsearchClient<DotNetCodeSearch.Elasticsearch.Changeset> client) : base(client)
    {
    }

    public override void IndexRepository(string repoPath)
    {
      Repository repo = new Repository(repoPath);
      string repoName = new DirectoryInfo(repoPath).Name;

      IEnumerable<DotNetCodeSearch.Elasticsearch.Changeset> changes = repo.Log().Select(rev =>
        new DotNetCodeSearch.Elasticsearch.Changeset(repoName, rev.Branch, rev.Hash, rev.CommitMessage, rev.AuthorName, rev.Timestamp));
      ElasticClient.IndexContent(changes);
    }
  }
}
