using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.MSBuild;

namespace Walterlv.Demo.Roslyn
{
    class Program
    {
        static void Main(string[] args)
        {
            RunAsync().Wait();
        }

        private static async Task RunAsync()
        {
            var solution = await MSBuildWorkspace.Create().OpenSolutionAsync(
                @"D:\Developments\Open\MSTestEnhancer\MSTest.Extensions.sln");
            var project = solution.Projects.First(x => x.Name == "MSTest.Extensions");
            var document = project.Documents.First(x =>
                x.Name.Equals("ContractTestContext.cs", StringComparison.InvariantCultureIgnoreCase));

            var tree = await document.GetSyntaxTreeAsync();
            var syntax = tree.GetCompilationUnitRoot();

            var visitor = new TypeParameterVisitor();
            var node = visitor.Visit(syntax);

            var text = node.GetText();
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