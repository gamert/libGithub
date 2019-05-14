/*
 * for covent vs Vcxproj to cross-compiler as android gcc
 * 2018/11/14
 * 
 */


using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Serialization;

namespace Helper
{
    public class CVcxproj
    {
        //用户定义的配置项: -std=gnu++11
        public string AndroidAPILevel = "android-19";
        public string pre_PreprocessorDefinitions = "_LINUX;ANDROID;POSIX;GNUC;__ANDROID__;";
        public string AdditionalOptions = "  -Wmultichar -Wno-unused-private-field -Wno-unused-local-typedef -Wno-unused-variable -Wno-invalid-source-encoding -Wno-overloaded-virtual -Wno-undefined-bool-conversion -Wno-writable-strings -Wno-reorder -Wno-tautological-compare -Wno-c++11-narrowing -Wno-unused-value %(AdditionalOptions)";

        //新建的Doc
        public XmlDocument newDoc = new XmlDocument();
        public XmlElement newRoot;

        public XmlElement PropertyGroup_Globals;


        //根据旧的工程创建一个新的工程文件...
        public void InitNew()
        {
            //创建Xml声明部分，即<?xml
            XmlDeclaration  decl = newDoc.CreateXmlDeclaration("1.0", "utf-8", "yes");
            newDoc.AppendChild(decl);
            //创建根节点
            newRoot = newDoc.CreateElement("Project");
            newDoc.AppendChild(newRoot);

            //rootNode
            newRoot.SetAttribute("DefaultTargets", "Build");
            newRoot.SetAttribute("ToolsVersion", "15.0");
            newRoot.SetAttribute("xmlns", "http://schemas.microsoft.com/developer/msbuild/2003");

            // rootNode.SetAttributeNode()
            XmlElement ItemGroup = Project_add_Node("ItemGroup", "Label", "ProjectConfigurations");
            Add_Group_PlatConf(
               delegate (string Plat, string Conf, string Condition)
               {
                   XmlNode sub = CreateProjectConfiguration(Conf, Plat);
                   ItemGroup.AppendChild(sub);
               }
               );

            Add_PropertyGroup_Globals("{C1F19423-8307-4399-B50D-C5021C083984}");

            Project_add_Node("Import", "Project", "$(VCTargetsPath)\\Microsoft.Cpp.Default.props");

            /////////////////////////////////////////////////
            string rCondition = "'$(Configuration)|$(Platform)'=='Debug|Win32'";
            //string xpath = ".//PropertyGroup[Condition=\"" + rCondition + "\"]";
            //XmlNode temp = Project.SelectSingleNode(xpath);
            XmlNode temp = FindSubXmlElement2(rawProjectRoot, "PropertyGroup", "Condition", rCondition, "Label", "Configuration");
            string ConfigurationType = GetSubNodeInnerText(temp, "ConfigurationType");
            //string ConfigurationType = "StaticLibrary";//DynamicLibrary
            Add_Group_PlatConf(
                delegate (string Plat, string Conf, string Condition)
                {
                    XmlElement sub = Project_add_Node("PropertyGroup", "Condition", Condition);
                    sub.SetAttribute("Label", "Configuration");
                    AddSubNodeWithInnerText(sub, "ConfigurationType", ConfigurationType);
                    AddSubNodeWithInnerText(sub, "UseDebugLibraries", Conf=="Debug"?"true":"false");
                    AddSubNodeWithInnerText(sub, "PlatformToolset", "Clang_3_8");
                    AddSubNodeWithInnerText(sub, "AndroidAPILevel", AndroidAPILevel);
                }
            );

            //<ImportGroup Label="PropertySheets" Condition="'$(Configuration)|$(Platform)'=='Debug|x86'">
            temp = FindSubXmlElement2(rawProjectRoot, "ImportGroup", "Condition", rCondition, "Label", "PropertySheets");
            if(temp!=null)
            {
                string _path = System.IO.Path.GetDirectoryName(m_xmlFilePath);
                Add_Group_PlatConf(
                    delegate (string Plat, string Conf, string Condition)
                    {
                        //clone 
                        XmlElement clone = newDoc.ImportNode(temp, true) as XmlElement;
                        clone.SetAttribute("Condition", Condition);
                        foreach (XmlNode node in clone.ChildNodes)
                        {
                            if (node.Name == "Import")
                            {
                                string vv = node.Attributes["Project"].Value;
                                if(!vv.EndsWith(".user.props") && vv.EndsWith(".props"))
                                {
                                    string new_vv = vv.Replace(".props", "_a.props");
                                    string oldName = _path +"\\" +vv;
                                    if (File.Exists(oldName))
                                    {
                                        File.Copy(oldName, _path + "\\" + new_vv, true);
                                        (node as XmlElement).SetAttribute("Project", new_vv);
                                    }
                                }
                            }
                        }
                        newRoot.AppendChild(clone);
                    }
                );
            }


            Project_add_Node("Import", "Project", "$(VCTargetsPath)\\Microsoft.Cpp.props");
            Project_add_Node("ImportGroup", "Label", "ExtensionSettings");
            Project_add_Node("ImportGroup", "Label", "Shared");
            Project_add_Node("ImportGroup", "Label", "PropertySheets");
            Project_add_Node("PropertyGroup", "Label", "UserMacros");

            Add_Group_PlatConf(
                delegate (string Plat, string Conf, string Condition)
                {
                    XmlElement sub = Project_add_Node("PropertyGroup", "Condition", Condition);
                    AddSubNodeWithInnerText(sub, "UseMultiToolTask", "true");
                }
            );

            Project_add_Node("PropertyGroup", "Label", "UserMacros");

            ///////////////配置编译//////////
            //string rCondition = "'$(Configuration)|$(Platform)'=='Debug|Win32'";
            XmlNode rrr = FindSubXmlElement(rawProjectRoot, "ItemDefinitionGroup", "Condition", rCondition);
            XmlNode rClCompile = FindSubXmlElement(rrr,"ClCompile","","");

            string AdditionalIncludeDirectories = GetSubNodeInnerText(rClCompile,"AdditionalIncludeDirectories");
            string []sa = AdditionalIncludeDirectories.Split(new char[] { ';' });
            if(sa!=null && sa.Length>0)
            {
                int changed = 0;
                for(int i = 0;i < sa.Length;++i)
                {
                    //$(MSBuildProgramFiles32)\Microsoft SDKs\Windows\v7.1A\include
                    if (sa[i].Contains("\\Microsoft SDKs\\Windows\\"))
                    {
                        sa[i] = ".;";
                        changed++;
                    }
                    else if (sa[i].EndsWith("\\win32"))
                    {
                        sa[i] = sa[i].Substring(0, sa[i].Length - 5) + "android";
                        changed++;
                    }                        
                }
                if(changed>0)
                    AdditionalIncludeDirectories = string.Join(";", sa);

            }

            //AdditionalIncludeDirectories.Replace("\\", "/");
            string PreprocessorDefinitions = GetSubNodeInnerText(rClCompile, "PreprocessorDefinitions");

            PreprocessorDefinitions = PreprocessorDefinitions.Replace("_WIN32;", "");
            PreprocessorDefinitions = PreprocessorDefinitions.Replace("_WINDOWS;", "");
            PreprocessorDefinitions = PreprocessorDefinitions.Replace("WIN32;", "");
            PreprocessorDefinitions = PreprocessorDefinitions.Replace("D3D_DEBUG_INFO;", "");

            PreprocessorDefinitions = pre_PreprocessorDefinitions + PreprocessorDefinitions;

            Add_Group_PlatConf(
                delegate (string Plat, string Conf, string Condition)
                {
                    XmlElement sub = Project_add_Node("ItemDefinitionGroup", "Condition", Condition);
                    XmlElement ClCompile = newDoc.CreateElement("ClCompile");
                    sub.AppendChild(ClCompile);
                    AddSubNodeWithInnerText(ClCompile, "PrecompiledHeader", "NotUsing");//Use
                    //AddSubNodeWithInnerText(ClCompile, "PrecompiledHeaderFile", "");//pch.h
                    AddSubNodeWithInnerText(ClCompile, "AdditionalIncludeDirectories", AdditionalIncludeDirectories);
                    if(Conf == "Release")
                        PreprocessorDefinitions = PreprocessorDefinitions.Replace("_DEBUG;", "");
                    AddSubNodeWithInnerText(ClCompile, "PreprocessorDefinitions", PreprocessorDefinitions);
                    AddSubNodeWithInnerText(ClCompile, "AdditionalOptions", AdditionalOptions);
                    AddSubNodeWithInnerText(ClCompile, "MultiProcessorCompilation", "true");
                    AddSubNodeWithInnerText(ClCompile, "CppLanguageStandard", "gnu++11");//c++11
                    AddSubNodeWithInnerText(ClCompile, "RuntimeTypeInfo", "true");
                    AddSubNodeWithInnerText(ClCompile, "ExceptionHandling", "Enabled");

                    //如果是bin?
                    XmlElement Link = newDoc.CreateElement("Link");
                    sub.AppendChild(Link);
                    AddSubNodeWithInnerText(Link, "LibraryDependencies", "m;z;%(LibraryDependencies)");
                    AddSubNodeWithInnerText(Link, "IncrementalLink", "true");
                    AddSubNodeWithInnerText(Link, "Relocation", "false");
                }
            );

            Project_add_Node("Import", "Project", "$(VCTargetsPath)\\Microsoft.Cpp.targets");
            Project_add_Node("ImportGroup", "Label", "ExtensionTargets");

            //add所有的ItemGroup 
            XmlNodeList subs = rawProjectRoot.ChildNodes;// ("ItemGroup");
            for (int i = 0; i < subs.Count; ++i)
            {
                XmlElement sub = subs[i] as XmlElement;
                if (sub.Name != "ItemGroup")
                    continue;

                var kk = sub.Attributes["Label"];//"Label"
                if (kk != null)
                    continue;

                //删除xmlns:
                //xmlns="http://schemas.microsoft.com/developer/msbuild/2003
                XmlElement clone = newDoc.ImportNode(sub, true) as XmlElement;
                clone.RemoveAllAttributes();
                //clone.RemoveAttribute("xmlns", clone.NamespaceURI);
                newRoot.AppendChild(clone);
            }
            Project_add_Node("ImportGroup", "Label", "ExtensionTargets");
        }

