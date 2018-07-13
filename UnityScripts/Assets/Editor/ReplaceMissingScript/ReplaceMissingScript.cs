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
using Tool.RMS;



public class ReplaceMissingScript
{
    //private static readonly string[] fileListPath = { "*.meta", "*.mat", "*.anim", "*.prefab", "*.unity", "*.asset" };

    static Regex rgx_11500000 = new Regex(@"m_Script\: \{fileID\: 11500000\, guid\: ([a-zA-Z0-9]+)\, type\: ([0-9]+)\}");
    static DllMonoBehavior m_UIBaseDLL = new DllMonoBehavior();

    //key为uuid？
    static CDicMeta m_dstMetas = new CDicMeta();
    static CDicMeta m_refMetas = new CDicMeta();

    [MenuItem("tool/Test_ReplaceMissScript1")]
    static void Test_ReplaceMissScript1()
    {
        m_UIBaseDLL.LoadDomainDllTypes("xGame");
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

    ////1. 首先遍历dst工程所有的meta，找到A = {uuid}；
    ////2. 遍历ref工程所有的meta，找到B = {uuid} 
    ////3. 对每个dst中的prefab进行测试，找到所有丢失的fileid，然后在B中找到，并通过B确认Type和路径；
    ////4. 将丢失的fileid，更换为新的uuid， md4(DLL+type)
    ////https://blog.csdn.net/gz_huangzl/article/details/52486509
    //[MenuItem("tool/ReplaceMissScript")]
    //public static void ReplaceMissScript()
    //{
    //    //if (!m_UIBaseDLL.IsOk())
    //    //    m_UIBaseDLL.LoadDomainDllTypes("xGame");

    //    //if (m_UIBaseDLL.m_meta == null)
    //    //    return;

    //    ////if (m_refMetas.Count == 0) 
    //    ////CreateMetaDic("G:/Moba1/workspace_602/GameRes/Assets", m_refMetas);

    //    //string proj_path = "G:/Moba1/workspace_602/GameRes - Android/Assets";
    //    //if(m_dstMetas.Count == 0)
    //    //    //m_dstMetas.Clear();
    //    //    CDicMeta.CreateMetaDic(proj_path, m_dstMetas,"*.cs");

    //    ////ReplaceMissScriptInFile(proj_path + "/Resources/Exporter/UI/Battle/Prefabs/UGUI_ListForm2D.prefab");

    //    ////替换DLL引用
    //    ////ReplaceScriptRefDLLInFile(proj_path + "/Resources/Exporter/UI/Battle/Prefabs/UGUI_ListForm2D.prefab");
    //    //string ff = "G:/Moba1/workspace_602/GameRes/Assets/Export/Effect/202/202_3_3.prefab";
    //    //ReplaceScriptRefDLLInFile(ff);

    //    ////处理所有的prefab
    //    ////string[] ss = Directory.GetFiles(proj_path, "*.prefab", SearchOption.AllDirectories);
    //    ////for (int i = 0; i < ss.Length; ++i)
    //    ////{
    //    ////    ReplaceMissScriptInFile(ss[i]);
    //    ////    //m_Script: {fileID: 11500000, guid: 7daa5a1ec3f91374e8d2866dc730e1ef, type: 3}
    //    ////}
    //    //Debug.Log("The end~");
    //}


    //script -->dll
    static int TestAndFindMissInLine2(int lineNo, string sLine, out string sNewLine)
    {
        //匹配DLL引用
        //baa3e98256fcd8542a6a5ba1a0290fa5
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
                    string fn = v2.fn.Substring(pos + 1);
                    int pos2 = fn.LastIndexOf(".");
                    fn = fn.Substring(0, pos2);

                    Type t = m_UIBaseDLL.Find(fn);
                    if (t != null)
                    {
                        Debug.Log(string.Format("[{0}]找到Ref={1},{2},{3}", lineNo, t, v2.fn, sLine));
                        int fileid = FileIDUtil.Compute(t);
                        string fid = fileid.ToString();
                        sNewLine = "  m_Script: {fileID: " + fid + ", guid: " + m_UIBaseDLL.m_meta.uuid + ", type: 3}";
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



}





