using Assets.Editor.CS_CompilerGeneated;

namespace libEditor
{
    class Program
    {
        static void Main(string[] args)
        {
            CodeFile_t cf = new CodeFile_t();

            string filepath = "G:/GitHub/libGithub/UnityScripts/Assets/Editor/CS_CompilerGeneated/CollectionLayout.txt";
            cf.Load(filepath);
            cf.Handle();
            cf.Save(filepath+".cs");
        }
    }
}
