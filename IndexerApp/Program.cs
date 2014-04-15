using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using DotNetCodeSearch.ElasticSearch;
using DotNetCodeSearch.Mercurial;

namespace DotNetCodeSearch
{
  class Program
  {
    static void Main(string[] args)
    {
      ChangesetIndexer csi = new ChangesetIndexer();
      csi.IndexRepository(@"C:\Source\Provantis\ClinPath", new ChangesetElasticSearchClient("http://localhost:9200"));
    }
  }
}
