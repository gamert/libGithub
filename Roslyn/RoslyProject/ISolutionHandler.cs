using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.SymbolStore;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.CodeAnalysis.Rename;

namespace RoslyProject
{
    public class ISolutionHandler
    {
        public string m_sln;
        public MSBuildWorkspace m_workspace;
        public Solution m_solution;
        public ISolutionHandler(string sln = "")
        {
            m_sln = sln;
        }
        public async Task InitSolution(string sln)
        {
            m_sln = sln;
            m_workspace = MSBuildWorkspace.Create();
            m_solution = await m_workspace.OpenSolutionAsync(sln);

            //m_workspace
        }
        public void CloseSolution()
        {
            if(m_solution!=null)
            {
                m_workspace.CloseSolution();
                m_solution = null;
            }

        }

        public SSolution m_solu = new SSolution();
        public DocTypeList_t m_dtl;

        public virtual bool preCheck(Document document)
        {
            m_dtl = new DocTypeList_t(document);
            m_solu.doctypelist.Add(m_dtl);
            return true;
        }
        public async virtual Task Run(Document document)
        {
        }
        public virtual bool postHandle(Document document)
        {
            return false;
        }


        public virtual void postHandleSolution()
        {
            m_solu.SaveTypeList("TypeList.txt");
        }

        //public static void LogError(string s)
        //{
        //    Console.WriteLine(s);
        //}
    }

    //重命名
    public class CRenameTokenHandler : ISolutionHandler
    {
        public string docName;
        public TypeDeclaration_t ref_tdt;   //动态参数..
        public MemberDeclaration_t ref_md;
        public string newName = "";

        public Solution newSolution;   //下一个可以使用的...
        public int AllCount;    //总替换次数
        public int CurCount;    //当前替换次数

        public int pass_cmd = 1;   //成员 函数 类型..
        public int compile_fails = 0;

        public async Task beginReplace(TypeDeclaration_t tdt, MemberDeclaration_t md, string FilePath)
        {
            this.ref_tdt = tdt;
            this.ref_md = md;
            this.docName = FilePath;
            if (newSolution != null)//
            {
                m_solution = newSolution;
                newSolution = null;
            }
            if (m_solution == null)
            {
                m_solution = await m_workspace.OpenSolutionAsync(this.m_sln);
                //if(m_solution!= m_workspace.CurrentSolution)
                //{
                //    CCLog.Error("m_solution!= m_workspace.CurrentSolution");
                //}
                if (m_solution == null)
                {
                    CCLog.Error("!!!!!!!!!!!!!!!OpenSolutionAsync fail."+ this.m_sln);
                }
            }
        }

        public override bool preCheck(Document document)
        {
            if (document.FilePath != docName)
                return false;
            m_dtl = new DocTypeList_t(document);
            //solu.doctypelist.Add(dtl);
            return true;
        }
        public override bool postHandle(Document document)
        {
            return true;
        }

        //no save
        public override void postHandleSolution()
        {
            //if(newSolution!=null)
            //{
            //    workspace.TryApplyChanges(newSolution);
            //}
        }


        //保存并清空...
        public bool SaveNewSolution()
        {
            if (newSolution != null)
            {
                //编译大工程还有问题，可能是依赖？？
                //bool res = Compiler.CompileSolution(newSolution, ".");
                //CCLog.Info("CompileSolution:"+ res);

                //if (m_solution != m_workspace.CurrentSolution)
                //    m_solution = m_workspace.CurrentSolution;
                bool b = m_workspace.TryApplyChanges(newSolution);
                if(m_solution != m_workspace.CurrentSolution)
                    m_solution = m_workspace.CurrentSolution;
                //if(m_solution != newSolution)
                //{
                //    CCLog.Error("m_solution != newSolution");//保存完后，会生成新的？
                //}
                newSolution = null;
                //m_solution = null;
                //m_workspace.CloseSolution();

                //GC.Collect();
                return b;
            }
            return false;
        }

