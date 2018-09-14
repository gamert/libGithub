using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.MSBuild;

namespace RoslyProject
{


    public class DumpSolutionTypes
    {
        static string[] IncludeDocNames = new string[]
        {
            @"Assets\Src\BattleCore\",
            //@"Assets\Src\BattleCore\",
        };
        static string[] ExcludeDocNames = new string[]
        {
            @"Assets\Src\BattleCore\Plugin\IL\",
            @"Assets\Src\BattleCore\Plugin\Pathfinder",
            @"Assets\Src\BattleCore\Plugin\Base\MonoXml",
            @"Assets\Src\BattleCore\Game\Battle\Core\ActorAI",
            @"Assets\Src\BattleCore\Misc\Profiler",
            @"Assets\Src\BattleCore\Robot",
            @"Assets\Src\BattleCore\ProtoData",
            @"Assets\Src\BattleCore\Network\MG_Network\Protocols",
        };

        //是否包含某个str
        static bool IsContainsSomeStr(string[] aa,string docName)
        {
            for (int i = 0; i < aa.Length; ++i)
            {
                if (docName.Contains(aa[i]))
                    return true;
            }
            return false;
        }

        //是否使用路径排除..
        static bool ValidDocName(string docName, bool useInclude)
        {
            bool bOk = true;
            if (useInclude)
            {
                bOk = IsContainsSomeStr(IncludeDocNames, docName);
            }
            if (bOk && IsContainsSomeStr(ExcludeDocNames, docName))
                return false;
            return bOk;
        }


        public static async Task DumpAllTypesAsync(string sln, bool useInclude)
        {
            ISolutionHandler handle = new ISolutionHandler(sln);
            await handle.InitSolution(sln);
            await SolutionAsync(handle, useInclude);
            //
            string path = Path.GetDirectoryName(sln);
            Directory.SetCurrentDirectory(path);
            CRenameTokenHandler rhandle = new CRenameTokenHandler();
            rhandle.m_workspace = handle.m_workspace;
            rhandle.newSolution = handle.m_solution;
            rhandle.m_solu = handle.m_solu;
            rhandle.m_sln = handle.m_sln;
            await replaceAllSolutionTypeMemberID(rhandle, useInclude);
            rhandle.SaveNewSolution();
        }

        //SSolution solu,string docName
        // @"G:\GitHub\dotnet\TestRoslyn\Samples\ConsoleApp1\ConsoleApp1.sln"
        public static async Task SolutionAsync(ISolutionHandler handle, bool useInclude)
        {
            CancellationToken cancellationToken = default(CancellationToken);
            if(handle.m_solution == null)
            {
                Console.WriteLine("SolutionAsync: solution == null");
                return;
            }

            //提取所有的Type
            IEnumerable<Project> Projects = handle.m_solution.Projects;
            foreach (Project project in handle.m_solution.Projects)
            {
                if (!project.HasDocuments)
                {
                    continue;
                }

                //		FilePath	"G:\\Aotu\\worksapce100\\DClient2\\Trunk\\Assets\\Cinemachine\\Base\\Runtime\\Behaviours\\CinemachineBlendListCamera.cs"	string
                foreach (Document document in project.Documents)
                {
                    if (!ValidDocName(document.FilePath, useInclude))
                    {
                        continue;
                    }

                    //docName != null
                    if (!handle.preCheck(document))
                    {
                        continue;
                    }
                    //遍历
                    await handleDocument(document, handle.m_dtl);
                    //替换
                    await handle.Run( document);
                    //保存
                    if (handle.postHandle(document))
                        return;
                }

            }
            handle.postHandleSolution();
        }


        //处理一个文档:
        static async Task handleDocument(Document document, DocTypeList_t dtl)
        {
            SyntaxTree tree = await document.GetSyntaxTreeAsync();
            CompilationUnitSyntax syntax = tree.GetCompilationUnitRoot();

            ClassDeclarationVisitor visitor = new ClassDeclarationVisitor(dtl);
            SyntaxNode node = visitor.Visit(syntax);
        }



        //替换SolutionTypeMember
        public static async Task replaceAllSolutionTypeMemberID(CRenameTokenHandler rhandle, bool useInclude)
        {
            int limit = 0;

            CCLog.Info(string.Format("=== replaceAllSolutionTypeMemberID Count: {0} ", rhandle.m_solu.doctypelist.Count));

            int types = 0;
            rhandle.pass_cmd = 1;
            for (int i = 0; i < rhandle.m_solu.doctypelist.Count; ++i)
            {
                DocTypeList_t dtt = rhandle.m_solu.doctypelist[i];
                for (int j = 0; j < dtt.typelist.Count; ++j)
                {
                    TypeDeclaration_t tdt = dtt.typelist[j];

                    CCLog.Debug(string.Format("=== replaceAllSolutionTypeMemberID Type: {0}/{1} ;Doc {2}/{3};{4}/{5}", j, dtt.typelist.Count,i, rhandle.m_solu.doctypelist.Count, tdt.Identifier, dtt.Name));

                    await replaceTypeMemberID(rhandle, dtt.FilePath, tdt, useInclude);

                    types++;
                }
                //if((i%100) == 99)
                //    rhandle.SaveNewSolution();
//                if((i%20) == 19)
                    rhandle.CheckCompileAndCommit();
            }
            rhandle.CheckCompileAndCommit();
            CCLog.Info(string.Format("=== replaceAllSolutionTypeMemberID types: {0} ;compile_fails: {1}", types, rhandle.compile_fails));
        }

        //替换一个类...
        static async Task replaceTypeMemberID(CRenameTokenHandler handle, string FilePath, TypeDeclaration_t tdt, bool useInclude)
        {
            for (int k = 0; k < tdt.Members.Count; ++k)
            {
                MemberDeclaration_t md = tdt.Members[k];
                //替换并保存
                if (md.pType != null)
                {
                    await replaceTypeMemberID(handle, FilePath, md.pType, useInclude);
                    return;
                }

                await handle.beginReplace(tdt, md, FilePath);
                await SolutionAsync(handle, useInclude);
                //if (limit++ > 1)
                //    return;
            }

            {
                await handle.beginReplace(tdt, null, FilePath);
                await SolutionAsync(handle, useInclude);
                //if (limit++ > 1)
                //    return;
            }
        }
    }
}
