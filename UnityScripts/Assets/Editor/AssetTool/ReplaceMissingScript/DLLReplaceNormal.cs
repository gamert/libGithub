using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace AssetTool.RMS
{
    //DLL 替换普通:支持从xGame和Assembly-CSharp中替换:
    public class DLLReplaceNormal
    {
        Regex rgx_11500000 = new Regex(@"m_Script\: \{fileID\: 11500000\, guid\: ([a-zA-Z0-9]+)\, type\: ([0-9]+)\}");
        DllMonoBehavior _dll = new DllMonoBehavior();
        DllMonoBehavior _dllBack = new DllMonoBehavior();

        CDicMeta m_dstMetas = new CDicMeta();
        CDicMeta m_refMetas = new CDicMeta();

        int missrow = 0;
        int replacerow = 0;

        //1. 首先遍历dst工程所有的meta，找到A = {uuid}；
        //2. 遍历ref工程所有的meta，找到B = {uuid} 
        //3. 对每个dst中的prefab进行测试，找到所有丢失的fileid，然后在B中找到，并通过B确认Type和路径；
        //4. 将丢失的fileid，更换为新的uuid， md4(DLL+type)
        //https://blog.csdn.net/gz_huangzl/article/details/52486509
        //[MenuItem("tool/ReplaceMissScript")]
        public static void ReplaceMissScript()
        {
            DLLReplaceNormal dr = new DLLReplaceNormal();
            dr.Run("xGame", "G:/Moba1/workspace_602/GameRes - Android/Assets");

            EditorUtility.ClearProgressBar();
        }

        //"xGame"
        // proj_path: 目标工程路径，如 "G:/Moba1/workspace_602/GameRes - Android/Assets";
        // 注意: 必须把dll里面的文件先挪到该工程路径下，同时，去除dll
        public void Run(string dll, string proj_path)
        {
            if (!_dll.IsOk())
                _dll.LoadDomainDllTypes(dll);
            if (!_dllBack.IsOk())
                _dllBack.LoadDomainDllTypes("Assembly-CSharp");

            if (_dll.m_meta == null)
            {
                Debug.LogWarning(string.Format("[{0}] 未找到 m_meta", dll));
                return;
            }

            //初始化...
            string patten = string.Format(@"m_Script: {{fileID: ([0-9\-]+), guid: {0}, type: ([0-9]+)}}", _dll.m_meta.uuid);
            rgx_11500000 = new Regex(patten);

            //if (m_refMetas.Count == 0) 
            //CreateMetaDic("G:/Moba1/workspace_602/GameRes/Assets", m_refMetas);

            if (m_dstMetas.Count == 0)
                //m_dstMetas.Clear();
                CDicMeta.CreateMetaDic(proj_path, m_dstMetas, "*.cs");

            //ReplaceMissScriptInFile(proj_path + "/Resources/Exporter/UI/Battle/Prefabs/UGUI_ListForm2D.prefab");

            //替换DLL引用
            //ReplaceScriptRefDLLInFile(proj_path + "/Resources/Exporter/UI/Battle/Prefabs/UGUI_ListForm2D.prefab");
            //string ff = "G:/Moba1/workspace_602/GameRes/Assets/Export/Effect/202/202_3_3.prefab";

            //string ff = "G:/Moba1/workspace_602/GameRes/Assets/Export/Hero/Animations/212hero_0/jiansheng_Low.prefab";
            //ReplaceMissScriptInFile(ff);
            //处理所有的prefab
            ReplaceProjectMissScriptInFile(proj_path, "*.prefab");
            ReplaceProjectMissScriptInFile(proj_path, "*.asset");

            Debug.Log("The end~");
        }


        //m_Script: {fileID: 11500000, guid: 7daa5a1ec3f91374e8d2866dc730e1ef, type: 3}
        void ReplaceProjectMissScriptInFile(string proj_path, string filte)
        {
            string[] ss = Directory.GetFiles(proj_path, filte, SearchOption.AllDirectories);
            for (int i = 0; i < ss.Length; ++i)
            {
                float progress = (float)i / ss.Length;
                EditorUtility.DisplayProgressBar("ReplaceMissScriptInFile...", ss[i], progress);
                ReplaceMissScriptInFile(ss[i]);
            }
        }
        string _curFile = "";
        //判断各种脚本引用的包含情况
        void ReplaceMissScriptInFile(string assetfn)
        {
            _curFile = assetfn;

            int changed = 0;
            string[] ll = File.ReadAllLines(assetfn);
            for (int i = 0; i < ll.Length; ++i)
            {
                int pos = ll[i].IndexOf("m_Script: ");
                if (pos != -1)
                {
                    string sNewLine;
                    int res = TestAndFindMissInLine(i, ll[i], out sNewLine);
                    if (res > 0)
                    {
                        ll[i] = sNewLine;
                        changed += res;
                    }
                }
            }
            if (changed > 0)
            {
                Debug.Log(assetfn + "  引用Miss个数=" + changed);
                File.WriteAllLines(assetfn, ll);
            }
        }

        //必须使用dll工程去替换非DLL工程
        //查找并替换行
        //dll -- > script
        public virtual int TestAndFindMissInLine(int lineNo, string sLine, out string sNewLine)
        {
            //匹配DLL引用
            //baa3e98256fcd8542a6a5ba1a0290fa5
            sNewLine = "";

            Match m = rgx_11500000.Match(sLine);
            if (m != null && m.Success)
            {
                missrow++;
                //
                string fileID = m.Groups[1].ToString();
                string fileType = m.Groups[2].ToString();
                //根据fileID找到
                CCTypeMeta_t t = _dll.FindByFileID(fileID);  //只是提供一个dll的类型表
                if (t == null)
                {
                    string log = string.Format("[{0}] 未找到Ref={1},{2},{3}", lineNo, fileID, sLine, _curFile);
                    CCTypeMeta_t t2 = _dllBack.FindByFileID(fileID);
                    if (t2 == null)
                    {
                        Debug.LogWarning(log);
                        return 0;
//                        log = log + t2.type;
                    }
                    t = t2;
                }
                //根据t，找到对应的文件..
                Meta_t v;
                v = m_dstMetas.FindByType(t.type);   //表示目标工程路径
                if (v == null)
                {
                    Debug.LogWarning(string.Format("[{0}] 未找到m_dstMetas ={1},{2},{3}", lineNo, t.type.Name, sLine, _curFile));
                    return 0;
                }
                //构造替换语句 " +  + "
                //Debug.Log(string.Format("[{0}]找到Ref={1},{2},{3}", lineNo, t, v.fn, sLine));
                sNewLine = string.Format(@"  m_Script: {{fileID: {0}, guid: {1}, type: {2}}}", t.ClassIDString, v.uuid, fileType);
                replacerow++;
                return 1;
            }
            return 0;
        }
    }
}

