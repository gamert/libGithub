using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.MSBuild;
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
            var document = project.Documents.First(x =>
                x.Name.Equals("DataDefine.cs", StringComparison.InvariantCultureIgnoreCase));
            //IEnumerable<Document> itr = project.Documents;
            //Document document = itr.First(x =>true);

            SyntaxTree tree = await document.GetSyntaxTreeAsync();
            CompilationUnitSyntax syntax = tree.GetCompilationUnitRoot();

            TypeParameterVisitor visitor = new TypeParameterVisitor();
            SyntaxNode node = visitor.Visit(syntax);

            SourceText text = node.GetText();
            File.WriteAllText(document.FilePath, text.ToString());
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