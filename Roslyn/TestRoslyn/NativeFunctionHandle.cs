using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;



class ExtractStringLiteralVisitor : SyntaxWalker
{
    readonly List<string> literals = new List<string>();



    /*
     * 句法节点是句法树的主要元素。这些节点呈现了如声明、语句、子句和表达式。每一类句法节点都是通过继承自SyntaxNode的类来表示的。节点类集是不可扩展的。

句法树中所有的语法节点都是非终止节点，意思是它们可以一直有其他节点作为子节点。作为其他节点的子节点，每一个子节点都可以通过Parent属性获取父节点。因为节点和树是固定不变的，因此节点的父节点从来不会变。树的根节点的父节点是null。
     * */
    public virtual void Visit(SyntaxNode node)
    {
        base.Visit(node);
    }

    /*
     * 句法令牌（Syntax Tokens）
句法令牌是语言语法的终端，是代码的最小语法单位。它们从来都不是其他节点或令牌的父辈。句法令牌由关键词、标示符、文本和标点符号组成。

出于效率的目的，SyntaxToken类型是CLR值类型。但是，不像句法节点，对于混合了属性（依赖于所要表示令牌的种类）的所有令牌只有一种结构。
例如，一个整型文本令牌表示一个数字值。此外，对于令牌所指的原始源文本，文本令牌有一个Value属性用来告诉你怎么准确解码整型值。该属性被记为对象类型，因为它可能是许多原始类型中得一种。
*/
    protected virtual void VisitToken(SyntaxToken token)
    {


        base.VisitToken(token);
    }

    //句法杂项是用来表示源文本中那些大量的对于理解代码来说是微不足道部分，比如空白字符、注释和预处理指令。
    protected virtual void VisitTrivia(SyntaxTrivia trivia)
    {
        base.VisitTrivia(trivia);
    }
    //public override void VisitLiteralExpression(LiteralExpressionSyntax node)
    //{
    //    if (node.Kind == SyntaxKind.StringLiteralExpression)
    //        literals.Add(node.ToString());
    //    base.VisitLiteralExpression(node);
    //}

    public IEnumerable<string> Literals { get { return literals; } }
}


