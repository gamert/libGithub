using System;
using System.Xml;

namespace Helper
{
    public class CXmlVcxproj
    {
        XmlDocument doc = new XmlDocument();
        XmlNode Project = null;
        public void CreateAndroid(string xmlFilePath)
        {
            doc.Load(xmlFilePath);
            Project = doc.DocumentElement;

            InitNew();

            Save(xmlFilePath+".new");
        }


        public XmlDocument xmlDoc = new XmlDocument();
        public XmlElement rootNode;

        public XmlElement PropertyGroup_Globals;

        //根据旧的工程创建一个新的工程文件...
        public void InitNew()
        {
            //创建Xml声明部分，即<?xml
            xmlDoc.CreateXmlDeclaration("1.0", "utf-8", "yes");
            //创建根节点
            rootNode = xmlDoc.CreateElement("Project");
            xmlDoc.AppendChild(xmlDoc);

            //rootNode
            rootNode.SetAttribute("DefaultTargets", "Build");
            rootNode.SetAttribute("ToolsVersion", "15.0");
            rootNode.SetAttribute("xmlns", "http://schemas.microsoft.com/developer/msbuild/2003");

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

            Add_Group_PlatConf(
                delegate (string Plat, string Conf, string Condition)
                {
                    XmlElement sub = Project_add_Node("PropertyGroup", "Condition", Condition);
                    sub.SetAttribute("Label", "Configuration");
                    AddSubNodeWithInnerText(sub, "ConfigurationType", "DynamicLibrary");
                    AddSubNodeWithInnerText(sub, "UseDebugLibraries", "true");
                    AddSubNodeWithInnerText(sub, "PlatformToolset", "Clang_3_8");
                }
            );

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
            string rCondition = "'$(Configuration)|$(Platform)'=='Debug|Win32'";
            XmlNode rrr = FindSubXmlElement(Project, "ItemDefinitionGroup", "Condition", rCondition);
            XmlNode rClCompile = FindSubXmlElement(rrr,"ClCompile","","");

            string AdditionalIncludeDirectories = GetSubNodeInnerText(rClCompile,"AdditionalIncludeDirectories");
            AdditionalIncludeDirectories.Replace("\\", "/");
            string PreprocessorDefinitions = GetSubNodeInnerText(rClCompile, "PreprocessorDefinitions");
            PreprocessorDefinitions = PreprocessorDefinitions.Replace("_WIN32;", "_LINUX;");
            PreprocessorDefinitions = PreprocessorDefinitions.Replace("_WINDOWS;", "ANDROID;");
            PreprocessorDefinitions = PreprocessorDefinitions.Replace("WIN32;", "");

            Add_Group_PlatConf(
                delegate (string Plat, string Conf, string Condition)
                {
                    XmlElement sub = Project_add_Node("ItemDefinitionGroup", "Condition", Condition);
                    XmlElement ClCompile = xmlDoc.CreateElement("ClCompile");
                    sub.AppendChild(ClCompile);
                    AddSubNodeWithInnerText(ClCompile, "PrecompiledHeader", "Use");
                    AddSubNodeWithInnerText(ClCompile, "PrecompiledHeaderFile", "pch.h");
                    AddSubNodeWithInnerText(ClCompile, "AdditionalIncludeDirectories", AdditionalIncludeDirectories);
                    AddSubNodeWithInnerText(ClCompile, "PreprocessorDefinitions", "_LINUX;ANDROID;USE_GL");
                    AddSubNodeWithInnerText(ClCompile, "AdditionalOptions", "-Wunused-private-field -std=gnu++11 -Wmultichar %(AdditionalOptions)");
                    AddSubNodeWithInnerText(ClCompile, "MultiProcessorCompilation", "true");
                    XmlElement Link = xmlDoc.CreateElement("Link");
                    sub.AppendChild(Link);
                    AddSubNodeWithInnerText(Link, "LibraryDependencies", "m;z;%(LibraryDependencies)");
                }
            );

            Project_add_Node("Import", "Project", "$(VCTargetsPath)\\Microsoft.Cpp.targets");
            Project_add_Node("ImportGroup", "Label", "ExtensionTargets");

            //add所有的ItemGroup 
            XmlNodeList subs = Project.ChildNodes;// ("ItemGroup");
            for (int i = 0; i < subs.Count; ++i)
            {
                XmlElement sub = subs[i] as XmlElement;
                if (sub.Name != "ItemGroup")
                    continue;

                var kk = sub.Attributes["Label"];//"Label"
                if (kk != null)
                    continue;

                //XmlNode clone = sub.CloneNode(true);
                rootNode.AppendChild(xmlDoc.ImportNode(sub, true));
            }
            Project_add_Node("ImportGroup", "Label", "ExtensionTargets");
        }
        public void Add_PropertyGroup_Globals(string Guid)
        {
            XmlNode rrr = FindSubXmlElement(Project, "PropertyGroup", "Label", "Globals");
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
            XmlNodeList subs = rrr.ChildNodes;// ("/"+Project.Name+"/"+PropertyGroup);
            for (int i = 0; i < subs.Count; ++i)
            {
                if (subs[i].Name == sub_name)
                {
                    return subs[i].InnerText;
                }
            }
            return null;
        }

        //查找子节点:
        XmlNode FindSubXmlElement(XmlNode Project, string PropertyGroup, string key, string Label)
        {
            XmlNodeList subs = Project.ChildNodes;// ("/"+Project.Name+"/"+PropertyGroup);
            for (int i = 0; i < subs.Count; ++i)
            {
                if (subs[i].Name == PropertyGroup)
                {
                    if (key == "")
                        return subs[i];
                    var kk = subs[i].Attributes[key];//"Label"
                    if (kk != null && kk.Value == Label)
                        return subs[i];
                }
            }
            return null;
        }

        //public void AddAttribute(XmlNode node ,string key ,string value)
        //{
        //    node.SetAttribute(key, value);
        //}

        //增加属性节点: <Configuration>Debug</Configuration>
        public void AddSubNodeWithInnerText(XmlNode node, string key, string value)
        {
            XmlNode att = xmlDoc.CreateElement(key);
            att.InnerText = value;
            //xml节点附件属性
            node.AppendChild(att);
        }

        public void AddSubNode(XmlNode node, XmlNode sub_node)
        {
            node.AppendChild(sub_node);
        }

        public void Save(string fn)
        {
            xmlDoc.Save(fn);
            Console.WriteLine("save:" + fn);
        }
        public XmlElement Project_add_Node(string ele, string prop_key, string prop_value)
        {
            XmlElement sub = xmlDoc.CreateElement("ItemGroup");
            if (prop_key.Length > 0)
                sub.SetAttribute(prop_key, prop_value);
            rootNode.AppendChild(sub);
            return sub;
        }


        string[] _Confs = { "Debug", "Release" };
        string[] _Plats = { "ARM", "ARM64", "x64", "x86" };



        XmlNode CreateProjectConfiguration(string Configuration, string Platform)
        {
            XmlNode res = xmlDoc.CreateElement("ProjectConfiguration");
            AddSubNodeWithInnerText(res, "Include", Configuration + "|" + Platform);
            AddSubNodeWithInnerText(res, "Configuration", Configuration == "Debug" ? "true" : "false");
            AddSubNodeWithInnerText(res, "Platform", Platform);
            return res;
        }

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
    }
}
