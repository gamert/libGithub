using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

//载入dll中所有的类型...
namespace AssetTool.RMS
{
    class CSharpTypesWithMember : CCAssembleTypes
    {

        [MenuItem("tool/DllTypesWithMember Create")]
        public static void TestDllTypesWithMember()
        {
            CSharpTypesWithMember dr = new CSharpTypesWithMember();
            dr.Run("Assembly-CSharp");
            dr.Run("Assembly-CSharp-firstpass");
            EditorUtility.ClearProgressBar();
        }
        public void Run(string dll)
        {
            if (LoadDomainDllTypes(dll) != null)
                Dump(true);
            else
                UnityEngine.Debug.LogError("LoadDomainDllTypes fail:" + dll);

            UnityEngine.Debug.LogError("Run end");
        }
    }



    //
    class DllTypesComparer
    {
        static char[] trimChars = new char[] { '\t' };
        static int fielterLine = 0;

        public Dictionary<string, CSharpType_t> dic = new Dictionary<string, CSharpType_t>();
        public void Load(string type_dump_file)
        {
            CSharpType_t mem = null;
            string[] ll = File.ReadAllLines(type_dump_file);
            for (int i = 0; i < ll.Length && i < TestClassCount; ++i)
            {
                if(i%200 == 0)
                    EditorUtility.DisplayProgressBar(type_dump_file, ll[i], (float)i/ ll.Length);

                if (ll[i].StartsWith("\t"))
                {
                    string newLine = ll[i].TrimStart(trimChars);
                    //过滤掉
                    if (newLine == "Field;System.Int32 value__")
                    {
                        fielterLine++;
                    }
                    else
                    {
                        mem.Add(newLine);
                    }
                }
                else
                {
                    mem = new CSharpType_t(ll[i]);
                    if (dic.ContainsKey(mem.TypeName))
                    {
                        dic.Add(mem.TypeName + "_1", mem);
                    }
                    else
                    {
                        dic.Add(mem.TypeName, mem);
                    }
                }
            }
            UnityEngine.Debug.LogError("Load: "+ type_dump_file + ";ll = " +ll.Length+ ";dic = "+ dic.Count+ ";fielterLine="+ fielterLine);
        }

        //
        void Filter(string pub_class,bool bFuzzy, bool bInversLeft)
        {
            List<string> t = new List<string>();
            foreach (var p in dic)
            {
                if(bFuzzy)
                {
                    if (p.Value.TypeWithNS.StartsWith(pub_class))
                    {
                        t.Add(p.Key);
                    }
                    //特殊的Lua的Wrap类踢出:
                    else if (p.Key.EndsWith("Wrap") && p.Key == p.Value.TypeWithNS)
                    {
                        t.Add(p.Key);
                    }
                }
                else
                {
                    if (p.Value.TypeWithNS == (pub_class))
                    {
                        if (!bInversLeft)
                            t.Add(p.Key);
                    }
                    else if(bInversLeft)
                    {
                        t.Add(p.Key);
                    }
                }
            }
            for(int i = 0;i <t.Count;++i)
            {
                dic.Remove(t[i]);
            }
        }

