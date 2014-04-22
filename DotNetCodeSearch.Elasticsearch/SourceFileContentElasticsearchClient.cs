using System;
using System.Collections.Generic;

using Nest;

namespace DotNetCodeSearch.Elasticsearch
{
  public class SourceFileContentElasticsearchClient : ElasticsearchClient<SourceFileContent>
  {
    private const string IndexName = "file_contents";

    public SourceFileContentElasticsearchClient(string uri)
      : base(uri, IndexName)
    {
    }

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

      //Analyser which tokenises on whitespace and removes all keywords and english stop words. This is 
      //indented to produce "phrase query" type tokens where multiple property/function calls appear 
      //together - i.e. it will tokenise SomeObj.SomeProperty.FunctionCall as a single token.
      CustomAnalyzer fileContentsStandardAnalyer = new CustomAnalyzer();
      fileContentsStandardAnalyer.Filter = new List<string>() { "standard", "lowercase", "stop", "vb_kw_stop" };
      fileContentsStandardAnalyer.Tokenizer = "standard";

      //Analyser which tokenises on non-letter characters and removes all keywords and english stop words. This 
      //is indented to decompose chained property access/function calls into one token per element - i.e. it will tokenise 
      //SomeObj.SomeProperty.FunctionCall into ["SomeObj", "SomeProperty", "FunctionCall"] tokens to allow each 
      //element to be searched for independent of where it appears within an expression.
      CustomAnalyzer fileContentsSimpleAnalyer = new CustomAnalyzer();
      fileContentsSimpleAnalyer.Filter = new List<string>() { "lowercase", "stop", "vb_kw_stop" };
      fileContentsSimpleAnalyer.Tokenizer = "lowercase";

      Client.DeleteIndex(i => i.Index(IndexName));
      Client.CreateIndex(IndexName, indx => indx
        .Analysis(analysis => analysis
          .Analyzers(analyser => analyser
            .Add("contents_standard", fileContentsStandardAnalyer))
          .Analyzers(analyser => analyser
            .Add("contents_simple", fileContentsSimpleAnalyer))
          .TokenFilters(tf => tf
            .Add("vb_kw_stop", vbKeywordsFilter)))
        .AddMapping<Changeset>(mapping => mapping
          .Type("file_content")
          .Index(IndexName)
          .Enabled(true)
          .Properties(props => props
            .String(s => s
              .Name("repository")
              .Index(FieldIndexOption.not_analyzed))
            .String(s => s
              .Name("branch")
              .Index(FieldIndexOption.not_analyzed))
            .String(s => s
              .Name("file_name"))
            .MultiField(mf => mf
              .Name("contents")
              .Fields(f => f
                .String(s => s  
                  .Name("contents_simple")
                  .IndexAnalyzer("contents_simple")
                  .SearchAnalyzer("contents_simple"))
                .String(s => s
                  .Name("contents_standard")
                  .IndexAnalyzer("contents_standard")
                  .SearchAnalyzer("contents_standard")))))));
    }
  }
}
