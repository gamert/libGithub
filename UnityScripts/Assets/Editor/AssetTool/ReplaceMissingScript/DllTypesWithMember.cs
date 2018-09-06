using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;

//载入dll中所有的类型...
namespace AssetTool.RMS
{
    class DllTypesWithMember : CCAssembleTypes
    {

        [MenuItem("tool/TestDllTypesWithMember")]
        public static void TestDllTypesWithMember()
        {
            DllTypesWithMember dr = new DllTypesWithMember();
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


    //Cinemachine.CinemachineBlendListCamera=-450300591
    public class DllMember_t
    {
        public string MemberType;

        public string Return;      //成员类型申明、返回类型
        public string Name;        //
        public string Params;      //参数列表

        //Method;Void set_LiveChild(ICinemachineCamera)
        public DllMember_t(string line)
        {
            string[] kv = line.Split(new char[] {'\t', ';' });
            if (kv.Length == 2)
            {
                //if(kv[0].Contains('.'))
                //{
                //}
                MemberType = kv[0];
                int p1 = kv[1].IndexOf(' ');
                if(p1!=-1)
                {
                    Return = kv[1].Substring(0, p1);
                    int p2 = kv[1].IndexOf('(');
                    if (p2 != -1)
                    {
                        Name = kv[1].Substring(p1 + 1, p2-(p1 + 1));
                        Params = kv[1].Substring(p2 + 1);
                    }
                    else
                    {
                        Name = kv[1].Substring(p1 + 1);
                    }
                }
                else //NestedType;Cinemachine.CinemachineBlendListCamera+Instruction
                {
                    Return = kv[1];
                }
            }
            else
            {
                UnityEngine.Debug.LogError("DllMember_t fail: "+ line);
            }
        }
    }

    public class DllType_t
    {
        public string TypeWithNS;   //
        public string TypeName;     
        public List<DllMember_t> Members = new List<DllMember_t>();

        //Cinemachine.CinemachineBlendListCamera=-450300591
        public DllType_t(string line)
        {
            string[] kv = line.Split(new char[] { '=' });
            if (kv.Length == 2)
            {
                //if(kv[0].Contains('.'))
                //{
                //}
                TypeWithNS = kv[0];
                string[] kv2 = TypeWithNS.Split(new char[] { '.' });
                TypeName = kv2[kv2.Length - 1];
            }
            else
            {
                UnityEngine.Debug.LogError("DllType_t fail: " + line);
            }
        }

        public void Add(string line)
        {
            Members.Add(new DllMember_t(line));
        }
    }

    //
    class DllTypesComparer
    {
        static char[] trimChars = new char[] { '\t' };

        Dictionary<string, DllType_t> dic = new Dictionary<string, DllType_t>();
        public void Load(string type_dump_file)
        {
            DllType_t mem = null;
            string[] ll = File.ReadAllLines(type_dump_file);
            for (int i = 0; i < ll.Length; ++i)
            {
                if(i%200 == 0)
                    EditorUtility.DisplayProgressBar(type_dump_file, ll[i], i/ ll.Length);

                if (ll[i].StartsWith("\t"))
                {
                    mem.Add(ll[i].TrimStart(trimChars));
                }
                else
                {
                    mem = new DllType_t(ll[i]);
                    if(dic.ContainsKey(mem.TypeName))
                    {
                        dic.Add(mem.TypeName + "_1", mem);
                    }
                    else
                    {
                        dic.Add(mem.TypeName, mem);
                    }
                }
            }
        }

        //
        [MenuItem("tool/TestDllTypesComparer")]
        public static void TestDllTypesComparer()
        {
            DllTypesComparer dr = new DllTypesComparer();
            string path = "G:/Aotu/worksapce100/Client2/Assets/StreamingAssets/";
            dr.Load(path+ "Type_Assembly-CSharp.txt");
            dr.Load(path + "Type_Assembly-CSharp-firstpass.txt");

            EditorUtility.ClearProgressBar();
        }
    }
}