        //过滤掉公共类型,bFuzzy: 是否模糊过滤，StartWith(NameSpace)
        //bInversLeft: 只保留匹配的
        public void Filter(string[] pub_class,bool bFuzzy,bool bInversLeft)
        {
            for(int i=0;i < pub_class.Length;++i)
            {
                EditorUtility.DisplayProgressBar("Filter", pub_class[i], (float)i / pub_class.Length);

                //过滤掉# 注释 、空行等
                if (pub_class[i].Length >2 && !pub_class[i].StartsWith("#"))
                {
                    Filter(pub_class[i], bFuzzy, bInversLeft);
                }
            }

            UnityEngine.Debug.LogError("Fielt: 剩余 dic = " + dic.Count);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="other"></param>
        /// <param name="testList"> 限制清单，用于测试</param>
        public void Intersect(DllTypesComparer other)
        {
            int sameTypeName = 0;

            string fn = "CompareResult";
            FileStream fp = new FileStream(UnityEngine.Application.streamingAssetsPath + "/" + fn + ".txt", FileMode.Create);
            StreamWriter sw = new StreamWriter(fp);

            int index = 0;
            foreach (var p in dic)
            {
                if (index % 100 == 0)
                    EditorUtility.DisplayProgressBar("Compare: 完全相同名字的(类) ", " ", (float)index / dic.Count);

                if (other.dic.ContainsKey(p.Key))
                {
                    sw.WriteLine("\t" + sameTypeName++ + "  " + p.Key + " ;" + p.Value.TypeWithNS);
                }

                index++;
            }
            UnityEngine.Debug.Log("Compare: 完全相同名字的(类): sameTypeName="+ sameTypeName);

            sw.WriteLine("\r\n");

            //交叉相似性:
            index = 0;
            foreach (var p in dic)
            {
                EditorUtility.DisplayProgressBar("Compare: 交叉相似性(类) ", p.Key, (float)index / dic.Count);
                p.Value.Compare(other.dic);
                //p.Value.Dump(sw);
                index++;
            }
            //TODO: 基于模糊算法的一致性检查:

            sw.Close();
            fp.Close();
        }


        public delegate bool delegate_check(CSharpType_t t);
        public delegate int delegate_comparer(CSharpType_t r1, CSharpType_t r2);
        public delegate void delegate_dumper(CSharpType_t t, StreamWriter sw);
        //带条件dump
        public void Dump(string fn, delegate_check fun, Comparison<CSharpType_t> comparison, delegate_dumper dumper,bool bAutoDelDumped)
        {
            List<string> t = new List<string>();
            List<CSharpType_t> t2 = new List<CSharpType_t>();

            FileStream fp = new FileStream(UnityEngine.Application.streamingAssetsPath + "/" + fn + ".txt", FileMode.Create);
            if(fp!=null)
            {
                StreamWriter sw = new StreamWriter(fp);
                sw.WriteLine("CONFIG_Type_LevenshteinDistance = "+ CSharpType_t.CONFIG_Type_LevenshteinDistance);
                sw.WriteLine("CONFIG_Member_LevenshteinDistance = " + CSharpType_t.CONFIG_Member_LevenshteinDistance);
                sw.WriteLine("");

                int index = 0;
                foreach (var p in dic)
                {
                    if(fun(p.Value))
                    {
                        t2.Add(p.Value);// p.Value.Dump(sw);
                        if (bAutoDelDumped)
                        {
                            t.Add(p.Key);
                        }
                    }
                    index++;
                }
                //排序..
                if (comparison != null)
                    t2.Sort(comparison);
                else
                    t2.Sort();

                for (int i = 0;i < t2.Count;++i)
                {
                    if (dumper != null)
                        dumper(t2[i], sw);
                    else
                        t2[i].Dump(sw);
                }

                if(bAutoDelDumped)
                {
                    for (int i = 0; i < t.Count; ++i)
                    {
                        dic.Remove(t[i]);
                    }
                }

                sw.Close();
                fp.Close();
            }
        }

        //控制测试总数...
        static int TestClassCount = 200000000;//;

        //
        [MenuItem("tool/DllTypes Comparer")]
        public static void TestDllTypesComparer()
        {
            //            
            string[] pub_class = File.ReadAllLines(Application.streamingAssetsPath + "/pub_class.txt");

            DllTypesComparer dr = new DllTypesComparer();
            string path = "G:/Aotu/worksapce100/DClient2/Trunk/Assets/StreamingAssets/";
            dr.Load(path+ "Type_Assembly-CSharp.txt");
            dr.Load(path + "Type_Assembly-CSharp-firstpass.txt");

            dr.Filter(pub_class, true, false);

            //过滤安全类型（已经确认的私有函数）
            string[] safe_class = File.ReadAllLines(Application.streamingAssetsPath + "/safe_class.txt");
            dr.Filter(safe_class, false, false);

            //string[] test_Left = new string[] { "MobaGo.Game.CChangeSkillRule" };
            //dr.Filter(test_Left, false, true);

            //参考
            DllTypesComparer dr2 = new DllTypesComparer();
            string path2 = Application.streamingAssetsPath+ "/ReliefCSharp/";
            dr2.Load(path2 + "Type_Assembly-CSharp.txt");
            dr2.Load(path2 + "Type_Assembly-CSharp-firstpass.txt");
            dr2.Filter(pub_class, true, false);

            //string[] test_Left2 = new string[] { "BackgroundWorker" };
            //dr2.Filter(test_Left2, false, true);

            //跟参考求交集: :8
            dr.Intersect(dr2);

            //
            dr.Dump("TypeNamePercent50", delegate (CSharpType_t t)
                                        {
                                            if (t.CompareNSResult.Count>0
                                            && t.CompareNSResult[0].percent > 300
                                            )
                                                return true;
                                            return false;
                                        }
            , CSharpType_t.NSResultCompare, delegate (CSharpType_t t, StreamWriter sw)
                                        {
                                            t.DumpNSResult(sw,50);
                                        }
            , false);

            //
            dr.Dump("Percent100", delegate (CSharpType_t t)
            {
                if (t.CompareResult.Count == 1
                && t.CompareResult[0].percent > 30
                && !t.TypeWithNS.StartsWith("io.marsdigtal.")
                && !t.TypeWithNS.StartsWith("project1615_mobago.")
                )
                    return true;
                return false;
            }
            , null, null, false);


            dr.Dump("Percent100", delegate (CSharpType_t t) 
                                {
                                    if (t.CompareResult.Count == 1 
                                    && t.CompareResult[0].percent  > 30
                                    && !t.TypeWithNS.StartsWith("io.marsdigtal.")
                                    && !t.TypeWithNS.StartsWith("project1615_mobago.")
                                    )
                                        return true;
                                    return false;
                                }
            ,null,null,true);

            dr.Dump("marsdigtal", delegate (CSharpType_t t)
                                {
                                        if (t.CompareResult.Count == 1 
                                        && t.CompareResult[0].percent > 30
                                        && (t.TypeWithNS.StartsWith("io.marsdigtal.")|| t.TypeWithNS.StartsWith("project1615_mobago"))
                                        )
                                        return true;
                                    return false;
                                }
            , null, null, true);

            //智能判断：1. 如果都是MonoBahaviour，那么如果只有1个公共函数[]
            string[] mono_public = new string[] { "Awake", "Start", "Update", "LateUpdate", "OnDestroy", "OnDrawGizmos", "OnDrawGizmosSelected" };

            dr.Dump("Percent_Left", delegate (CSharpType_t t)
            {
                return true;
            }
            , null, null, true);

            EditorUtility.ClearProgressBar();
            UnityEngine.Debug.LogError("TestDllTypesComparer: end ");
        }
    }
}
