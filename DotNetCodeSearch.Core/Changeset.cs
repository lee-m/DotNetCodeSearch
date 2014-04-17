using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNetCodeSearch.Core
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
      Branch = string.IsNullOrEmpty(branch) ? "default" : branch;
      ID = revID;
      Message = message;
      Author = author;
      ChangeDateTime = changeDateTime;
    }

    /// <summary>
    /// Name of the repository this change was for.
    /// </summary>
    /// <returns></returns>
    public string Repository { get; private set; }

    /// <summary>
    /// THe branch the change was made on.
    /// </summary>
    /// <returns></returns>
    public string Branch { get; private set; }

    /// <summary>
    /// The changeset SHA-1 hash.
    /// </summary>
    /// <returns></returns>
    public string ID { get; private set; }

    /// <summary>
    /// The comment message.
    /// </summary>
    /// <returns></returns>
    public string Message { get; private set; }

    /// <summary>
    /// Name of the changeset author.
    /// </summary>
    /// <returns></returns>
    public string Author { get; private set; }

    /// <summary>
    /// The data/time when the change was made.
    /// </summary>
    /// <returns></returns>
    public DateTime ChangeDateTime { get; private set; }
  }
}
