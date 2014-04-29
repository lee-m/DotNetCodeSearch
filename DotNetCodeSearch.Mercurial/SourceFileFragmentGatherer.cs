using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;

using DotNetCodeSearch.Elasticsearch;

namespace DotNetCodeSearch.Mercurial
{
  public class SourceFileFragmentGatherer : SyntaxWalker
  {
    /// <summary>
    /// List of gathered fragments.
    /// </summary>
    private List<string> mFragments;

    #region Public Methods

    public SourceFileFragmentGatherer()
      : base(SyntaxWalkerDepth.Node)
    {
      mFragments = new List<string>();
    }

    public IEnumerable<string> GetFragments(string fileContents)
    {
      SyntaxTree syntaxTree = VisualBasicSyntaxTree.ParseText(fileContents);
      CompilationUnitSyntax compilationUnit = (CompilationUnitSyntax)syntaxTree.GetRoot();
      Visit(compilationUnit);

      return mFragments;
    }

    public override void Visit(SyntaxNode node)
    {
      switch (node.VisualBasicKind())
      {
      case SyntaxKind.AddressOfExpression:
      case SyntaxKind.AnonymousObjectCreationExpression:
      case SyntaxKind.ArrayCreationExpression:
      case SyntaxKind.AttributesStatement:
      case SyntaxKind.AddAssignmentStatement:
      case SyntaxKind.AddHandlerStatement:
      case SyntaxKind.CaseStatement:
      case SyntaxKind.CatchFilterClause:
      case SyntaxKind.CatchStatement:
      case SyntaxKind.ConcatenateAssignmentStatement:
      case SyntaxKind.ConcatenateExpression:
      case SyntaxKind.DelegateFunctionStatement:
      case SyntaxKind.DelegateSubStatement:
      case SyntaxKind.DivideAssignmentStatement:
      case SyntaxKind.DoStatement:
      case SyntaxKind.ElseIfStatement:
      case SyntaxKind.EqualsExpression:
      case SyntaxKind.ExponentiateAssignmentStatement:
      case SyntaxKind.FunctionLambdaHeader:
      case SyntaxKind.IntegerDivideAssignmentStatement:
      case SyntaxKind.LeftShiftAssignmentStatement:
      case SyntaxKind.MidAssignmentStatement:
      case SyntaxKind.ModuleStatement:
      case SyntaxKind.MultiLineFunctionLambdaExpression:
      case SyntaxKind.MultiLineSubLambdaExpression:
      case SyntaxKind.MultiplyAssignmentStatement:
      case SyntaxKind.NamespaceStatement:
      case SyntaxKind.ParenthesizedExpression:
      case SyntaxKind.PropertyStatement:
      case SyntaxKind.QueryExpression:
      case SyntaxKind.RaiseEventStatement:
      case SyntaxKind.ReDimStatement:
      case SyntaxKind.ReDimPreserveStatement:
      case SyntaxKind.RemoveHandlerStatement:
      case SyntaxKind.RightShiftAssignmentStatement:
      case SyntaxKind.SelectStatement:
      case SyntaxKind.SimpleMemberAccessExpression:
      case SyntaxKind.SingleLineFunctionLambdaExpression:
      case SyntaxKind.SingleLineIfStatement:
      case SyntaxKind.SingleLineSubLambdaExpression:
      case SyntaxKind.StructureStatement:
      case SyntaxKind.SubtractAssignmentStatement:
      case SyntaxKind.SubLambdaHeader:
      case SyntaxKind.SubNewStatement:
      case SyntaxKind.SubStatement:
      case SyntaxKind.SyncLockStatement:
      case SyntaxKind.TernaryConditionalExpression:
      case SyntaxKind.ThrowStatement:
      case SyntaxKind.UsingStatement:
      case SyntaxKind.WhileStatement:
      case SyntaxKind.WithStatement:
        {
          mFragments.Add(node.ToString());
          break;
        }

      //Stop traversing the tree when we reach these, there's nothing interesting within them
      //so no point continuing
      case SyntaxKind.ClassStatement:
      case SyntaxKind.HandlesClause:
      case SyntaxKind.ImplementsStatement:
      case SyntaxKind.InheritsStatement:
      case SyntaxKind.InterfaceStatement:
      case SyntaxKind.ParameterList:
        return;

      case SyntaxKind.EnumMemberDeclaration:
      case SyntaxKind.EnumStatement:
      case SyntaxKind.EraseStatement:
      case SyntaxKind.EventStatement:
      case SyntaxKind.FieldDeclaration:
      case SyntaxKind.ForEachStatement:
      case SyntaxKind.ForStatement:
      case SyntaxKind.FunctionStatement:
      case SyntaxKind.GoToStatement:
      case SyntaxKind.IfStatement:
      case SyntaxKind.ImportsStatement:
      case SyntaxKind.InvocationExpression:
      case SyntaxKind.LabelStatement:
      case SyntaxKind.LocalDeclarationStatement:
      case SyntaxKind.MembersImportsClause:
      case SyntaxKind.OnErrorGoToZeroStatement:
      case SyntaxKind.OnErrorGoToMinusOneStatement:
      case SyntaxKind.OnErrorGoToLabelStatement:
      case SyntaxKind.OnErrorResumeNextStatement:
      case SyntaxKind.OperatorStatement:
      case SyntaxKind.PrintStatement:
      case SyntaxKind.ResumeLabelStatement:
      case SyntaxKind.SimpleAssignmentStatement:
      case SyntaxKind.YieldStatement:
        {
          //Not interested in child nodes
          mFragments.Add(node.ToString());
          return;
        }

      case SyntaxKind.ReturnStatement:
        {
          ReturnStatementSyntax returnnode = (ReturnStatementSyntax)node;

          //Only interested in return statements which actually return something.
          if (returnnode.Expression != null)
            mFragments.Add(node.ToString());

          //Not interested in any child nodes
          return;
        }

      case SyntaxKind.ClassBlock:
        {
          //Combine the inherits and implements sub-clauses into the class decl
          ClassBlockSyntax classBlock = (ClassBlockSyntax)node;

          StringBuilder bldr = new StringBuilder();
          bldr.AppendLine(classBlock.Begin.ToString());

          foreach (SyntaxNode inherits in classBlock.Inherits)
          {
            bldr.Append("    ");
            bldr.AppendLine(inherits.ToString());
          }

          foreach (SyntaxNode implements in classBlock.Implements)
          {
            bldr.Append("    ");
            bldr.AppendLine(implements.ToString());
          }

          mFragments.Add(bldr.ToString());
          break;
        }

      case SyntaxKind.InterfaceBlock:
        {
          InterfaceBlockSyntax interfaceBlock = (InterfaceBlockSyntax)node;
          StringBuilder bldr = new StringBuilder();

          bldr.AppendLine(interfaceBlock.Begin.ToString());

          foreach (SyntaxNode inherits in interfaceBlock.Inherits)
          {
            bldr.Append("    ");
            bldr.AppendLine(inherits.ToString());
          }

          mFragments.Add(bldr.ToString());
          break;
        }
      }

      base.Visit(node);
    }

    #endregion
  }
}
