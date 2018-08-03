using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace AssetTool
{
    /// <summary>
    /// 
    /// </summary>
    class ReplaceDefaultMaterial : BaseReplaceAlgo
    {
        [MenuItem("tool/AssetTool/Replace Mat")]
        static void ReplaceComponentSelect()
        {
            //查找所有的mat中，是否有引用默认
            ReplaceDefaultMaterial rdm = new ReplaceDefaultMaterial("Default-Particle (Instance)", "Assets/Effect/Common/TDefault.mat");
            rdm.Run(Application.dataPath);

            EditorUtility.ClearProgressBar();
            //Repalce();
        }
        private static void Repalce()
        {
            UnityEngine.GameObject[] selections = Selection.gameObjects;
            ReplaceDefaultMaterial rdm = new ReplaceDefaultMaterial("Default-Particle (Instance)", "Assets/Effect/Common/TDefault.mat");
            foreach (var o in selections)
            {
                rdm.SetParticleSystemRecursively(o);
            }
            AssetDatabase.SaveAssets();
        }


        string src_uudi = "0000000000000000f000000000000000";
        Regex rgx_11500000 = null;
        public void Run(string proj_path)
        {
            //m_Shader: {fileID: 46, guid: 0000000000000000f000000000000000, type: 0}
            //46-Standard
            //7 - Legacy Shaders/Diffuse
            string patten = string.Format(@"m_Shader: {{fileID: 46, guid: {0}, type: ([0-9]+)}}", src_uudi);
            rgx_11500000 = new Regex(patten);

            ReplaceProjectMissScriptInFile(proj_path, "*.mat"); //*.asset;*.mat;

            Debug.Log(proj_path + "  引用Miss个数=" + missrow);

            //    public static Dictionary<string, string> shaderTable = new Dictionary<string, string>()
            //{
            //    {"Standard", "Legacy Shaders/Diffuse"}, //map standard to the diffuse shader, good enough for this small example
            //    {"Standard (Specular Setup)", "Legacy Shaders/Specular"}
            //    //more...
            //};
        }


        protected override int TestAndFindMissInLine(int lineNo, string sLine, out string sNewLine)
        {
            sNewLine = "";

            if (sLine.Contains(src_uudi))
            {
                Match m = rgx_11500000.Match(sLine);
                if (m != null && m.Success)
                {
                    missrow++;
                    //Debug.Log("Match ok: " + sLine + _curFile);
                    //
                    //string fileID = m.Groups[1].ToString();
                    string fileType = m.Groups[1].ToString();

                    int pos = sLine.IndexOf(":");
                    string pad = sLine.Substring(0, pos);
                    ////Debug.Log(string.Format("[{0}]找到Ref={1},{2},{3}", lineNo, t, v.fn, sLine));
                    sNewLine = string.Format(@"{0}: {{fileID: {1}, guid: {2}, type: {3}}}", pad, 7, src_uudi, fileType);
                    replacerow++;
                    //_shouldStop = true;
                    return 1;
                }
                else
                {
                    //Debug.LogError("Match fail: " + ":" + sLine);
                }
            }
            return 0;
        }

            /// <summary>
            /// /
            /// </summary>
        string mSrc;
        Material mMaterial;

        public int count = 0;

        //"Default-Particle (Instance)"
        public ReplaceDefaultMaterial(string src,string dst)
        {
            _prefix = "m_Shader: ";
            _progress_prefix = "ReplaceDefaultMaterial...";

            mSrc = src;
            UnityEngine.Object obj = AssetDatabase.LoadMainAssetAtPath(dst);//"Assets/TDefault.mat"
            mMaterial = obj as Material;
        }

        public void SetParticleSystemRecursively(GameObject rootObj)
        {
            //if (null != rootObj.GetComponent<Renderer>())
            //{
            //    Debug.Log(rootObj.GetComponent<Renderer>().name);
            //}
            count = 0;

            ParticleSystem ps = rootObj.GetComponent<ParticleSystem>();
            if (null != ps)
            {
                replaceDefault(ps);
            }
            ParticleSystem[] pss = rootObj.GetComponentsInChildren<ParticleSystem>(true);
            if (pss != null)
            {
                for (int i = 0; i < pss.Length; ++i)
                    replaceDefault(pss[i]);
            }
            //IEnumerable<GameObject> subObj = rootObj.get();//rootObj.GetDirectChildren();
            //IEnumerator<GameObject> e = subObj.GetEnumerator();
            //if (null != rootObj)
            //{
            //    while (e.MoveNext())
            //    {
            //        SetObjRecursively(e.Current,mat);
            //    }
            //}            
        }
        void replaceDefault(ParticleSystem ps)
        {
            //Debug.Log(ps.name);
            Renderer r = ps.GetComponent<Renderer>();
            if (null != r)
            {
                if (mSrc == r.material.name)
                {
                    r.material = mMaterial;
                    count++;
                }
            }
        }

    }



}