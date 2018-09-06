using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using cn.crashByNull;

namespace AssetTool.RMS
{
    //绑定类型，直接计算
    //外部： fileID的值永远是11500000，它实际上指的是MonoScript类型组件，它的值由ClassID * 10000
    //dll内部: fileID为hash值，
    public class CCTypeMeta_t
    {
        public CCTypeMeta_t(Type t)
        {
            type = t;
            int d = FileIDUtil.Compute(t);
            fileID = d.ToString();
        }
        public Type     type;
        public string   fileID;
        public int      ClassID;    //如果这个是YAML，那么保存其ClassID

        public string ClassIDString
        {
            get { return (ClassID * 100000).ToString(); }
        }

        public void Dump(StreamWriter sw,bool bWithMember)
        {
            sw.WriteLine(type.ToString() + "=" + fileID);//
            if (!bWithMember)
            {
                return;
            }

            //sw.WriteLine("\nThe members of class '{0}' are :\n", type);

            //返回为当前 Type 的所有公共成员。
            //成员包括属性、 方法、 字段、 事件和等等。
            BindingFlags bindingAttr = BindingFlags.DeclaredOnly 
                | BindingFlags.Public 
                | BindingFlags.NonPublic 
                | BindingFlags.Instance 
                | BindingFlags.Static
                ;
            MemberInfo[] mi = type.GetMembers(bindingAttr);
            for (int i = 0; i < mi.Length; ++i)
            {
                if(mi[i].MemberType == MemberTypes.Constructor)
                {
                    if(mi[i].Name == ".ctor")
                    {
                        continue;
                    }
                }

                sw.WriteLine("\t{0};{1}", mi[i].MemberType, mi[i].ToString());
                //if(mi[i].MemberType == MemberTypes.Method)
                //{
                //    MethodInfo tt = mi[i] as MethodInfo;
                //    sw.WriteLine("\t{0};{1};{2}", tt.MemberType, tt.ReturnParameter, tt.Name);
                //}
                //else if (mi[i].MemberType == MemberTypes.Field)
                //{
                //    FieldInfo tt = mi[i] as FieldInfo;
                //    sw.WriteLine("\t{0};{1};{2}", tt.MemberType, tt.FieldType, tt.Name);
                //}
                //else if (mi[i].MemberType == MemberTypes.Field)
                //{
                //    EventInfo tt = mi[i] as EventInfo;
                //    sw.WriteLine("\t{0};{1};{2}", tt.MemberType, tt.EventHandlerType, tt.Name);
                //}
                /*mi[i].MemberType =
        Constructor = 1,
        Event = 2,
        Field = 4,
        Method = 8,
        Property = 16,
        TypeInfo = 32,
        Custom = 64,
        NestedType = 128,
                */
                //sw.WriteLine(mi[i].ToString());
            }

            //sw.WriteLine("\nThe GetFields of class '{0}' are :\n", type);
            //FieldInfo[] fi = type.GetFields(bindingAttr);
            //for(int i = 0;i < fi.Length;++i)
            //{
            //    sw.WriteLine(fi[i].ToString());
            //}

            //sw.WriteLine("\nThe GetEvents of class '{0}' are :\n", type);
            //EventInfo[] ei = type.GetEvents(bindingAttr);
            //for (int i = 0; i < ei.Length; ++i)
            //{
            //    sw.WriteLine(ei[i].ToString());
            //}

        }
    }


    //基本类
    public class CCAssembleTypes
    {
        public string mName;
        public Assembly candi; //

        //k = ""
        //
        public Dictionary<string, CCTypeMeta_t> typeDic = new Dictionary<string, CCTypeMeta_t>();

        public virtual bool IsOk()
        {
            return candi != null;// && m_meta != null
        }

        public virtual bool CanAdd(Type t)
        {
            return true;
        }

        public virtual void OnLoad()
        {

        }

        public CCTypeMeta_t FindTypeMeta(string typeName)
        {
            CCTypeMeta_t t = null;
            typeDic.TryGetValue(typeName, out t);
            return t;
        }
        
        //根据fileid找到type:
        public CCTypeMeta_t FindByFileID(string fid)
        {
            foreach (CCTypeMeta_t t in typeDic.Values)
            {
                if (t.fileID == fid)
                    return t;
            }
            return null;
        }

        public Type Find(string typeName)
        {
            CCTypeMeta_t t = null;
            typeDic.TryGetValue(typeName, out t);
            return t.type;
        }

        //
        public void Dump(bool bWithMember = false)
        {
            //int pos = m_meta.fn.LastIndexOf('/');
            //string fn;
            //if (pos == -1)
            //    pos = m_meta.fn.LastIndexOf('\\');
            string fn = mName;// m_meta.fn.Substring(pos + 1);
            FileStream fp = new FileStream(UnityEngine.Application.streamingAssetsPath + "/Type_" + fn + ".txt", FileMode.Create);
            StreamWriter sw = new StreamWriter(fp);
            foreach (CCTypeMeta_t t in typeDic.Values)
            {
                t.Dump(sw, bWithMember);
                //sw.WriteLine();//t.type.ToString() + "=" + t.fileID
            }
            sw.Close();
            fp.Close();
        }

        //读取本Domain 中DLL中的所有类型
        public Assembly LoadDomainDllTypes(string dllName)
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
                mName = dllName;

                //candi.FullName
                // "G:\Aotu\worksapce100\Client2\UIBase, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null.meta".
                Type[] types = candi.GetTypes();
                SafeAdd(types);
                //Dump();
                OnLoad();
            }
            return candi;
        }

        void Clear()
        {
            candi = null;
            typeDic.Clear();
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
                if (cc != '<' && cc != '$' && cc != '_' && CanAdd(type))
                {
                    AddType(type);
                }
            }
        }
        public virtual CCTypeMeta_t AddType(Type type)
        {
            CCTypeMeta_t p = null;
            if (typeDic.ContainsKey(type.Name))
            {
            }
            else
            {
                p = new CCTypeMeta_t(type);
                typeDic.Add(type.Name, p);
            }
            return p;
        }
    }

    //专门处理UnityEngine
    class CCUnity : CCAssembleTypes
    {
        //
        public CCUnity()
        {
            LoadDomainDllTypes("UnityEngine");
        }
        public override void OnLoad()
        {
            foreach (var kv in typeDic)
            {
                kv.Value.ClassID = YAMLClassID.ClassID(kv.Key);
            }
        }

        public static CCUnity Instance = new CCUnity();
    }

}
