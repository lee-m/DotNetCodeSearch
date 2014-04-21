using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using Mercurial;
using DotNetCodeSearch.Elasticsearch;

namespace DotNetCodeSearch.Mercurial
{
  public class SourceFileContentIndexer : MercurialRepositoryIndexerBase<SourceFileContent>
  {
    public SourceFileContentIndexer(ElasticsearchClient<SourceFileContent> client) : base(client)
    {
    }

    public override void IndexRepository(string repoPath)
    {
      Repository repo = new Repository(repoPath);
      string repoName = new DirectoryInfo(repoPath).Name;

      foreach (var branch in repo.Branches())
      { 
        string branchArg = string.Format("-r {0}", branch.Name);
        ManifestCommand manifestCmd = new ManifestCommand();
        manifestCmd.AdditionalArguments.Add(branchArg);

        var fileContent = new List<SourceFileContent>();

        foreach(var branchFile in repo.Manifest(manifestCmd))
        {
          //Only interested in VB files
          if (!Path.GetExtension(branchFile).Equals(".vb", StringComparison.OrdinalIgnoreCase))
            continue;

          CatCommand catCmd = new CatCommand();
          catCmd.AdditionalArguments.Add(branchArg);
          catCmd.AdditionalArguments.Add(string.Format("\"{0}\"", branchFile));

          fileContent.Add(new SourceFileContent(branchFile, branch.Name, repoName, repo.Cat(catCmd)));
        }

        ElasticClient.IndexContent(fileContent);
      }
    }
  }
}
