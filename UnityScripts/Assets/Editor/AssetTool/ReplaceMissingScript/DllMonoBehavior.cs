using System;
using UnityEditor;
using UnityEngine;

namespace Tool.RMS
{

    //管理Dll中的Mono
    public class DllMonoBehavior: CCAssembleTypes
    {
        //[MenuItem("tool/test DllMonoBehavior")]
        //public static void test_DllMonoBehavior()
        //{
        //    DllMonoBehavior dr = new DllMonoBehavior();
        //    dr.LoadDomainDllTypes("xGame");
        //}

        public Meta_t m_meta;
        //
        public override bool CanAdd(Type type)
        {
            return ClassID(type) > 0;
        }
        //取得
        int ClassID(Type type)
        {
            if (type.IsSubclassOf(type_mono))
                return 115; //特殊的

            if (type.IsSubclassOf(type_ScriptableObject))
                return 115; //特殊的

            Type b = type.BaseType;
            while(b != null)
            {
                //基类必须在UnityEngine
                if(b.Module.ToString() == "UnityEngine.dll")
                {
                    int ClassID = YAMLClassID.ClassID(b.Name);
                    return ClassID ;
                }
                b = b.BaseType;
            };
            return 0;
        }

        public override void OnLoad()
        {
            m_meta = Meta_t.FromFile(candi.Location + ".meta");
        }

        //
        public override CCTypeMeta_t AddType(Type type)
        {
            CCTypeMeta_t p = base.AddType(type);
            if(p!=null)
            {
                p.ClassID = ClassID(type);
            }
            else
            {
                UnityEngine.Debug.LogError("AddType fail:"+ type.ToString());
            }
            return p;
        }

        Type type_mono = typeof(MonoBehaviour);//Type.GetType("UnityEngine.MonoBehaviour");
        //bool IsMonoBehavior(Type type)
        //{
        //    if (type.IsSubclassOf(type_mono))
        //    {
        //        return true;
        //    }
        //    return false;
        //}

        Type type_ScriptableObject = typeof(ScriptableObject);//Type.GetType("UnityEngine.MonoBehaviour");
        //bool IsScriptableObject(Type type)
        //{
        //    if (type.IsSubclassOf(type_ScriptableObject))
        //    {
        //        return true;
        //    }
        //    return false;
        //}

    }

}
