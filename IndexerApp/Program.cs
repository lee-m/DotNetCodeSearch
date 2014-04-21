using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using DotNetCodeSearch.Elasticsearch;
using DotNetCodeSearch.Mercurial;

namespace DotNetCodeSearch
{
  class Program
  {
    private class IndexerApplication
    {
      /// <summary>
      /// Stores the parsed command line options.
      /// </summary>
      private class CommandLineOptions
      {
        /// <summary>
        /// Whether to recreate the search indices.
        /// </summary>
        /// <returns></returns>
        public bool CreateIndices { get; set; }

        /// <summary>
        /// List of repositories to index.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> RepositoresToIndex { get; set; }

        /// <summary>
        /// Address of the Elasticsearch server.
        /// </summary>
        /// <returns></returns>
        public string ServerAddress { get; set; }
      }

      /// <summary>
      /// Pares the command line options and runs the application.
      /// </summary>
      /// <param name="args">Command line options.</param>
      public void Run(IEnumerable<string> args)
      {
        if(!args.Any())
        {
          PrintUsage();
          return;
        }

        //If there's an error parsing the command line options bail out.
        CommandLineOptions opts = ParseCommandLineOptions(args);

        if (opts == null)
          return;

        ///Server address is mandatory
        if(string.IsNullOrEmpty(opts.ServerAddress))
        {
          Console.WriteLine("No Elasticsearch server address specified.");
          return;
        }

        ChangesetElasticsearchClient csc = new ChangesetElasticsearchClient(opts.ServerAddress);
        SourceFileContentElasticsearchClient sfc = new SourceFileContentElasticsearchClient(opts.ServerAddress);

        if (opts.CreateIndices)
        {
          csc.CreateIndex();
          sfc.CreateIndex();
        }

        if(opts.RepositoresToIndex != null)
        {
          ChangesetIndexer csi = new ChangesetIndexer(csc);
          SourceFileContentIndexer sfi = new SourceFileContentIndexer(sfc);

          foreach(string repo in opts.RepositoresToIndex)
          {
            csi.IndexRepository(repo);
            sfi.IndexRepository(repo);
          }
        }
      }

      /// <summary>
      /// Parses the command line options.
      /// </summary>
      /// <param name="args">Options to parse</param>
      /// <returns>The parsed options or <code>null</code> in the event of an error.</returns>
      private CommandLineOptions ParseCommandLineOptions(IEnumerable<string> args)
      {
        CommandLineOptions opts = new CommandLineOptions();
        Queue<string> queuedOpts = new Queue<string>(args);

        while(queuedOpts.Any())
        {
          string opt = queuedOpts.Dequeue();

          if (opt.StartsWith("-"))
          {
            string optName = opt.Substring(1);

            if (optName == "create")
              opts.CreateIndices = true;
            else if (optName == "index")
            {
              if (!queuedOpts.Any())
              {
                Console.WriteLine("Missing option value for 'index' option.");
                return null;
              }
              else
                opts.RepositoresToIndex = queuedOpts.Dequeue().Split(',');
            }
            else if (optName == "server")
            {
              if (!queuedOpts.Any())
              {
                Console.WriteLine("Missing option value for 'server' option.");
                return null;
              }
              else
                opts.ServerAddress = queuedOpts.Dequeue();
            }
            else
            {
              Console.WriteLine("Ignoring unrecognised option '{0}'", opt);
            }
          }
        }

        return opts;
      }

      /// <summary>
      /// Prints the supported command line options to the console.
      /// </summary>
      private void PrintUsage()
      {
        Console.WriteLine("Usage: Indexer <options> ");
        Console.WriteLine("  -create               Creates the required Elasticsearch index and mappings.");
        Console.WriteLine("  -index repo1,repo2    Indexes the repositories specified.");
        Console.WriteLine("  -server <server url>  Specifies the Elasticsearch server address.");
      }
    }

    static void Main(string[] args)
    {
      IndexerApplication app = new IndexerApplication();
      app.Run(args);
    }
  }
}