        //special add group:
        public void Add_PropertyGroup_Globals(string Guid)
        {
            XmlNode rrr = FindSubXmlElement(rawProjectRoot, "PropertyGroup", "Label", "Globals");
            string ProjectGuid = GetSubNodeInnerText(rrr,"ProjectGuid");
            string RootNamespace = GetSubNodeInnerText(rrr,"RootNamespace");

            XmlElement ItemGroup = Project_add_Node("PropertyGroup", "Label", "Globals");
            AddSubNodeWithInnerText(ItemGroup, "ProjectGuid", ProjectGuid);
            AddSubNodeWithInnerText(ItemGroup, "Keyword", "Android");
            AddSubNodeWithInnerText(ItemGroup, "RootNamespace", RootNamespace);
            AddSubNodeWithInnerText(ItemGroup, "MinimumVisualStudioVersion", "14.0");
            AddSubNodeWithInnerText(ItemGroup, "ApplicationType", "Android");
            AddSubNodeWithInnerText(ItemGroup, "ApplicationTypeRevision", "3.0");
        }


        static string GetSubNodeInnerText(XmlNode rrr,string sub_name)
        {
            if (rrr == null)
                return "";

            XmlNodeList subs = rrr.ChildNodes;// ("/"+Project.Name+"/"+PropertyGroup);
            for (int i = 0; i < subs.Count; ++i)
            {
                if (subs[i].Name == sub_name)
                {
                    return subs[i].InnerText;
                }
            }
            return ""; 
        }

