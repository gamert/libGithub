//using System.Collections;
//using System.Collections.Generic;
//using System.IO;
//using System.Text.RegularExpressions;
//using AssetTool.RMS;
//using UnityEditor;
//using UnityEngine;


///// <summary>
///// 移除冗余的Shader文件
///// </summary>
//namespace AssetTool
//{
//    //for test use
//    class CCFile
//    {
//        public static string GetFullName(string s)
//        {
//            return s;
//        }
//    }


//    class SameNameShadersList
//    {
//        public Meta_t main;
//        public List<Meta_t> ls = new List<Meta_t>();

//        public void Add(string key, Meta_t v)
//        {
//            if (key.Contains("/Export/Shaders/"))
//            {
//                if (main == null)
//                    main = v;
//                else
//                    Debug.LogError("重复:" + key);
//            }
//            else
//            {
//                ls.Add(v);
//            }
//        }
//        //
//        public void MakeSure()
//        {
//            if(main == null)
//            {
//                //找到最短名字的，设置为最优的存储位置。。。
//                int index = 0;
//                for(int i = 1;i<ls.Count;++i)
//                {
//                    if (ls[index].fn.Length > ls[i].fn.Length)
//                        index = i;
//                }
//                main = ls[index];
//                ls.RemoveAt(index);
//            }
//        }
//    }



//    public class RemoveRedShaderFile: BaseReplaceAlgo
//    {
//        public RemoveRedShaderFile()
//        {
//            _prefix = "m_Shader: ";
//            _progress_prefix = "RemoveRedShaderFile...";
//        }


//        [MenuItem("tool/AssetTool/移除冗余Shader文件")]
//        public static void doRemoveRedShaderFile()
//        {
//            RemoveRedShaderFile dr = new RemoveRedShaderFile();
//            dr.Run(Application.dataPath);

//            EditorUtility.ClearProgressBar();
//        }

//        Dictionary<string, SameNameShadersList> fenlei = null;
//        Regex rgx_11500000 = null;
//        void Run(string proj_path)
//        { 
//            //string proj_path = Application.dataPath;
//            //1. 
//            CDicMeta m_dstMetas = new CDicMeta();
//            CDicMeta.CreateMetaDic(proj_path, m_dstMetas, "*.shader");

//            //2. 查找同名并分类
//            fenlei = new Dictionary<string, SameNameShadersList>();
//            foreach(var e in m_dstMetas.dic)
//            {
//                string fn = CCFile.GetFullName(e.Value.fn);
//                SameNameShadersList snf = null;
//                if(!fenlei.TryGetValue(fn, out snf))
//                {
//                    snf = new SameNameShadersList();
//                    fenlei.Add(fn, snf);
//                }
//                snf.Add(e.Value.fn, e.Value);
//            }
//            foreach (var e in fenlei)
//            {
//                SameNameShadersList sns = e.Value;
//                sns.MakeSure();
//            }

//            //
//            ReplaceProjectMissScriptInFile(proj_path, "*.mat"); //*.asset;*.mat;

//            ReplaceProjectMissScriptInFile(proj_path, "*.asset");
//            if (replacerow > 0)
//            {
//                Debug.Log("asset 替换 =" + replacerow);
//            }
//            ReplaceProjectMissScriptInFile(proj_path, "*.prefab");
//            if (replacerow > 0)
//            {
//                Debug.Log("prefab 替换 =" + replacerow);
//            }
//            ReplaceProjectMissScriptInFile(proj_path, "*.unity");
//            if (replacerow > 0)
//            {
//                Debug.Log("unity 替换 =" + replacerow);
//            }

//            //删除其他的引用...
//            foreach (var e in fenlei)
//            {
//                SameNameShadersList sns = e.Value;
//                for (int i = sns.ls.Count - 1; i >= 0; --i)
//                {
//                    Meta_t src = sns.ls[i];
//                    //删除这个shader..
//                    //string an = GetAssetPath(src.fn);
//                    File.Delete(src.fn);
//                    if(File.Exists(src.fn))
//                    {
//                        Debug.LogError("DeleteAsset fail:" + src.fn);
//                        return;
//                    }
//                }
//            }
//            fenlei.Clear();
//        }



//        string dstUUID = "";
//        protected override int TestAndFindMissInLine(int lineNo, string sLine, out string sNewLine)
//        {
//            sNewLine = "";

//            //对每一行做判断？
//            int num = 0;

//            //3. 处理冗余：把其他引用换成main引用：
//            //m_Shader: {fileID: 4800000, guid: 57aaee5a2d5e29c40b771baaec768e35, type: 3}
//            foreach (var e in fenlei)
//            {
//                num++;
//                SameNameShadersList sns = e.Value;
//                if (sns.ls.Count == 0)
//                    continue;

//                dstUUID = sns.main.uuid;    //

//                for (int i = sns.ls.Count - 1; i >= 0; --i)
//                {
//                    Meta_t src = sns.ls[i];
//                    if(sLine.Contains(src.uuid))
//                    {
//                        string patten = string.Format(@"m_Shader: {{fileID: ([0-9\-]+), guid: {0}, type: ([0-9]+)}}", src.uuid);
//                        rgx_11500000 = new Regex(patten);

//                        Match m = rgx_11500000.Match(sLine);
//                        if (m != null && m.Success)
//                        {
//                            missrow++;
//                            //
//                            string fileID = m.Groups[1].ToString();
//                            string fileType = m.Groups[2].ToString();

//                            int pos = sLine.IndexOf(":");
//                            string pad = sLine.Substring(0, pos);
//                            //Debug.Log(string.Format("[{0}]找到Ref={1},{2},{3}", lineNo, t, v.fn, sLine));
//                            sNewLine = string.Format(@"{0}: {{fileID: {1}, guid: {2}, type: {3}}}", pad, fileID, dstUUID, fileType);
//                            replacerow++;

//                            //_shouldStop = true;
//                            return 1;
//                        }
//                        else
//                        {
//                            Debug.LogError("Match fail: "+ patten + ":" + sLine);
//                        }
//                    }
//                }
//            }
//            return 0;
//        }

//    }//end 
//}

