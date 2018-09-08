using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.CodeAnalysis.Rename;
using Microsoft.CodeAnalysis.Text;

namespace Walterlv.Demo.Roslyn
{
    class Program
    {
        static void Main(string[] args)
        {
            RunAsync().Wait();
        }

        private static async Task RunAsync()
        {//G:\GitHub\dotnet\otnet-campus\MSTestEnhancer\MSTest.Extensions.sln
            Solution solution = await MSBuildWorkspace.Create().OpenSolutionAsync(
                @"G:\GitHub\libGitHub\Roslyn\Samples\ConsoleApp1\ConsoleApp1.sln");
            Project project = solution.Projects.First(x => x.Name == "ConsoleApp1");
            if(!project.HasDocuments)
            {
                return;
            }
            Document document = project.Documents.First(x =>
                x.Name.Equals("DataDefine.cs", StringComparison.InvariantCultureIgnoreCase));
            //IEnumerable<Document> itr = project.Documents;
            //Document document = itr.First(x =>true);

            CancellationToken cancellationToken = default(CancellationToken);

            SyntaxTree tree = await document.GetSyntaxTreeAsync();
            CompilationUnitSyntax syntax = tree.GetCompilationUnitRoot();

            //TypeParameterVisitor visitor = new TypeParameterVisitor();
            //SyntaxNode node = visitor.Visit(syntax);

            //SourceText text = node.GetText();
            //File.WriteAllText(document.FilePath, text.ToString());

            ClassDeclarationVisitor visitor = new ClassDeclarationVisitor();
            SyntaxNode node = visitor.Visit(syntax);

            for(int i = 0;i < ClassDeclarationVisitor.typelist.Count;++i)
            {
                TypeDeclarationSyntax typeDecl = ClassDeclarationVisitor.typelist[i];
                //Renamer.RenameSymbolAsync
                // Produce a reversed version of the type declaration's identifier token.
                var identifierToken = typeDecl.Identifier;
                var newName = new string(identifierToken.Text.ToCharArray().Reverse().ToArray());
                // Get the symbol representing the type to be renamed.
                var semanticModel = await document.GetSemanticModelAsync(cancellationToken);
                var typeSymbol = semanticModel.GetDeclaredSymbol(typeDecl, cancellationToken);
                // Produce a new solution that has all references to that type renamed, including the declaration.
                var originalSolution = document.Project.Solution;
                var optionSet = originalSolution.Workspace.Options;
                Solution newSolution = await Renamer.RenameSymbolAsync(document.Project.Solution, typeSymbol, newName, optionSet, cancellationToken).ConfigureAwait(false);
                //newSolution.
                AutoSave(newSolution);
            }
        }//end task

        async static void AutoSave(Solution newSolution)
        {
            CancellationToken cancellationToken = default(CancellationToken);
            //遍历工程，遍历文档，获取节点逐个保存
            IEnumerable<Project> Projects = newSolution.Projects;//.GetEnumerator()
            foreach (var project in newSolution.Projects)
            {
                foreach (var document in project.Documents)
                {
                    SyntaxTree tree = await document.GetSyntaxTreeAsync();
                    CompilationUnitSyntax node = (CompilationUnitSyntax)tree.GetRoot();
                    if (node != null)
                    {
                        node = node.NormalizeWhitespace();
                        string path = document.FilePath;
                        File.WriteAllText(path, node.ToString());
                    }
                }
            }
        }
    }


    class ClassDeclarationVisitor : CSharpSyntaxRewriter
    {
        public static List<TypeDeclarationSyntax> typelist = new List<TypeDeclarationSyntax>();

        public override SyntaxNode VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            //
            TypeDeclarationSyntax typeDecl = node as TypeDeclarationSyntax;
            typelist.Add(typeDecl);

            return node;// node.Update(lessThanToken, syntaxList, greaterThanToken);
        }
    }

    class TypeParameterVisitor : CSharpSyntaxRewriter
    {
        public override SyntaxNode VisitTypeParameterList(TypeParameterListSyntax node)
        {
            var syntaxList = new SeparatedSyntaxList<TypeParameterSyntax>();
            syntaxList = syntaxList.Add(SyntaxFactory.TypeParameter("TParameter"));

            var lessThanToken = this.VisitToken(node.LessThanToken);
            var greaterThanToken = this.VisitToken(node.GreaterThanToken);
            return node.Update(lessThanToken, syntaxList, greaterThanToken);
        }
    }
}