//
//class TypeParameterVisitor : CSharpSyntaxRewriter
//{
//    public override SyntaxNode VisitTypeParameterList(TypeParameterListSyntax node)
//    {
//        var lessThanToken = this.VisitToken(node.LessThanToken);
//        var parameters = this.VisitList(node.Parameters);
//        var greaterThanToken = this.VisitToken(node.GreaterThanToken);
//        return node.Update(lessThanToken, parameters, greaterThanToken);
//    }
//}


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

    public override SyntaxNode VisitParameter(ParameterSyntax node)
    {
        return node;
    }


    static string BuildRetStr(TypeSyntax ReturnType)
    {
        string sr = ReturnType.ToString();
        if (sr == "void") return "";
        if (sr == "bool") return "true";
        if (sr == "int" || sr == "uint" || sr == "ulong" || sr == "byte") return "0";
        return string.Format("default({0})", sr);
    }

    //访问函数，并且
    public override SyntaxNode VisitMethodDeclaration(MethodDeclarationSyntax node)
    {
        MethodDeclarationSyntax node_raw = node;

        Console.WriteLine("VisitMethodDeclaration: " + node.Identifier);

        SyntaxTree SF = node.SyntaxTree;
        //增加一个body
        BlockSyntax Body = node.Body;
        if (Body == null)
        {
            //处理[DllImport("CSteamworks", CallingConvention = CallingConvention.Cdecl)]
            foreach (AttributeListSyntax attr in node.AttributeLists)
            {
                if (attr.ToString().StartsWith("[DllImport"))
                {
                    var r = node.AttributeLists.Remove(attr);
                    node = node.WithAttributeLists(r);

                    SyntaxTrivia st = SyntaxFactory.PreprocessingMessage("\t\t//" + attr.ToString() + "\r\n");
                    //改为
                    SyntaxTriviaList s = node.GetLeadingTrivia();
                    if (s != null)
                    {
                        s = s.Add(st);
                        node = node.WithLeadingTrivia(s);
                    }
                    else
                        node = node.WithLeadingTrivia(st);
                    break;
                }
            }

            //SyntaxNode
            SyntaxList<StatementSyntax> statements = new SyntaxList<StatementSyntax>();
            BinaryExpressionSyntax bes = SyntaxFactory.BinaryExpression(SyntaxKind.EqualsExpression, SyntaxFactory.IdentifierName("_a"),
                SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal("X")));

            //ExpressionSyntax e = SyntaxFactory.ParseExpression(string.Format("Debug.Log(\"{0}: \");", node.Identifier));
            //statements.Add(e);
            // StatementSyntax myStatement = SyntaxFactory.ParseStatement(@"Console.WriteLine();");
            //Body.AddStatements(myStatement);
            //statements.Add(bes);
            //CSharpScript.Create<int>("int x = 1;");
            StatementSyntax myStatement = SyntaxFactory.ParseStatement(string.Format("Debug.Log(\"****call to {0}: \");", node.Identifier));
            node = node.AddBodyStatements(myStatement);

            //处理带有out 修饰的参数: 
            SeparatedSyntaxList<ParameterSyntax> Parameters = node.ParameterList.Parameters;
            if (Parameters.Count > 0)
            {
                foreach (ParameterSyntax param in Parameters)
                {
                    foreach (SyntaxToken vv in param.Modifiers)
                    {
                        if (vv.ToString().Contains("out"))
                        {
                            TypeSyntax t1 = param.Type;
                            string t2 = param.Identifier.ToString();
                            StatementSyntax ss = SyntaxFactory.ParseStatement(string.Format("{0} = default({1});", t2, t1.ToString()));
                            node = node.AddBodyStatements(ss);
                            break;
                        }
                    }
                    foreach (AttributeListSyntax vv in param.AttributeLists)
                    {
                        if (vv.ToString().Contains("[Out]"))
                        {
                            TypeSyntax t1 = param.Type;
                            string t2 = param.Identifier.ToString();
                            StatementSyntax ss = SyntaxFactory.ParseStatement(string.Format("{0} = default({1});", t2, t1.ToString()));
                            node = node.AddBodyStatements(ss);
                            break;
                        }
                    }
                }
                //+		AttributeLists	{[In] [Out]}	Microsoft.CodeAnalysis.SyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.AttributeListSyntax>
            }

            //补充返回值:
            string sr = string.Format("return {0};", BuildRetStr(node.ReturnType));
            StatementSyntax ret = SyntaxFactory.ParseStatement(sr);
            node = node.AddBodyStatements(ret).WithSemicolonToken(default(SyntaxToken));

            //处理node的修饰符:
            foreach (SyntaxToken mod in node.Modifiers)
            {
                if (mod.Text == "extern")
                {
                    SyntaxTokenList n = node.Modifiers.Remove(mod);
                    node = node.WithModifiers(n);
                    break;
                }
            }

        }

        // This is new, copies the trivia (indentations, comments, etc.)
        //newVariable = newVariable.WithLeadingTrivia(localDeclarationStatementSyntax.GetLeadingTrivia());
        //newVariable = newVariable.WithTrailingTrivia(localDeclarationStatementSyntax.GetTrailingTrivia());

        //Console.WriteLine(node.AddBodyStatements);
        //StatementSyntax[] items

        //SeparatedSyntaxList<ParameterSyntax> Parameters = node.ParameterListSyntax;
        //if (Parameters.Count > 0)
        //{
        //    for (int i = 0; i < Parameters.Count; ++i)
        //    {
        //        Console.WriteLine(node.Parent);
        //        Console.WriteLine(Parameters[i]);
        //    }
        //}
        node = node.NormalizeWhitespace();
        //Console.WriteLine(node);

        //将新节点替换到旧节点上:
        node_raw.Parent.ReplaceNode(node_raw, node);
        return node;
    }


    public override SyntaxNode VisitParameterList(ParameterListSyntax node)
    {

        var lessThanToken = this.VisitToken(node.OpenParenToken);
        var greaterThanToken = this.VisitToken(node.CloseParenToken);

        //如果带有参数，那么是个函数节点？
        MethodDeclarationSyntax Parent = node.Parent as MethodDeclarationSyntax;
        Console.WriteLine(Parent.ReturnType);

        SeparatedSyntaxList<ParameterSyntax> Parameters = node.Parameters;
        if (Parameters.Count > 0)
        {
            for (int i = 0; i < Parameters.Count; ++i)
            {
                Console.WriteLine(node.Parent);
                Console.WriteLine(Parameters[i]);
            }
        }

        return node;
    }

}

