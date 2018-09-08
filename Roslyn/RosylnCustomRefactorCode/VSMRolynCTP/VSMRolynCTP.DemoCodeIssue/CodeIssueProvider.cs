using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading;
using Roslyn.Compilers;
using Roslyn.Compilers.Common;
using Roslyn.Compilers.CSharp;
using Roslyn.Services;
using Roslyn.Services.Editor;

namespace VSMRolynCTP.DemoCodeIssue
{
    [ExportSyntaxNodeCodeIssueProvider("VSMRolynCTP.DemoCodeIssue", LanguageNames.CSharp,
        typeof(MethodDeclarationSyntax))]
    class CodeIssueProvider : ICodeIssueProvider
    {
        private readonly ICodeActionEditFactory editFactory;

        [ImportingConstructor]
        public CodeIssueProvider(ICodeActionEditFactory editFactory)
        {
            this.editFactory = editFactory;
        }

        public IEnumerable<CodeIssue> GetIssues(IDocument document, CommonSyntaxNode node, CancellationToken cancellationToken)
        {
            var methodDeclaration = (MethodDeclarationSyntax)node;
            var semanticModel = document.GetSemanticModel(cancellationToken);

            if (methodDeclaration.DescendentTokens().Any(x => x.Kind == SyntaxKind.StaticKeyword))
            {
                yield return null;
            }
            else
            {
                IEnumerable<IdentifierNameSyntax> memberIdentifiers = from m in methodDeclaration.DescendentNodes()
                                                                where m as IdentifierNameSyntax != null
                                                                && IsClassMember(cancellationToken,semanticModel,m as ExpressionSyntax)
                                                                select m as IdentifierNameSyntax;

                if (!memberIdentifiers.Any())
                {
                    yield return new CodeIssue(CodeIssue.Severity.Info, methodDeclaration.Span,
                   String.Format("{0} can be made static.", methodDeclaration),
                   new StaticMethodCodeAction(editFactory, document, methodDeclaration));
                }
            }
        }

        private static bool IsClassMember(CancellationToken cancellationToken, ISemanticModel semanticModel, ExpressionSyntax syntax)
        {
            bool isMember = false;

            var memberInfo = semanticModel.GetSemanticInfo(syntax, cancellationToken);
            
            if (memberInfo.Symbol != null && !memberInfo.Symbol.IsStatic && 
                memberInfo.Symbol.OriginalDefinition.Kind != CommonSymbolKind.Local)
            { 
                isMember = true;
            }

            return isMember;
        }

        #region Unimplemented ICodeIssueProvider members

        public IEnumerable<CodeIssue> GetIssues(IDocument document, CommonSyntaxToken token, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<CodeIssue> GetIssues(IDocument document, CommonSyntaxTrivia trivia, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
