using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using cn.crashByNull;
using UnityEngine;

namespace Tool.RMS
{

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
            try
            {
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
            }
            catch(Exception e)
            {

            }
            return v;
        }
    }



    class CDicMeta
    {
        public Dictionary<string, Meta_t> dic = new Dictionary<string, Meta_t>();

        public int Count
        {
            get { return dic.Count; }
        }
        public void Add(string k, Meta_t v)
        {
            dic.Add(k, v);
        }
        public bool TryGetValue(string k, out Meta_t v)
        {
            return dic.TryGetValue(k, out v);
        }
        public void Clear()
        {
            dic.Clear();
        }

        //根据type来找到一个ref Meta，
        public Meta_t FindByType(Type t)
        {
            string key = "\\" + t.Name + ".cs";
            foreach (Meta_t m in dic.Values)
            {
                if (m.fn.EndsWith(key))
                {
                    return m;
                }
            }
            return null;
        }

        //增加到dic中
        static void HandleFileMetaToDic(string metafile, CDicMeta dicMetas)
        {
            Meta_t v = Meta_t.FromFile(metafile);
            if (v != null)
                dicMetas.Add(v.uuid, v);
        }

        //如何快速处理meta?
        //"*.cs"
        public static void CreateMetaDic(string proj_path, CDicMeta dicMetas, string filt = "*.meta")
        {
            bool bAllMeta = filt == "*.meta";
            string[] ss = Directory.GetFiles(proj_path, filt, SearchOption.AllDirectories);
            for (int i = 0; i < ss.Length; ++i)
            {
                //统一处理成unix文件名
                string fn = ss[i].Replace('\\','/');
                HandleFileMetaToDic(bAllMeta ? fn : (fn + ".meta"), dicMetas);
            }
            Debug.Log(string.Format("****创建Meta字典****： {0}处理完毕，count={1}", proj_path, dicMetas.Count));
        }

    }


    //参数1 为要查找的总路径， 参数2 保存路径  
    //private static void GetDirs(string dirPath, ref List<string> dirs)
    //{
    //    foreach (string path in Directory.GetFiles(dirPath))
    //    {
    //        //获取所有文件夹中包含后缀为 .prefab 的路径  
    //        if (System.IO.Path.GetExtension(path) == ".prefab")
    //        {
    //            dirs.Add(path.Substring(path.IndexOf("Assets")));
    //            //Debug.Log(path.Substring(path.IndexOf("Assets")));
    //        }
    //    }

    //    if (Directory.GetDirectories(dirPath).Length > 0)  //遍历所有文件夹  
    //    {
    //        foreach (string path in Directory.GetDirectories(dirPath))
    //        {
    //            GetDirs(path, ref dirs);
    //        }
    //    }
    //}


}
