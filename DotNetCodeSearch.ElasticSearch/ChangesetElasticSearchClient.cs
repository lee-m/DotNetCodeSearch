using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DotNetCodeSearch.Core;

using Nest;

namespace DotNetCodeSearch.ElasticSearch
{
  public class ChangesetElasticSearchClient : ElasticSearchClient<Changeset>
  {
    private const string IndexName = "changesets";

    public ChangesetElasticSearchClient(string uri)
      : base(uri, IndexName)
    {

    }

    public override void CreateIndex()
    {
      //Custom analyser for the commit message field - tokenises on whitespace
      //and then removes english stop words
      var messageFieldAnalyser = new CustomAnalyzer()
      {
        Tokenizer = "whitespace",
        Filter = new List<string> { "lowercase", "stop" }
      };
      
      Client.DeleteIndex(i => i.Index(IndexName));
      Client.CreateIndex(IndexName, indx => indx
        .Analysis(analysis => analysis
          .Analyzers(analyser => analyser
            .Add("message_analyser", messageFieldAnalyser)))
        .AddMapping<Changeset>(mapping => mapping
          .Type("changeset")
          .Index(IndexName)
          .DateDetection(true)
          .NumericDetection(true)
          .Enabled(true)
          .IdField(id => id.SetPath("iD"))
          .Properties(props => props
            .String(s => s
              .Name("repository")
              .Index(FieldIndexOption.not_analyzed))
            .String(s => s
              .Name("branch")
              .Index(FieldIndexOption.not_analyzed))
            .String(s => s
              .Name("iD")
              .Index(FieldIndexOption.not_analyzed))
            .String(s => s
              .Name("message")
              .IndexAnalyzer("message_analyser")
              .SearchAnalyzer("message_analyser"))
            .String(s => s
              .Name("author"))
            .Date(d => d
              .Name("changeDateTime")
              .Index(NonStringIndexOption.not_analyzed)))));
    }
  }
}