        //检查结果:
        void PostRenameSymbolAsync(Document document, ISymbol typeSymbol)
        {
            bool b = SaveNewSolution(); // workspace.TryApplyChanges(newSolution);

            CurCount++;

            string sTip = string.Format("{0} RenameSymbol {1} to {2};res={3};CurCount={4}",
                document.Name, typeSymbol, newName, b, CurCount);
            CCLog.Info(sTip);
            //CCLog.Error(" TryApplyChanges = " + b + ";AllCount=" + AllCount + ";CurCount=" + CurCount);
                             //尝试编译？ 
        }

        //
        public void CheckCompileAndCommit()
        {
            if (CurCount>0)
            {
                AllCount += CurCount;    //总替换次数
                CurCount = 0;    //当前替换次数

                int ExitCode = RumCmd(MSBuild, m_sln + param);
                if (ExitCode == 1)
                {
                    CCLog.Error(string.Format("=== Compile Fail: {0}==>{1}", ref_tdt.Identifier, ref_tdt.NewName));
                    RumCmd(git_ext, "reset HEAD");
                    RumCmd(git_ext, "checkout -- .");

                    //强制重新载入工程...
                    m_workspace.CloseSolution();
                    m_solution = null;
                    compile_fails++;
                }
                else
                {
                    RumCmd(git_ext, "add .");
                    RumCmd(git_ext, string.Format("commit -m \"{0}==>{1} ;Tool Commits\"", ref_tdt.Identifier, ref_tdt.NewName));//
                }
            }
        }


        static string MSBuild = @"C:\Program Files (x86)\Microsoft Visual Studio\2017\Community\MSBuild\15.0\Bin\amd64\MSBuild.exe ";
        //string sln  = @"G:\Aotu\worksapce100\DClient2\Trunk\Trunk.sln";
        static string param = @" /maxcpucount /p:Configuration=Debug /p:AllowUnsafeBlocks=true";
        static string git_ext = @"G:\ToolBase\SmartGit/git/bin/git.exe";

        static int RumCmd(string fileName, string arguments)
        {
            Process exep = new Process();
            exep.StartInfo.FileName = fileName; //命令
            exep.StartInfo.Arguments = arguments;//m_sln + param
            exep.StartInfo.UseShellExecute = false; //不启用shell启动进程
            exep.StartInfo.RedirectStandardInput = true; // 重定向输入
            exep.StartInfo.RedirectStandardOutput = true; // 重定向标准输出
            exep.StartInfo.RedirectStandardError = true; // 重定向错误输出 
            exep.StartInfo.CreateNoWindow = true; // 不创建新窗口
            exep.OutputDataReceived += OnOutputDataReceived;
            exep.Start();
            //Process exep = Process.Start(fileName, arguments);
            try
            {
                string output = exep.StandardOutput.ReadToEnd();
            }
            catch (Exception e)
            {

            }
            exep.WaitForExit();
            return exep.ExitCode;
        }

