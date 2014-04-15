using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

using DotNetCodeSearch.Core;
using DotNetCodeSearch.ElasticSearch;
using Mercurial;

namespace DotNetCodeSearch.Mercurial
{
  /// <summary>
  /// 
  /// </summary>
  public class ChangesetIndexer : IMercurialRepositoryIndexer<Changeset>
  {
    public void IndexRepository(string repoPath, ElasticSearchClient<Changeset> indexer)
    {
      CommandClient hgClient = new CommandClient(repoPath, null, null, null);
      string repoName = new DirectoryInfo(repoPath).Name;

      IEnumerable<Changeset> changes = hgClient.Log(null).Select(rev =>
        new Changeset(repoName, rev.Branch, rev.RevisionId, rev.Message, rev.Author, rev.Date));
      indexer.IndexContent(changes);
    }
  }
}
