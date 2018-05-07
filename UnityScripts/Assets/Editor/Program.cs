using Assets.Editor.CS_CompilerGeneated;
using System.IO;

namespace libEditor
{
    class Program
    {
        static void Main(string[] args)
        {
            string filepath = "D:/Github/gamert/libGithub/UnityScripts/Assets/Editor/CS_CompilerGeneated/CollectionLayout.txt";
            //            string filepath = "G:/GitHub/libGithub/UnityScripts/Assets/Editor/CS_CompilerGeneated/CollectionLayout.txt";
            handleOneFile(filepath);


        }

        static void handlePath(string path)
        {
            string[] ss = Directory.GetFiles(path, "*.cs", SearchOption.AllDirectories);
            for (int i = 0; i < ss.Length; ++i)
            {
                handleOneFile(ss[i]);
            }
            //Debug.Log(string.Format("****创建Meta字典****： {0}处理完毕，count={1}", proj_path, dicMetas.Count));
        }


        static void handleOneFile(string filepath)
        {
            string ss = File.ReadAllText(filepath);
            int pos = ss.IndexOf("// Is alternative restored code with simple decompilation options:");
            if(pos!=-1)
            {
                File.WriteAllText(filepath,ss.Substring(pos));

                CodeFile_t cf = new CodeFile_t();
                cf.Load(filepath);
                cf.Handle();
                if (filepath.EndsWith(".cs"))
                    cf.Save(filepath);
                else if (filepath.EndsWith(".cs"))
                    cf.Save(filepath + ".cs");

            }
        }

    }
}
