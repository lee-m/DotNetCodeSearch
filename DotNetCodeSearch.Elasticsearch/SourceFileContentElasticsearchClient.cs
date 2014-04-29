using System;
using System.Collections.Generic;

using Nest;

namespace DotNetCodeSearch.Elasticsearch
{
  /// <summary>
  /// Wrapper around ElasticsearchClient to handle dealing with the file contents index.
  /// </summary>
  public class SourceFileContentElasticsearchClient : ElasticsearchClient<SourceFileContent>
  {
    /// <summary>
    /// Name of the index.
    /// </summary>
    private const string IndexName = "file_contents";

    /// <summary>
    /// Creates a new client pointing to the specified server.
    /// </summary>
    /// <param name="uri">Address of the Elasticsearch server.</param>
    public SourceFileContentElasticsearchClient(string uri)
      : base(uri, IndexName)
    {
    }

    /// <summary>
    /// Creates the index and defines any required mappings.
    /// </summary>
    public override void CreateIndex()
    {
      //Stop word filter to remove VB keywords
      StopTokenFilter vbKeywordsFilter = new StopTokenFilter();
      vbKeywordsFilter.Stopwords = new string[] { "addhandler", "addressof","alias","and","andalso","as","boolean","byref",
                                                  "byte","byval","call","case","catch","cbool","cbyte","cchar","cdate","cdbl",
                                                  "cdec","char", "cint","class","clng","cobj","const","continue", "csbyte",
                                                  "cshort","csng","cstr","ctype","cuint","culng","cushort","date", "decimal",
                                                  "declare","default","delegate", "dim", "directcast","do", "double", "each",
                                                  "else", "elseif", "end", "endif","enum","erase","error", "event","exit","false",
                                                  "finally","for","friend","function","get","gettype","getxmlnamespace",
                                                  "global","gosub","goto","handles","if","implements","imports","in","inherits",
                                                  "integer","interface","is","isnot","let","lib","like","long","loop","me","mod",
                                                  "module","mustinherit","mustoverride","mybase","myclass","namespace",
                                                  "narrowing","new","next","not","nothing","notinheritable",
                                                  "notoverridable","object","of","on","operator","option","optional",
                                                  "or","orelse","out","overloads","overridable","overrides",
                                                  "paramarray","partial","private","property","protected","public",
                                                  "raiseevent","readonly","redim","rem","removehandler","resume",
                                                  "return","sbyte","select","set","shadows","shared","short","single",
                                                  "static","step","stop","string","structure","sub","synclock","then",
                                                  "throw","to","true","try","trycast","typeof","uinteger","ulong","ushort",
                                                  "using","variant","wend","when","while","widening","with","withevents",
                                                  "writeonly","xor" };

      //Analyser based on the built-in standard analyzer but which also removes VB stop words
      CustomAnalyzer fragmentsAnalyer = new CustomAnalyzer();
      fragmentsAnalyer.Filter = new List<string>() { "standard", "lowercase", "stop", "vb_kw_stop" };
      fragmentsAnalyer.Tokenizer = "standard";
      
      Client.DeleteIndex(i => i.Index(IndexName));
      Client.CreateIndex(IndexName, indx => indx
        .Analysis(analysis => analysis
          .Analyzers(analyser => analyser
            .Add("fragments_analyser", fragmentsAnalyer))
          .TokenFilters(tf => tf
            .Add("vb_kw_stop", vbKeywordsFilter)))
        .AddMapping<SourceFileContent>(mapping => mapping
          .Type("file_content")
          .Index(IndexName)
          .Enabled(true)
          .Properties(props => props
            .String(s => s
              .Name("file_name"))
            .String(s => s
              .Name("branch")
              .Index(FieldIndexOption.not_analyzed))
            .Completion(c => c
              .Name("branch_suggest"))
            .String(s => s
              .Name("repository")
              .Index(FieldIndexOption.not_analyzed))
            .Completion(c => c
              .Name("repository_suggest"))
            .Boolean(b => b
              .Name("designer_generated")
              .Index(NonStringIndexOption.not_analyzed))
            .String(s => s
              .Name("fragments")
              .IndexAnalyzer("fragments_analyser")
              .SearchAnalyzer("fragments_analyser")
              .TermVector(TermVectorOption.with_positions_offsets)))));
    }
  }
}
