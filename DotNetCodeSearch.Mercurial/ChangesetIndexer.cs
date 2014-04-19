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
  public class ChangesetIndexer : MercurialRepositoryIndexerBase<Changeset>
  {
    public ChangesetIndexer(ElasticsearchClient<Changeset> client) : base(client)
    {
    }

    public override void IndexRepository(string repoPath)
    {
      CommandClient hgClient = new CommandClient(repoPath, null, null, null);
      string repoName = new DirectoryInfo(repoPath).Name;

      IEnumerable<Changeset> changes = hgClient.Log(null).Select(rev =>
        new Changeset(repoName, rev.Branch, rev.RevisionId, rev.Message, rev.Author, rev.Date));
      ElasticClient.IndexContent(changes);
    }
  }
}
