﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Nest;

namespace DotNetCodeSearch.Elasticsearch
{
  public class ChangesetElasticsearchClient : ElasticsearchClient<Changeset>
  {
    private const string IndexName = "changesets";

    public ChangesetElasticsearchClient(string uri)
      : base(uri, IndexName)
    {

    }

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
          .IdField(id => id.SetPath("iD"))
          .Properties(props => props
            .String(s => s
              .Name("repository")
              .Index(FieldIndexOption.not_analyzed))
            .Completion(c => c
              .Name("repository_suggest"))
            .String(s => s
              .Name("branch")
              .Index(FieldIndexOption.not_analyzed))
            .Completion(c => c
              .Name("branch_suggest"))
            .String(s => s
              .Name("id")
              .Index(FieldIndexOption.not_analyzed))
            .MultiField(m => m
              .Name("message")
              .Fields(f => f
                .String(s => s
                  .Name("message")
                  .IndexAnalyzer("english")
                  .SearchAnalyzer("english")
                  .TermVector(TermVectorOption.with_positions_offsets)
                  .Store(true))
                .String(s => s
                  .Name("plain")
                  .IndexAnalyzer("standard")
                  .SearchAnalyzer("standard")
                  .TermVector(TermVectorOption.with_positions_offsets)
                  .Store(true))))
            .String(s => s
              .Name("author"))
            .Completion(c => c
              .Name("author_suggest"))
            .Date(d => d
              .Name("changeDateTime")
              .Index(NonStringIndexOption.not_analyzed)))));
    }
  }
}