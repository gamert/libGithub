using cn.crashByNull;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

public class MonoTool
{
    static Regex rgx_11500000 = new Regex(@"m_Script\: \{fileID\: 11500000\, guid\: ([a-zA-Z0-9]+)\, type\: ([0-9]+)\}");

    [MenuItem("tool/Test_ReplaceMissScript1")]
    static void Test_ReplaceMissScript1()
    {
        m_UIBaseDLL.LoadDomainDllTypes("UIBase");
#if false
        string tt = "m_Script: {fileID: 11500000, guid: 7daa5a1ec3f91374e8d2866dc730e1ef, type: 3}";
        //rgx_11500000.IsMatch()
        Match m = rgx_11500000.Match(tt);
        if(m!=null && m.Success)
        {
            for(int i = 0;i < m.Groups.Count;++i)
            {
                Debug.Log(m.Groups[i]); //0 表示自身；1表示第一个括号；2表示第二个括号
            }
            //m.Groups.Count
        }
#endif
    }

    //1. 首先遍历dst工程所有的meta，找到A = {uuid}；
    //2. 遍历ref工程所有的meta，找到B = {uuid} 
    //3. 对每个dst中的prefab进行测试，找到所有丢失的fileid，然后在B中找到，并通过B确认Type和路径；
    //4. 将丢失的fileid，更换为新的uuid， md4(DLL+type)
    //https://blog.csdn.net/gz_huangzl/article/details/52486509
    [MenuItem("tool/ReplaceMissScript")]
    public static void ReplaceMissScript()
    {
        if (!m_UIBaseDLL.IsOk())
            m_UIBaseDLL.LoadDomainDllTypes("UIBase");

        if (m_refMetas.Count == 0) ;
        CreateMetaDic("G:/Aotu/worksapce100/Client3/Assets", m_refMetas);

        string proj_path = "G:/Aotu/worksapce100/Client2/Assets";
        m_dstMetas.Clear();
        CreateMetaDic(proj_path, m_dstMetas);

        ReplaceMissScriptInFile(proj_path + "/Resources/Exporter/UI/Battle/Prefabs/UGUI_ListForm2D.prefab");

        //处理所有的prefab
        //string[] ss = Directory.GetFiles(proj_path, "*.prefab", SearchOption.AllDirectories);
        //for (int i = 0; i < ss.Length; ++i)
        //{
        //    ReplaceMissScriptInFile(ss[i]);
        //    //m_Script: {fileID: 11500000, guid: 7daa5a1ec3f91374e8d2866dc730e1ef, type: 3}
        //}
        Debug.Log("The end~");
    }
    static DllMonoBehavior m_UIBaseDLL = new DllMonoBehavior();


    //查找行
    static int TestAndFindMissInLine(int lineNo,string sLine,out string sNewLine)
    {
        sNewLine = "";

        Match m = rgx_11500000.Match(sLine);
        if (m != null && m.Success)
        {
            Meta_t v;
            if (m_dstMetas.TryGetValue(m.Groups[1].ToString(), out v))
            {

            }
            else
            {
                Meta_t v2;
                if (m_refMetas.TryGetValue(m.Groups[1].ToString(), out v2))
                {
                    //如果在ref中找到，那么取得名字
                    //16007找到Ref = G:/Aotu/worksapce100/Client3/Assets\Src\BattleCore\UI\UGUIImage2.cs ,  m_Script: {fileID: 11500000, guid: 00c5cc3b6a3258848869cb04912bbefc, type: 3}
                    //那么新的名字就是：
                    int pos = v2.fn.LastIndexOf("\\");
                    string fn = v2.fn.Substring(pos+1);
                    int pos2 = fn.LastIndexOf(".");
                    fn = fn.Substring(0, pos2);

                    Type t = m_UIBaseDLL.Find(fn);
                    if (t!=null)
                    {
                        Debug.Log(string.Format("[{0}]找到Ref={1},{2},{3}", lineNo, t, v2.fn, sLine));
                        int fileid = FileIDUtil.Compute(t);
                        string fid = fileid.ToString();
                        sNewLine = "  m_Script: {fileID: " + fid+", guid: "+ m_UIBaseDLL.m_meta.uuid+", type: 3}";
                        return 1;
                    }
                    else
                    {
                        Debug.LogWarning(string.Format("[{0}] 未找到Ref={1},{2},{3}", lineNo, t, v2.fn, sLine));
                    }
                }
                else
                {
                    Debug.LogError(string.Format("[{0}] 未找到Ref={1}", lineNo, sLine));
                }
            }
        }
        return 0;
    }

    //判断各种脚本引用的包含情况
    static void ReplaceMissScriptInFile(string assetfn)
    {
        int changed = 0;
        string[] ll = File.ReadAllLines(assetfn);
        for (int i = 0; i < ll.Length; ++i)
        {
            int pos = ll[i].IndexOf("m_Script: ");
            if (pos != -1)
            {
                string sNewLine;
                int res =TestAndFindMissInLine(i, ll[i],out sNewLine);
                if(res>0)
                {
                    ll[i] = sNewLine;
                    changed += res;
                }
            }
        }
        if(changed >0 )
        {
            Debug.Log(assetfn+"  引用Miss个数="+ changed);
            File.WriteAllLines(assetfn, ll);
        }
    }