/// <summary>
/// 处理 NativeFunction
/// </summary>
public class CNativeFunctionHandler
{
    public void Main(string[] args)
    {
        //string path = "G:/Dev/Battlerite/Libs/MergedUnity/Scripts/ns25/NativeMethods.cs";
        //            string path = "C:/Users/txz/Desktop/TestRoslyn/NativeMethods_raw.cs";
        string path = "G:/Aotu/worksapce100/DClient2/Trunk/Assets/Src/BattleCore/DataDefine.cs";
        string source = File.ReadAllText(path);

        //var fileToCompile = @"C:\Users\DesktopHome\Documents\Visual Studio 2013\Projects\ConsoleForEverything\SignalR_Everything\Program.cs";
        //var source = File.ReadAllText(fileToCompile);
        //var parsedSyntaxTree = Parse(source, "", CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.CSharp5));

        //
        // SyntaxTree tree = Parse(source, path, CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.CSharp5));
        SyntaxTree tree = CSharpSyntaxTree.ParseText(source);
        if (tree != null)
        {
            // 获得语法树的根结点root，root.KindText为CompliationUnit。注意Using语句通过root.Usings属性返回，而不是作为root的子节点
            CompilationUnitSyntax root = (CompilationUnitSyntax)tree.GetRoot();

            TypeParameterVisitor visitor = new TypeParameterVisitor();
            SyntaxNode node = visitor.Visit(root);
            if (node != null)
            {
                node = node.NormalizeWhitespace();
                File.WriteAllText(path + ".txt", node.ToString());
                Console.WriteLine(node);
            }

            // root只有一个namespace子节点，代表了整个namespace语句，也就是从namespace开始到最后的反大括号之间所有代码，KindText属性值为NamespaceDeclaration
            var firstMember = root.Members[0];
            //foreach(var mem in root.Members)
            //{
            //    Console.WriteLine(mem);
            //}

            // Members列表的默认类型为MemberDeclarationSyntax，这里转换为NamespaceDeclarationSyntax
            NamespaceDeclarationSyntax helloWorldDeclaration = (NamespaceDeclarationSyntax)firstMember;
            // namespace节点只有一个class子节点，代表了整个类型声明语句
            ClassDeclarationSyntax programDeclaration = (ClassDeclarationSyntax)helloWorldDeclaration.Members[0];
            // class节点也只有一个method子节点，代表了整个Main方法

            //foreach (var mem2 in programDeclaration.Members)
            //{
            //    MethodDeclarationSyntax mainDeclaration = (MethodDeclarationSyntax)mem2;
            //    Console.WriteLine(mainDeclaration);
            //    // main方法节点的子节点不再通过Members属性获得，ParameterList返回参数节点列表，Body属性返回方法体节点
            //   // var argsParameter = mainDeclaration.ParameterList.Parameters[0];
            //    foreach (var mem3 in mainDeclaration.ParameterList.Parameters)
            //    {
            //        Console.WriteLine(mem3);
            //    }
            //}
        }

        //var script = CSharpScript.Create(code, ScriptOptions.Default);//.WithPrevious(_previousInput).WithOptions(_options);
        //if(script!=null)
        //{
        //    Console.WriteLine(script.ToString()); 
        //}

        //SyntaxTree syntaxTree = new SyntaxTree();//.ParseText(source);
        //var root = syntaxTree.GetRoot();
        //var visitor = new ExtractStringLiteralVisitor();
        //visitor.Visit(root);
        //foreach (var literal in visitor.Literals)
        //    Console.WriteLine(literal);

    }
}
