using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Roslyn.Compilers;
using Roslyn.Compilers.CSharp;
using Roslyn.Services;
using Roslyn.Services.Editor;

namespace VSMRolynCTP.DemoCodeIssue
{
    class StaticMethodCodeAction : ICodeAction
    {
        ICodeActionEditFactory _editFactory;
        IDocument _document;
        MethodDeclarationSyntax _methodDeclaration;

        public StaticMethodCodeAction(ICodeActionEditFactory editFactory,
        IDocument document, MethodDeclarationSyntax methodDeclaration)
        {
            _editFactory = editFactory;
            _document = document;
            _methodDeclaration = methodDeclaration;
        }

        public string Description
        {
            get { return "Make method static"; }
        }

        public ICodeActionEdit GetEdit(System.Threading.CancellationToken cancellationToken)
        {
            var firstToken = _methodDeclaration.GetFirstToken();
            var firstTokenTrivia = firstToken.LeadingTrivia;

            var formattedMethodDeclaration = _methodDeclaration.ReplaceToken(
                firstToken, firstToken.WithLeadingTrivia(Syntax.TriviaList()));

            var staticToken = Syntax.Token(firstTokenTrivia, SyntaxKind.StaticKeyword);

            var newModifiers = Syntax.TokenList(
                new[] { staticToken }.Concat(formattedMethodDeclaration.Modifiers));

            var newMethodDeclaration = formattedMethodDeclaration.Update(
                formattedMethodDeclaration.Attributes,
                newModifiers,
                formattedMethodDeclaration.ReturnType,
                formattedMethodDeclaration.ExplicitInterfaceSpecifierOpt,
                formattedMethodDeclaration.Identifier,
                formattedMethodDeclaration.TypeParameterListOpt,
                formattedMethodDeclaration.ParameterList,
                formattedMethodDeclaration.ConstraintClauses,
                formattedMethodDeclaration.BodyOpt,
                formattedMethodDeclaration.SemicolonTokenOpt);
          
            var formattedLocalDeclaration = CodeActionAnnotations.FormattingAnnotation
                .AddAnnotationTo(newMethodDeclaration);

            var tree = (SyntaxTree)_document.GetSyntaxTree();
            var newRoot = tree.Root.ReplaceNode(_methodDeclaration,
                formattedLocalDeclaration);

            return _editFactory.CreateTreeTransformEdit(_document.Project.Solution,
                tree, newRoot, cancellationToken: cancellationToken);
        }

        public System.Windows.Media.ImageSource Icon
        {
            get { return null; }
        }
    }
}
