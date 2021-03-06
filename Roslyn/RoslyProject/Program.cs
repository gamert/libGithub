﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RoslyProject;

namespace Walterlv.Demo.Roslyn
{
    class Program
    {

        static void Main(string[] args)
        {
            String rootPath = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
            String projPath = Path.GetFullPath(@"..\..\..\") + @"Samples\ConsoleApp1\ConsoleApp1.sln";

            CCLog.Debug("Main");
            //DumpSolutionTypes.DumpAllTypesAsync(projPath,false).Wait();
            DumpSolutionTypes.DumpAllTypesAsync(@"G:\Aotu\worksapce100\DClient2\Trunk\Trunk.sln",true).Wait();

        }

    }

    //class TypeParameterVisitor : CSharpSyntaxRewriter
    //{
    //    public override SyntaxNode VisitTypeParameterList(TypeParameterListSyntax node)
    //    {
    //        var syntaxList = new SeparatedSyntaxList<TypeParameterSyntax>();
    //        syntaxList = syntaxList.Add(SyntaxFactory.TypeParameter("TParameter"));

    //        var lessThanToken = this.VisitToken(node.LessThanToken);
    //        var greaterThanToken = this.VisitToken(node.GreaterThanToken);
    //        return node.Update(lessThanToken, syntaxList, greaterThanToken);
    //    }
    //}
}