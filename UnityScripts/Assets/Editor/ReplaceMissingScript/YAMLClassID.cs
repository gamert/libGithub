using System.Collections.Generic;
using System.IO;

namespace Tool.RMS
{
    public class YAMLClassID
    {
        static YAMLClassID()
        {
            Load(UnityEngine.Application.dataPath + "/Editor/ReplaceMissingScript/ClassID.txt");
        }
        static Dictionary<string, int> dic = new Dictionary<string, int>();
        static void Load(string classid_file)
        {
            string[] ll = File.ReadAllLines(classid_file);
            for (int i = 0; i < ll.Length; ++i)
            {
                string[] kv = ll[i].Split(new char[] { '\t',' ' });
                if(kv.Length == 2)
                {
                    int v = 0;
                    int.TryParse(kv[0], out v);
                    dic.Add(kv[1], v);
                }
            }        
        }
        //
        public static int ClassID(string key)
        {
            int v = 0;
            dic.TryGetValue(key, out v);
            return v;
        }
    }
}
