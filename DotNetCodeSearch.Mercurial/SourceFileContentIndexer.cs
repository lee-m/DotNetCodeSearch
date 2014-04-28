using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

using DotNetCodeSearch.Elasticsearch;

using Mercurial;

namespace DotNetCodeSearch.Mercurial
{
  /// <summary>
  /// Handles indexing the file contents of the files within each branch of a Mercurial repository.
  /// </summary>
  public class SourceFileContentIndexer : MercurialRepositoryIndexerBase<SourceFileContent>
  {
    /// <summary>
    /// Maximum number of files to index in a single operation.
    /// </summary>
    private const int IndexBatchSize = 50;

    /// <summary>
    /// Initialise this instance to use the provided Elasticsearch client.
    /// </summary>
    /// <param name="client">Client instance to use for communicating with the server.</param>
    public SourceFileContentIndexer(ElasticsearchClient<SourceFileContent> client) : base(client)
    {
    }

    /// <summary>
    /// Iterates over each branch and each file in that branch and indexes the file contents.
    /// </summary>
    /// <param name="repoPath">Directory of a Mercurial repository to index.</param>
    public override void IndexRepository(string repoPath)
    {
      Repository repo = new Repository(repoPath);
      string repoName = new DirectoryInfo(repoPath).Name;

      repo.Branches().AsParallel().ForAll(branch => IndexBranch(repo, repoName, branch));
    }

    /// <summary>
    /// Thread callback to index the contents of a particular branch on a background thread.
    /// </summary>
    /// <param name="repo">Handle to the repository being indexed.</param>
    /// <param name="repoName">Name of the repostory.</param>
    /// <param name="branch">Name of the branch being indexed.</param>
    private void IndexBranch(Repository repo, string repoName, BranchHead branch)
    {
      string branchArg = string.Format("-r {0}", branch.Name);
      ManifestCommand manifestCmd = new ManifestCommand();
      manifestCmd.AdditionalArguments.Add(branchArg);

      var fileContent = new List<SourceFileContent>();

      foreach (var branchFile in repo.Manifest(manifestCmd))
      {
        //Only interested in VB files
        if (!Path.GetExtension(branchFile).Equals(".vb", StringComparison.OrdinalIgnoreCase))
          continue;

        CatCommand catCmd = new CatCommand();
        catCmd.AdditionalArguments.Add(branchArg);
        catCmd.AdditionalArguments.Add(string.Format("\"{0}\"", branchFile));

        bool designerGenerated = branchFile.EndsWith(".Designer.vb", StringComparison.InvariantCultureIgnoreCase);
        IEnumerable<SourceFileTokenFragment> tokenFragments = GetSourceFileTokenFragments(repo.Cat(catCmd));

        fileContent.Add(new SourceFileContent(branchFile, branch.Name, repoName, tokenFragments, designerGenerated));

        //Attempting to index too many files in one go OOM's the server so use smaller batches
        if (fileContent.Count == IndexBatchSize)
        {
          ElasticClient.IndexContent(fileContent);
          fileContent.Clear();
        }
      }

      if (fileContent.Any())
        ElasticClient.IndexContent(fileContent);
    }

    private IEnumerable<SourceFileTokenFragment> GetSourceFileTokenFragments(string contents)
    {
      SourceFileFragmentGatherer fragmentGather = new SourceFileFragmentGatherer();
      IEnumerable<SourceFileTokenFragment> frags = fragmentGather.GetFragments(contents);

      return frags;
    }

    //[Conditional("DEBUG")]
    //private void DumpFragmentsToFile(IEnumerable<SourceFileTokenFragment> fragments)
    //{
    //  using (StreamWriter debugOut = new StreamWriter("FragmentsDump.txt", false))
    //  {
    //    foreach (var frag in fragments)
    //    {
    //      debugOut.WriteLine(frag.FragmentText);
    //      debugOut.WriteLine("-------------------------------------");
    //    }
    //  }
    //}
  }
}