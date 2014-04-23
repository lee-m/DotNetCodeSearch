using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

using DotNetCodeSearch.Elasticsearch;
using Mercurial;

namespace DotNetCodeSearch.Mercurial
{
  /// <summary>
  /// Handles the indexing of changesets within a Mercurial repository.
  /// </summary>
  public class ChangesetIndexer : MercurialRepositoryIndexerBase<DotNetCodeSearch.Elasticsearch.Changeset>
  {
    /// <summary>
    /// Initialise this instance to use the provided Elasticsearch client.
    /// </summary>
    /// <param name="client">Client instance to use for communicating with the server.</param>
    public ChangesetIndexer(ElasticsearchClient<DotNetCodeSearch.Elasticsearch.Changeset> client) : base(client)
    {
    }

    /// <summary>
    /// Indexes the changesets contained within the specified repository.
    /// </summary>
    /// <param name="repoPath">Directory path of the Mercurial repository to index.</param>
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
