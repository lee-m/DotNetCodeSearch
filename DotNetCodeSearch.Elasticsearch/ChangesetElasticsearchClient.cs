using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Nest;

namespace DotNetCodeSearch.Elasticsearch
{
  /// <summary>
  /// Wrapper around ElasticsearchClient to handle dealing with the changesets index.
  /// </summary>
  public class ChangesetElasticsearchClient : ElasticsearchClient<Changeset>
  {
    /// <summary>
    /// Name of the index.
    /// </summary>
    private const string IndexName = "changesets";

    /// <summary>
    /// Creates a new client pointing to the specified server.
    /// </summary>
    /// <param name="uri">Address of the Elasticsearch server.</param>
    public ChangesetElasticsearchClient(string uri)
      : base(uri, IndexName)
    {

    }

    /// <summary>
    /// Creates the index and defines any required mappings.
    /// </summary>
    public override void CreateIndex()
    {
      Client.DeleteIndex(i => i.Index(IndexName));
      Client.CreateIndex(IndexName, indx => indx
        .AddMapping<Changeset>(mapping => mapping
          .Type("changeset")
          .Index(IndexName)
          .DateDetection(true)
          .NumericDetection(true)
          .Enabled(true)
          .IdField(id => id.Path("iD"))
          .Properties(props => props
            .String(s => s
              .Name("repository")
              .Index(FieldIndexOption.NotAnalyzed))
            .Completion(c => c
              .Name("repository_suggest"))
            .String(s => s
              .Name("branch")
              .Index(FieldIndexOption.NotAnalyzed))
            .Completion(c => c
              .Name("branch_suggest"))
            .String(s => s
              .Name("id")
              .Index(FieldIndexOption.NotAnalyzed))
            .MultiField(m => m
              .Name("message")
              .Fields(f => f
                .String(s => s
                  .Name("message")
                  .IndexAnalyzer("english")
                  .SearchAnalyzer("english")
                  .TermVector(TermVectorOption.WithPositionsOffsets)
                  .Store(true))
                .String(s => s
                  .Name("plain")
                  .IndexAnalyzer("standard")
                  .SearchAnalyzer("standard")
                  .TermVector(TermVectorOption.WithPositionsOffsets)
                  .Store(true))))
            .String(s => s
              .Name("author")
              .Index(FieldIndexOption.NotAnalyzed))
            .Completion(c => c
              .Name("author_suggest"))
            .Date(d => d
              .Name("changeDateTime")
              .Index(NonStringIndexOption.NotAnalyzed)))));
    }
  }
}
