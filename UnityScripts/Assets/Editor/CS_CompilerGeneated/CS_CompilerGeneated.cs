
using System.IO;
using System.Text;
//using System.Threading.Tasks;

namespace Assets.Editor.CS_CompilerGeneated
{

    public class CodeFile_t
    {
        public Block_t _using = new Block_t();

        public CodeClass_t _main = new CodeClass_t();

        string _filepath;
        public void Load(string filepath)
        {
            _filepath = filepath;

            string[] lines = File.ReadAllLines(filepath);
            for(int i = 0;i<lines.Length;++i)
            {
                if(lines[i] == (""))
                {
                }
                else if (lines[i].StartsWith("//"))
                {
                }
                else if (lines[i].StartsWith("/*"))
                {
                }
                else if (lines[i].StartsWith("using "))
                {
                    _using.rows.Add(lines[i]);
                }
                else if(!_main.IsEnd)  //如果已经结束
                {
                    _main.AddLine(lines[i]);
                }
                else
                {
                    break;
                }
            }
        }
        public void Handle()
        {
            _main.Handle();
        }

        public void Save(string filepath)
        {
            StringBuilder sb = new StringBuilder();
            _main.Save(sb);

            File.WriteAllText(filepath, sb.ToString());

        }
    }
}
