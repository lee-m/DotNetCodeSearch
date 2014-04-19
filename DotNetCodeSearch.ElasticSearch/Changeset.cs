using System;
using Nest;

namespace DotNetCodeSearch.ElasticSearch
{
  /// <summary>
  /// Represents details of a single changeset.
  /// </summary>
  public class Changeset
  {
    /// <summary>
    /// Initialises a new instance with the specified field values.
    /// </summary>
    /// <param name="repo">Name of the Mercurial repository the change belongs to.</param>
    /// <param name="branch">Name of the branch the change was made on.</param>
    /// <param name="revID">Revision SHA-1 ID.</param>
    /// <param name="message">Commit message.</param>
    /// <param name="author">Name of the person who made the change.</param>
    /// <param name="changeDateTime">When the change was made.</param>
    public Changeset(string repo, string branch, string revID, string message, string author, DateTime changeDateTime)
    {
      Repository = repo;
      RepositorySuggest = repo;
      Branch = string.IsNullOrEmpty(branch) ? "default" : branch;
      BranchSuggest = Branch;
      ID = revID;
      Message = message;
      Author = author;
      AuthorSuggest = author;
      ChangeDateTime = changeDateTime;
    }

    /// <summary>
    /// Name of the repository this change was for.
    /// </summary>
    /// <returns></returns>
    [ElasticProperty(Name = "repository")]
    public string Repository { get; private set; }

    /// <summary>
    /// Field to store suggestions for the repository name
    /// </summary>
    /// <returns></returns>
    [ElasticProperty(Name = "repository_suggest")]
    public string RepositorySuggest { get; private set; }

    /// <summary>
    /// THe branch the change was made on.
    /// </summary>
    /// <returns></returns>
    [ElasticProperty(Name = "branch")]
    public string Branch { get; private set; }

    /// <summary>
    /// Stores suggestions for the branch field.
    /// </summary>
    /// <returns></returns>
    [ElasticProperty(Name = "branch_suggest")]
    public string BranchSuggest { get; private set; }
    
    /// <summary>
    /// The changeset SHA-1 hash.
    /// </summary>
    /// <returns></returns>
    [ElasticProperty(Name = "id")]
    public string ID { get; private set; }

    /// <summary>
    /// The comment message.
    /// </summary>
    /// <returns></returns>
    [ElasticProperty(Name = "message")]
    public string Message { get; private set; }

    /// <summary>
    /// Name of the changeset author.
    /// </summary>
    /// <returns></returns>
    [ElasticProperty(Name = "author")]
    public string Author { get; private set; }

    /// <summary>
    /// Field to hold suggestions for the author field.
    /// </summary>
    /// <returns></returns>
    [ElasticProperty(Name = "author_suggest")]
    public string AuthorSuggest { get; private set; }

    /// <summary>
    /// The data/time when the change was made.
    /// </summary>
    /// <returns></returns>
    [ElasticProperty(Name = "changeDateTime")]
    public DateTime ChangeDateTime { get; private set; }
  }
}
