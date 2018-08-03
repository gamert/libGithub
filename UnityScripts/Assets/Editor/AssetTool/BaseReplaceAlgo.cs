using System.IO;
using UnityEditor;
using UnityEngine;

namespace AssetTool
{
    //
    public class BaseReplaceAlgo
    {
        protected int missrow = 0;
        protected int replacerow = 0;

        protected string _prefix = "m_Script: ";
        protected string _curFile = "";

        protected string _progress_prefix = "ReplaceMissScriptInFile...";
        protected bool _shouldStop = false;


        //Unity Editor: <path tp project folder>/Assets
        static public string GetAssetPath(string pathname)
        {
            string dataPath = Application.dataPath.Replace('\\', '/');
            if (pathname.StartsWith(dataPath))
            {
                pathname = "Assets" + pathname.Replace(dataPath, "");
            }
            return pathname;
        }

        protected void ReplaceProjectMissScriptInFile(string proj_path, string filte)
        {
            replacerow = 0;
            string[] ss = Directory.GetFiles(proj_path, filte, SearchOption.AllDirectories);
            for (int i = 0; i < ss.Length && !_shouldStop; ++i)
            {
                float progress = (float)i / ss.Length;

                EditorUtility.DisplayProgressBar(_progress_prefix + filte, ss[i], progress);
                ReplaceMissScriptInFile(ss[i]);
            }
        }

        //判断各种脚本引用的包含情况
        protected void ReplaceMissScriptInFile(string assetfn)
        {
            _curFile = assetfn;

            int changed = 0;
            string[] ll = File.ReadAllLines(assetfn);
            for (int i = 0; i < ll.Length; ++i)
            {
                int pos = ll[i].IndexOf(_prefix);
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

        protected virtual int TestAndFindMissInLine(int lineNo, string sLine, out string sNewLine)
        {
            sNewLine = "";
            return 0;
        }
    }



}
