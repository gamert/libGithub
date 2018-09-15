using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.MSBuild;


namespace RoslyProject
{
    public class BaseMemberDeclaration_t
    {
        public string Field;        //类型,Class? Field?
        public string Identifier;
        public MemberDeclarationSyntax Syntax;  //保存 BaseTypeDeclarationSyntax 

        public string NewName = "";
    }
    //成员声明:
    public class MemberDeclaration_t : BaseMemberDeclaration_t
    {
        //public TypeDeclarationSyntax container; //容器类...
        public TypeDeclaration_t pType;         //子类
        public MemberDeclaration_t(string f, string i, MemberDeclarationSyntax _syntax)
        {
            Field = f;
            Identifier = i;
            Syntax = _syntax;
            //container = _container;
        }
        public override string ToString()
        {
            return Field + ": " + Identifier;
        }
    }

    //类型声明:
    public class TypeDeclaration_t : BaseMemberDeclaration_t
    {
        public bool bMemberSaved = false;

        //父亲列表:
        public void declList(List<TypeDeclaration_t> ls)
        {
            ls.Insert(0, this);
            if (parent != null)
                parent.declList(ls);
        }

        public TypeDeclaration_t parent;    //用来合成名字保持关系...
        public List<MemberDeclaration_t> Members = new List<MemberDeclaration_t>();

        //支持 ClassDeclarationSyntax EnumDeclarationSyntax
        public TypeDeclaration_t(TypeDeclaration_t parent, BaseTypeDeclarationSyntax node)
        {
            Syntax = node;
            HandleDecl(parent, this);
        }

        ////合成唯一的类名字...
        //public string GetUUID()
        //{
        //    string pp = parent != null ? parent.GetUUID()+"+":"";
        //    return pp + identifierToken;
        //}

        /// <summary>
        /// 处理TypeDeclaration_t
        /// </summary>
        /// <param name="tdt"></param>
        public static void HandleDecl(TypeDeclaration_t parent, TypeDeclaration_t tdt)
        {
            BaseTypeDeclarationSyntax btd = tdt.Syntax as BaseTypeDeclarationSyntax;

            tdt.Identifier = btd.Identifier.Text;
            tdt.parent = parent;

            if (btd is TypeDeclarationSyntax)
            {
                HandleTypeMember(btd as TypeDeclarationSyntax, tdt);
            }
            else if (btd is EnumDeclarationSyntax)
            {
                HandleEnumMember(btd as EnumDeclarationSyntax, tdt);
            }
            //sb.AppendLine("\t" + td.Identifier.Text + ";");
        }

        static void HandleEnumMember(EnumDeclarationSyntax td, TypeDeclaration_t tdt)
        {
            for (int k = 0; k < td.Members.Count; ++k)
            {
                MemberDeclarationSyntax member = td.Members[k];
                HandleMemberDeclarationSyntax(member, tdt);
            }
            //           Console.WriteLine(td);
        }

        static void HandleTypeMember(TypeDeclarationSyntax td, TypeDeclaration_t tdt)
        {
            for (int k = 0; k < td.Members.Count; ++k)
            {
                MemberDeclarationSyntax member = td.Members[k];
                HandleMemberDeclarationSyntax(member, tdt);
            }
        }

        static void HandleMemberDeclarationSyntax(MemberDeclarationSyntax member, TypeDeclaration_t tdt)
        {
            //MemberDeclarationSyntax
            //
            if (member is FieldDeclarationSyntax)
            {
                FieldDeclarationSyntax ed = member as FieldDeclarationSyntax;
                VariableDeclarationSyntax decl = ed.Declaration;
                if (decl.Variables.Count > 0)
                {
                    VariableDeclaratorSyntax v = decl.Variables[0];
                    tdt.Members.Add(new MemberDeclaration_t("Field", v.Identifier.Text, ed));
                }

            }
            else if (member is EnumMemberDeclarationSyntax)
            {
                EnumMemberDeclarationSyntax ed = member as EnumMemberDeclarationSyntax;
                tdt.Members.Add(new MemberDeclaration_t("EnumMember", ed.Identifier.Text, ed));
            }
            else if (member is MethodDeclarationSyntax)
            {
                MethodDeclarationSyntax ed = member as MethodDeclarationSyntax;
                tdt.Members.Add(new MemberDeclaration_t("Method", ed.Identifier.Text, ed));
            }
            else if (member is PropertyDeclarationSyntax)
            {
                PropertyDeclarationSyntax ed = member as PropertyDeclarationSyntax;
                tdt.Members.Add(new MemberDeclaration_t("Property", ed.Identifier.Text, ed));
            }

            else if (member is EventFieldDeclarationSyntax)
            {
                EventFieldDeclarationSyntax ed = member as EventFieldDeclarationSyntax;
                VariableDeclarationSyntax decl = ed.Declaration;
                if (decl.Variables.Count > 0)
                {
                    VariableDeclaratorSyntax v = decl.Variables[0];
                    tdt.Members.Add(new MemberDeclaration_t("EventField", v.Identifier.Text, ed));
                }

            }
            else if (member is DelegateDeclarationSyntax)
            {
                DelegateDeclarationSyntax ed = member as DelegateDeclarationSyntax;
                tdt.Members.Add(new MemberDeclaration_t("Delegate", ed.Identifier.Text, ed));
            }
            else if (member is InterfaceDeclarationSyntax)
            {
                InterfaceDeclarationSyntax ed = member as InterfaceDeclarationSyntax;
                //tdt.Members.Add(new MemberDeclaration_t("Interface", ed.Identifier.Text, ed));
                MemberDeclaration_t t = new MemberDeclaration_t("Interface", ed.Identifier.Text, ed);
                tdt.Members.Add(t);
                //处理子节点...
                t.pType = new TypeDeclaration_t(tdt, member as BaseTypeDeclarationSyntax);
            }

            else if (member is EnumDeclarationSyntax)
            {
                EnumDeclarationSyntax ed = member as EnumDeclarationSyntax;
                MemberDeclaration_t t = new MemberDeclaration_t("Enum", ed.Identifier.Text, ed);
                tdt.Members.Add(t);
                //处理子节点...
                t.pType = new TypeDeclaration_t(tdt, member as BaseTypeDeclarationSyntax);
            }
            else if (member is ClassDeclarationSyntax)
            {
                ClassDeclarationSyntax ed = member as ClassDeclarationSyntax;

                MemberDeclaration_t t = new MemberDeclaration_t("Class", ed.Identifier.Text, ed);
                tdt.Members.Add(t);

                //处理子节点...
                t.pType = new TypeDeclaration_t(tdt, member as TypeDeclarationSyntax);
            }
            else if (member is StructDeclarationSyntax)
            {
                StructDeclarationSyntax ed = member as StructDeclarationSyntax;

                MemberDeclaration_t t = new MemberDeclaration_t("Struct", ed.Identifier.Text, ed);
                tdt.Members.Add(t);

                //处理子节点...
                t.pType = new TypeDeclaration_t(tdt, member as TypeDeclarationSyntax);
            }
            else if (member is ConstructorDeclarationSyntax)
            {
                member = member;
            }
            else if (member is DestructorDeclarationSyntax)
            {
                //忽略...
                member = member;
            }
            else if (member is OperatorDeclarationSyntax)
            {
                //忽略...
            }
            else if (member is IndexerDeclarationSyntax)
            {
                //忽略...
                member = member;
            }
            else if (member is ConversionOperatorDeclarationSyntax)
            {
                //忽略...
                member = member;
            }
            else
            {
                member = member;
                //sb.AppendLine("\t\tUnknow;" + member);
            }
            //                            sb.AppendLine("\t\t" + member + ";");
        }
    }

