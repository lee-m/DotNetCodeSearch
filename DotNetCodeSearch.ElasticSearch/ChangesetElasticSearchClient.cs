using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DotNetCodeSearch.Core;

namespace DotNetCodeSearch.ElasticSearch
{
  public class ChangesetElasticSearchClient : ElasticSearchClient<Changeset>
  {
    private const string INDEX_NAME = "changesets";

    public ChangesetElasticSearchClient(string uri)
      : base(uri, INDEX_NAME)
    {

    }

    public override void CreateIndex()
    {
      throw new NotImplementedException();
    }
  }
}