        //查找子节点: 
        static XmlNode FindSubXmlElement(XmlNode parentNode, string PropertyGroup, string key, string value)
        {
            if (parentNode == null)
                return null;

            // ("/"+Project.Name+"/"+PropertyGroup);
            foreach(XmlNode node in parentNode.ChildNodes)
            {
                if (node.Name == PropertyGroup)
                {
                    bool r1 = CheckAttr(node, key, value);
                    if (r1 )
                        return node;
                }
            }
            return null;
        }
        //查找满足条件的
        static XmlNode FindSubXmlElement2(XmlNode Project, string PropertyGroup, string key, string value, string key2, string value2)
        {
            foreach (XmlNode node in Project.ChildNodes)
            {
                if (node.Name == PropertyGroup)
                {
                    bool r1 = CheckAttr(node, key, value);
                    bool r2 = CheckAttr(node, key2, value2);
                    if (r1 && r2) return node;
                }
            }
            return null;
        }
        static bool CheckAttr(XmlNode node, string key, string value)
        {
            if (String.IsNullOrEmpty(key))
            {
                return true;
            }
            var tt = node.Attributes[key];//"Label"
            return (tt != null && tt.Value == value);
        }
        //public void AddAttribute(XmlNode node ,string key ,string value)
        //{
        //    node.SetAttribute(key, value);
        //}

        //增加属性节点: <Configuration>Debug</Configuration>
        public void AddSubNodeWithInnerText(XmlNode node, string key, string value)
        {
            XmlNode att = newDoc.CreateElement(key);
            att.InnerText = value;
            //xml节点附件属性
            node.AppendChild(att);
        }

