﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Nest;

namespace DotNetCodeSearch.Elasticsearch
{
  /// <summary>
  /// Base class for handling and creation of a search index.
  /// </summary>
  /// <typeparam name="TContent"></typeparam>
  public abstract class ElasticsearchClient<TContent> where TContent : class
  {
    /// <summary>
    /// Client to handle the interaction with the Elasticsearch server.
    /// </summary>
    private ElasticClient mClient;

#region Public Methods

    /// <summary>
    /// Initialises the object to point to the specified server with the specified default index.
    /// </summary>
    /// <param name="uri">Location of the Elasticsearch server</param>
    /// <param name="indexName">Name of the default index to use for this instance.</param>
    public ElasticsearchClient(string uri, string indexName)
    {
      if (string.IsNullOrEmpty(uri))
        throw new ArgumentNullException("uri");

      var settings = new ConnectionSettings(new Uri(uri));
      settings.SetDefaultIndex(indexName);

      mClient = new ElasticClient(settings);
    }

    /// <summary>
    /// Creates the index and any mappings required.
    /// </summary>
    public abstract void CreateIndex();

    /// <summary>
    /// Adds content to the index.
    /// </summary>
    /// <param name="content">The content to index.</param>
    public void IndexContent(IEnumerable<TContent> content)
    {
      mClient.IndexMany(content);
    }

    /// <summary>
    /// Indexes a single document.
    /// </summary>
    /// <param name="doc">The document to index.</param>
    public void IndexSingleDocument(TContent doc)
    {
      mClient.Index(doc);
    }
    #endregion

    #region Properties

    /// <summary>
    /// Accessor for the client instance.
    /// </summary>
    /// <returns></returns>
    protected ElasticClient Client
    {
      get { return mClient; }
    }

#endregion
  }
}