    //需要保存的信息..
    public class DocTypeList_t
    {
        public Document doc;
        public string Name;
        public string FilePath;

        public List<TypeDeclaration_t> typelist = new List<TypeDeclaration_t>();
        public DocTypeList_t(Document _doc)
        {
            doc = _doc;
            Name = doc.Name;
            FilePath = doc.FilePath;
        }
    }

    public class SSolution
    {
        public List<DocTypeList_t> doctypelist = new List<DocTypeList_t>();


        //把TypeName保存一下: 格式..文档名{文件名...；文件名2....}
        public void SaveTypeList(string fn)
        {
            FileStream fp = new FileStream(fn, FileMode.Create);
            if (fp != null)
            {
                StringBuilder sb = new StringBuilder();
                StreamWriter sw = new StreamWriter(fp);
                //逐个处理...
                for (int i = 0; i < doctypelist.Count; ++i)
                {
                    DocTypeList_t doctype = doctypelist[i];
                    sb.Clear();

                    sb.AppendLine(doctype.Name + ";" + doctype.FilePath);
                    for (int j = 0; j < doctype.typelist.Count; ++j)
                    {
                        TypeDeclaration_t tdt = doctype.typelist[j];
                        SaveTypeDeclaration(sb, tdt, "\t");
                    }
                    sw.WriteLine(sb.ToString());
                }

                sw.Close();
                fp.Close();
            }
        }

        //
        void SaveTypeDeclaration(StringBuilder sb, TypeDeclaration_t tdt, string prefix)
        {
            sb.AppendLine(prefix + tdt.Identifier + ";");
            for (int k = 0; k < tdt.Members.Count; ++k)
            {
                MemberDeclaration_t md = tdt.Members[k];
                sb.AppendLine(prefix + "\t" + md.Field + ";" + md.Identifier);
                if (md.pType != null)
                {
                    SaveTypeDeclaration(sb, md.pType, prefix + "\t");
                }
            }
        }
    }


    //遍历所有的Type 声明并保存:
    class ClassDeclarationVisitor : CSharpSyntaxRewriter
    {
        public ClassDeclarationVisitor(DocTypeList_t list)
        {
            doc_typelist = list;
        }
        DocTypeList_t doc_typelist;

        public override SyntaxNode VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            if (doc_typelist != null)
            {
                doc_typelist.typelist.Add(new TypeDeclaration_t(null, node));
            }
            return node;// node.Update(lessThanToken, syntaxList, greaterThanToken);
        }
        //
        public override SyntaxNode VisitStructDeclaration(StructDeclarationSyntax node)
        {
            if (doc_typelist != null)
            {
                doc_typelist.typelist.Add(new TypeDeclaration_t(null, node));
            }
            return node;// node.Update(lessThanToken, syntaxList, greaterThanToken);
        }

        //如何 BaseFieldDeclarationSyntax
        //public override SyntaxNode VisitFieldDeclaration(FieldDeclarationSyntax node)
        //{
        //    return node;
        //}
        public override SyntaxNode VisitEnumDeclaration(EnumDeclarationSyntax node)
        {
            if (doc_typelist != null)
            {
                doc_typelist.typelist.Add(new TypeDeclaration_t(null, node));
            }
            return node;
        }

        public override SyntaxNode VisitInterfaceDeclaration(InterfaceDeclarationSyntax node)
        {
            if (doc_typelist != null)
            {
                doc_typelist.typelist.Add(new TypeDeclaration_t(null, node));
            }
            return node;
        }
    }

}