        private static void OnOutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e != null && !string.IsNullOrEmpty(e.Data))
            {
               // Debug.Log(e.Data);
            }
        }

        //查找对应的节点
        BaseMemberDeclaration_t FindSyntaxNode()
        {
            List<TypeDeclaration_t> ref_ls = new List<TypeDeclaration_t>();
            ref_tdt.declList(ref_ls);
            for (int i = 0; i < m_dtl.typelist.Count; ++i)
            {
                TypeDeclaration_t td = m_dtl.typelist[i];

                TypeDeclaration_t dst = FindTypeDeclaration(td, ref_ls, 0); //类型也要符合...
                if (dst != null)
                {
                    return FindMemberSyntaxNode(dst);
                }
            }
            return null;
        }

        /// <param name="td"></param>
        /// <param name="ref_ls"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        TypeDeclaration_t FindTypeDeclaration(TypeDeclaration_t td, List<TypeDeclaration_t> ref_ls, int index)
        {
            if (td == null)
                return null;

            bool bLast = ref_ls.Count == index + 1;
            if (td.Identifier == ref_ls[index].Identifier)
            {
                if (bLast)
                {
                    return td;
                }

                for (int i = 0; i < td.Members.Count; ++i)
                {
                    MemberDeclaration_t md = td.Members[i];
                    if (md.pType != null)
                    {
                        TypeDeclaration_t sub = FindTypeDeclaration(md.pType, ref_ls, index + 1);
                        if (sub != null)
                        {
                            return sub;
                        }
                    }
                }
            }
            return null;
        }

        //从匹配的Type中找到
        BaseMemberDeclaration_t FindMemberSyntaxNode(TypeDeclaration_t td)
        {
            if (ref_md != null)
            {
                for (int j = 0; j < td.Members.Count; ++j)
                {
                    MemberDeclaration_t md = td.Members[j];
                    //如果是儿子？？
                    if (md.Identifier == ref_md.Identifier && md.Field == ref_md.Field)
                    {
                        newName = "_ZTest"+ md.Identifier;// new string(md.Identifier.ToCharArray().Reverse().ToArray());
                        ref_md.NewName = newName;//保存新名字：
                        return md;
                    }
                }
                return null;
            }
            newName = "_ZTest" + td.Identifier;// new string(td.Identifier.ToCharArray().Reverse().ToArray());
            
            ref_tdt.NewName = newName;//保存新名字：
            return td;
        }

        //}
        /// <param name="document"></param>
        /// <param name="dtl_"></param>
        /// <param name="refMem"></param>
        /// <returns></returns>
        public async override Task Run(Document document)
        {
            CancellationToken cancellationToken = default(CancellationToken);
            //如果是我要的文档?
            BaseMemberDeclaration_t memberDel = FindSyntaxNode();
            if (memberDel == null)
            {
                CCLog.Error("Run: 没有找到 " + memberDel);
                return;
            }
            SyntaxNode typeDecl = memberDel.Syntax;
            ISymbol typeSymbol = null;
            SemanticModel semanticModel = await document.GetSemanticModelAsync(cancellationToken);

            if (typeDecl is FieldDeclarationSyntax)
            {
                //IFieldSymbol fieldSybmol = null;
                FieldDeclarationSyntax fieldDecl = typeDecl as FieldDeclarationSyntax;
                typeSymbol = semanticModel.GetDeclaredSymbol(fieldDecl.Declaration.Variables.First());
                if(typeSymbol!=null)
                {
                    if (IsSerialClassMember(typeSymbol))
                        return;
                }
            }
            else if (typeDecl is EventFieldDeclarationSyntax)
            {
                EventFieldDeclarationSyntax fieldDecl = typeDecl as EventFieldDeclarationSyntax;
                typeSymbol = semanticModel.GetDeclaredSymbol(fieldDecl.Declaration.Variables.First());
            }
            else
            {
                typeSymbol = semanticModel.GetDeclaredSymbol(typeDecl, cancellationToken);
                if (typeSymbol != null)
                {
                    if (IsSerialClass(typeSymbol))
                        return;
                }
            }

            if (typeSymbol == null)
            {
                CCLog.Error("Run: 没有找到typeSymbol" + typeDecl);
                return;
            }
            // 如果是重载了MonoBehavior的几个函数，这里要过滤掉
            if (ShouldIgnoreMonoBehaviorKnownFunc(typeSymbol))
            {
                return;
            }

            //if(pass_cmd == 1)
            //{
            //    if (ref_md == null)
            //        return;
            //    if (typeSymbol.Kind != SymbolKind.Field)
            //        return;
            //}
            //else if (pass_cmd == 2)
            //{
            //    if (typeSymbol.Kind != SymbolKind.Method)
            //        return;
            //}
            if (typeSymbol.Kind == SymbolKind.Method && ref_tdt.bMemberSaved == false)
            {
                CheckCompileAndCommit();
                //首先保存一下
                ref_tdt.bMemberSaved = true;
            }

            // Produce a new solution that has all references to that type renamed, including the declaration.
            var originalSolution = document.Project.Solution;
            var optionSet = originalSolution.Workspace.Options;
            newSolution = await Renamer.RenameSymbolAsync(document.Project.Solution, typeSymbol, newName, optionSet, cancellationToken).ConfigureAwait(false);
            //AutoSaveSolution(newSolution, document.Project.Solution);

            PostRenameSymbolAsync(document, typeSymbol);
        }


        //
        static bool IsSerialClass(ISymbol typeSymbol)
        {
            INamedTypeSymbol containerType = typeSymbol as INamedTypeSymbol;
            if (containerType!=null && INamedTypeSymbol_IsSerializable(containerType))
            {
                return true;
            }
            return false;
        }
        static bool IsSerialClassMember(ISymbol typeSymbol)
        {
            INamedTypeSymbol containerType = typeSymbol.ContainingType;
            if (INamedTypeSymbol_IsSerializable(containerType))
            {
                if (typeSymbol.DeclaredAccessibility == Accessibility.Public)
                {
                    bool hasNonSerializedAttribute = hasAttribute(typeSymbol, "NonSerialized");  //NonSerializedAttribute                  
                    return !hasNonSerializedAttribute;
                }
            }
            return false;
            //container.AttributeOwner
            //容器类 具有 [System.Serializable]属性；且为public，且无[System.NonSerialized]，可以修改...
        }
        static bool INamedTypeSymbol_IsSerializable(INamedTypeSymbol containerType)
        {
            return containerType.IsSerializable || hasAttribute(containerType, "Serializable");//"SerializedAttribute"
        }
        //attrToken = "NonSerializedAttribute"
        public static bool hasAttribute( ISymbol typeSymbol,string attrToken)
        {
            foreach (AttributeData attr in typeSymbol.GetAttributes())
            {
                INamedTypeSymbol sy = attr.AttributeClass;
                if (sy.Name == attrToken)// is System.NonSerializedAttribute
                {
                    return true;
                    //CCLog.Error("sy is System.NonSerializedAttribute");
                }
                // CCLog.Error(attr.ToString());
            }
            return false;
        }

        //ScriptableObject
        static bool ShouldIgnoreMonoBehaviorKnownFunc(ISymbol typeSymbol)
        {
            //虚函数(虚属性)过滤..因为某些虚函数可能来自于DLL，直接的替换，可能导致错误...TODO: 带有NEW标记的？
            if (typeSymbol.IsOverride)
            {
                return true;
            }

            //if func?
            if (typeSymbol.Kind != SymbolKind.Method)
            {
                return false;
            }

            INamedTypeSymbol parentType = typeSymbol.ContainingType;
            return IsMonoBehaviorKnownFunc(typeSymbol.Name);
        }
        static string[] s_MonoBehaviorKnownFunc = new string[] {
            "Awake","Start","Update","LateUpdate", "FixedUpdate" ,
            "OnEnable" , "OnDisable","OnDestroy",
            "OnBecameInvisible", "OnBecameVisible",
            "OnApplicationFocus"  , "OnApplicationPause"  , "OnApplicationQuit" ,
            "OnTriggerEnter", "OnTriggerStay" , "OnTriggerExit" ,
            "OnCollisionStay" , "OnCollisionExit" , "OnCollisionEnter" ,
            "OnMouseDown" , "OnMouseEnter" , "OnMouseExit" , "OnMouseUp" , "OnMouseDrag" , "OnMouseOver", "OnMouseUpAsButton"  ,
            "OnDrawGizmosSelected", "OnDrawGizmos", "OnGUI" , "OnLevelWasLoaded",
            //特定的接口类..也需要排查
            "Compare","Dispose"
        };//, "" 

        static bool IsMonoBehaviorKnownFunc(string name)
        {
            return s_MonoBehaviorKnownFunc.Contains(name);
        }


        //保存Solution: 可以使用新老树做对比，加速: GetChangedSpans
        //async static void AutoSaveSolution(Solution newSolution, Solution oldSolution)
        //{
        //    CancellationToken cancellationToken = default(CancellationToken);
        //    //遍历工程，遍历文档，获取节点逐个保存
        //    IEnumerable<Project> Projects = newSolution.Projects;//.GetEnumerator()
        //    foreach (var project in newSolution.Projects)
        //    {
        //        foreach (var document in project.Documents)
        //        {                    
        //            SyntaxTree tree = await document.GetSyntaxTreeAsync();
        //            CompilationUnitSyntax node = (CompilationUnitSyntax)tree.GetRoot();
        //            if (node != null)
        //            {
        //                node = node.NormalizeWhitespace();
        //                string path = document.FilePath;
        //                File.WriteAllText(path, node.ToString());
        //            }
        //        }
        //    }
        //}
    }

}