    //key为uuid？
    static Dictionary<string, Meta_t> m_dstMetas = new Dictionary<string, Meta_t>();
    static Dictionary<string, Meta_t> m_refMetas = new Dictionary<string, Meta_t>();

    //增加到dic中
    static void HandleFileMetaToDic(string metafile, Dictionary<string, Meta_t> dicMetas)
    {
        Meta_t v = Meta_t.FromFile(metafile);
        if(v!=null)
            dicMetas.Add(v.uuid,v);
    }
    static void CreateMetaDic(string proj_path, Dictionary<string, Meta_t> dicMetas)
    {
        string[] ss = Directory.GetFiles(proj_path, "*.meta", SearchOption.AllDirectories);
        for (int i = 0; i < ss.Length; ++i)
        {
            HandleFileMetaToDic(ss[i], dicMetas);
        }
        Debug.Log(string.Format("****创建Meta字典****： {0}处理完毕，count={1}", proj_path, dicMetas.Count));
    }

    //参数1 为要查找的总路径， 参数2 保存路径  
    private static void GetDirs(string dirPath, ref List<string> dirs)
    {
        foreach (string path in Directory.GetFiles(dirPath))
        {
            //获取所有文件夹中包含后缀为 .prefab 的路径  
            if (System.IO.Path.GetExtension(path) == ".prefab")
            {
                dirs.Add(path.Substring(path.IndexOf("Assets")));
                //Debug.Log(path.Substring(path.IndexOf("Assets")));
            }
        }

        if (Directory.GetDirectories(dirPath).Length > 0)  //遍历所有文件夹  
        {
            foreach (string path in Directory.GetDirectories(dirPath))
            {
                GetDirs(path, ref dirs);
            }
        }
    }
}


//保存所有的meta信息
public class Meta_t
{
    public string fn;
    public string uuid;

    public static Meta_t FromFile(string metafn)
    {
        //目录的话，就不要处理了
        string reffile = metafn.Substring(0, metafn.LastIndexOf(".meta"));
        if (Directory.Exists(reffile))
        {
            return null;
        }
        //string key = metafile.Substring(metafile.IndexOf("Assets"));
        Meta_t v = new Meta_t();
        v.fn = reffile;
        //fileFormatVersion: 2
        //guid: 7d6e24a13990b6549b206d11106e445a
        string[] ll = File.ReadAllLines(metafn);
        for (int i = 0; i < ll.Length; ++i)
        {
            int pos = ll[i].IndexOf("guid: ");
            if (pos != -1)
            {
                v.uuid = ll[i].Substring(6);
                break;
            }
        }
        return v;
    }
}


//管理Dll中的Mono
public class DllMonoBehavior
{
    public Meta_t  m_meta;
    Assembly candi; //
    Dictionary<string, Type> typeList = new Dictionary<string, Type>();
    public bool IsOk()
    {
        return candi!=null && m_meta!=null;
    }
    public Type Find(string typeName)
    {
        Type t = null;
        typeList.TryGetValue(typeName, out t);
        return t;
    }
    //读取本Domain 中DLL中的所有类型
    public void LoadDomainDllTypes(string dllName)
    {
        Clear();

        AppDomain currentDomain = AppDomain.CurrentDomain;
        Assembly[] assemblyInThisDomain = currentDomain.GetAssemblies();
        foreach (Assembly assembly in assemblyInThisDomain)
        {
            //UnityEngine.Debug.Log(assembly.GetName().Name); //TODO: 处理xGame 和 CSharp_Assembly.dll
            if (assembly != null)
            {
                string name = assembly.GetName().Name;
                if (name == dllName)
                {
                    candi = assembly;
                    break;
                }
            }
        }
        if (candi != null)
        {
            //candi.FullName
            // "G:\Aotu\worksapce100\Client2\UIBase, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null.meta".
            m_meta = Meta_t.FromFile(candi.Location + ".meta");

            Type[] types = candi.GetTypes();
            SafeAdd(types);
        }
    }
    void Clear()
    {
        candi = null;
        typeList.Clear();
    }

    void SafeAdd(Type[] types)
    {
        for (int i = 0; i < types.Length; ++i)
        {
            Type type = types[i];
            //"******0:<>c__DisplayClass1" ******0:$ArrayType$60
            //UnityEngine.Debug.Log("******" + count + ":" + type.Name); count++;
            //if(type==null)
            //{
            //    return;
            //}
            //if (type.Name.Length == 0)
            //{
            //    return;
            //}
            int cc = type.Name[0];
            if (cc != '<' && cc != '$' && cc != '_' && IsMonoBehavior(type) && !typeList.ContainsKey(type.Name))
            {
                typeList.Add(type.Name, type);
            }
        }
    }
    Type type_mono = typeof(MonoBehaviour);//Type.GetType("UnityEngine.MonoBehaviour");
    bool IsMonoBehavior(Type type)
    {
        if (type.IsSubclassOf(type_mono))
        {
            return true;
        }
        return false;
    }
}