        public void AddSubNode(XmlNode node, XmlNode sub_node)
        {
            node.AppendChild(sub_node);
        }


        public XmlElement Project_add_Node(string ele, string prop_key, string prop_value)
        {
            XmlElement sub = newDoc.CreateElement(ele);
            if (prop_key.Length > 0)
                sub.SetAttribute(prop_key, prop_value);
            newRoot.AppendChild(sub);
            return sub;
        }

        /*
            <ProjectConfiguration Include="Debug|ARM">
                  <Configuration>Debug</Configuration>
                  <Platform>ARM</Platform>
            </ProjectConfiguration> 
         */
        XmlNode CreateProjectConfiguration(string Configuration, string Platform)
        {
            XmlElement res = newDoc.CreateElement("ProjectConfiguration");
            res.SetAttribute("Include", Configuration + "|" + Platform);
            AddSubNodeWithInnerText(res, "Configuration", Configuration);
            AddSubNodeWithInnerText(res, "Platform", Platform);
            return res;
        }

        string[] _Confs = { "Debug" };//, "Release"
        string[] _Plats = { "x86" };//"ARM", "ARM64", "x64", 
        void Add_Group_PlatConf(Action<string, string, string> action)
        {
            for (int i = 0; i < _Plats.Length; ++i)
            {
                for (int j = 0; j < _Confs.Length; ++j)
                {
                    string Condition = string.Format("'$(Configuration)|$(Platform)'=='{0}|{1}'", _Confs[j], _Plats[i]);
                    action(_Plats[i], _Confs[j], Condition);
                }
            }
        }

        /// <summary>
        /// /////////////////////////////////////////////////////////////////////
        /// </summary>
        XmlDocument doc = new XmlDocument();
        XmlNode rawProjectRoot = null;
        string m_xmlFilePath = "";
        public void CreateAndroid(string xmlFilePath)
        {
            m_xmlFilePath = xmlFilePath;
            if (String.IsNullOrEmpty(xmlFilePath))
            {
                Console.WriteLine("CreateAndroid: xmlFilePath = "+ xmlFilePath);
                return;
            }
            doc.Load(xmlFilePath);
            rawProjectRoot = doc.DocumentElement;

            InitNew();

            string newFileName = xmlFilePath.Replace(".vcxproj", proj_android);
            Save(newFileName);


            File.Copy(xmlFilePath + ".filters", newFileName + ".filters",true);
        }
        static string proj_android = "_a.vcxproj";
        public void Save(string fn)
        {
            RegexOptions options = System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.Multiline;
            //去掉注释 和命名空间
            string newXml = Regex.Replace(newDoc.OuterXml, @"(xmlns:?[^=]*=[""][^""]*[""])", "", options);
            //File.WriteAllText(fn, newXml); //fail: will eat all /r/n
            ////newDoc.RemoveAll();
            //<ProjectReference Include="..\plat\plat.vcxproj">
            if(newXml.Contains("<ProjectReference Include=\""))
            {
                newXml = newXml.Replace(".vcxproj", proj_android); //Regex.Replace(newXml, "\<ProjectReference Include\=\"()", "", options);
            }

            newDoc.LoadXml(newXml);
            newDoc.Save(fn);
            Console.WriteLine("save:" + fn);
        }

        //fail:
        //public static void Save2(string fn, XmlDocument obj)
        //{
        //    XmlSerializer serializer = new XmlSerializer(obj.GetType());

        //    // 将对象序列化输出到文件
        //    FileStream stream = new FileStream(fn, FileMode.Create);

        //    XmlWriterSettings settings = new XmlWriterSettings();
        //    settings.Indent = true;
        //    settings.IndentChars = "    ";
        //    settings.NewLineChars = "\r\n";
        //    settings.Encoding = Encoding.UTF8;
        //    //settings.OmitXmlDeclaration = true;  // 不生成声明头

        //    using (XmlWriter xmlWriter = XmlWriter.Create(stream, settings))
        //    {
        //        // 强制指定命名空间，覆盖默认的命名空间
        //        XmlSerializerNamespaces namespaces = new XmlSerializerNamespaces();
        //        namespaces.Add(string.Empty, string.Empty);
        //        serializer.Serialize(xmlWriter, obj, namespaces);
        //        xmlWriter.Close();
        //    };
        //    stream.Close();
        //}
    }//end class 
}//end NS
