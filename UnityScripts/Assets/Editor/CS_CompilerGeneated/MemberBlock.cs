using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Assets.Editor.CS_CompilerGeneated
{

    //一个代码块
    public class Block_t
    {
        public static string SPACE4 = "    ";

        //普通的表达式
        public List<string> rows = new List<string>();
    }


    public enum State_e
    {
        State_Null = 0,
        State_Begin,
        State_End,
    }

    //管理一个成员块:包括带{}的函数，类等
    public class MemberBlock_t : Block_t
    {
        public string _title = "";
        public string type;    //默认的主type

        public MemberBlock_t(int deep)
        {
            setDeep(deep);
        }

        public List<string> attrs = new List<string>(); //保存属性
        //普通的表达式
        protected int _deep = 0;   //表示深度
        protected string sdeep = "";
        public void setDeep(int deep)
        {
            _deep = deep;
            sdeep = "";
            for (int i = 0; i < _deep; ++i)
            {
                sdeep = sdeep + "    ";
            }
        }

        public void ClearBody()
        {
            rows.Clear();
        }
        public void AddRow(string row)
        {
            rows.Add(row);
        }

        //
        public static void AutoTruncate_StartsWith(ref string line, string prefix, string replace = "")
        {
            if (line.StartsWith(prefix))
                line = line.Replace(prefix, replace);
        }

        protected State_e _stat = State_e.State_Null;   //未初始化
        public bool IsEnd { get { return _stat == State_e.State_End; } }
        public void setEnd()
        {
            _stat = State_e.State_End;
        }
        protected bool isClassStart(string line)
        {
            bool b = _stat == State_e.State_Null && line.StartsWith("{");
            if(b && rows.Count > 1)
            {
                string t = rows[rows.Count - 2];
                if (t.Contains(" enum "))
                    return true;
                if (t.Contains(" class "))
                    return true;
                if (t.Contains(" struct "))
                    return true;
                if (t.Contains(" IEnumerator"))
                    return true;
                if (t.Contains("_003Ec__Iterator"))
                    return true;

                //是否是函数的开始？
                if (t.StartsWith("public ")|| t.StartsWith("private ") || t.StartsWith("protected "))
                {
                    Log("找到一个函数： "+t);
                    Match m = rgx_func_dec.Match(t);
                    if (m != null && m.Success)
                    {
                        string dec = m.Groups[1].ToString();
                        string ret = m.Groups[2].ToString();
                        string func = m.Groups[3].ToString();
                        return !string.IsNullOrEmpty(ret) && !string.IsNullOrEmpty(func);
                    }
                    return false;
                }
            }
            return false;
        }
        protected bool isClassEnd(string line)
        {
            return _stat == State_e.State_Begin && line.StartsWith("}");
        }

        static Regex rgx_func_dec = new Regex(@"([public|private|protected]) ([a-zA-Z0-9]+) ([a-zA-Z0-9]+)\(");
        //成员..
        static Regex rgx_memb_dec = new Regex(@"([public|private|protected]) ([a-zA-Z0-9]+) ([a-zA-Z0-9]+);");

        //
        public void AddLine(string line)
        {
            if (line == "")
            {
                return;
            }
            this.rows.Add(line);
            if (isClassStart(line))
            {
                _title = rows[rows.Count-2];    //
                //从前一行中，处理声明?
                _stat = State_e.State_Begin;
            }
            else if (isClassEnd(line))
            {
                setEnd();// _stat = State_e.State_End;
            }
            else if(_stat == State_e.State_Begin)
            {
                if(line.StartsWith("    "))
                {
                    line = line.Substring(4);   //先去掉
                    handleLine(line);
                }
                else
                {
                    LogError("未识别的行 "+ line);
                }
            }
            else
            {
                //private FaeriaButtonDock craftModeButton;
                if (line.StartsWith("public ") || line.StartsWith("private ") || line.StartsWith("protected "))
                {
                    Match m = rgx_memb_dec.Match(line);
                    if (m != null && m.Success)
                    {
                        Log("找到一个成员： " + line);
                        string dec = m.Groups[1].ToString();
                        string ret = m.Groups[2].ToString();
                        string func = m.Groups[3].ToString();
                        if (!string.IsNullOrEmpty(ret) && !string.IsNullOrEmpty(func))
                        {
                            setEnd();
                        }
                    }
                }
                //               LogError("未识别的行 " + line);
            }
        }

        //由派生类执行处理
        public virtual void handleLine(string line)
        {

        }
        public virtual void Handle()
        {

        }

        //
        public virtual void Save(StringBuilder sb)
        {
            sb.AppendLine("");
            for (int i = 0; i < attrs.Count; ++i)
            {
                sb.AppendLine(sdeep + attrs[i]);
            }
            for (int i=0;i<rows.Count;++i)
            {
                sb.AppendLine(sdeep+rows[i]);
            }
        }


        public static void LogError(string s)
        {
#if TRACE
            Console.WriteLine(s);
#endif
        }
        public static void Log(string s)
        {
#if TRACE
            Console.WriteLine(s);
#endif
        }

        public bool isIEnumerator()
        {
            return _title.StartsWith("public IEnumerator ") || _title.StartsWith("private IEnumerator ") || _title.StartsWith("protected IEnumerator ");
        }

        //([a-zA-Z0-9]+)\, type\: ([0 - 9]+)\}
        static Regex rgx_CodeIteratorClass = new Regex(@"private sealed class _003C([a-zA-Z0-9]+)_003Ec__Iterator([0-9]+) \: IEnumerator\, IDisposable");
        public static string Regex_FetchMatch(Regex r,string t, int n)
        {
            Match m = r.Match(t);
            if (m != null && m.Success)
            {
                return m.Groups[n].ToString();
            }
            return null;
        }
        public bool isCodeIteratorClass(string func)
        {
            bool bfind = _title.Contains("private sealed class ");
            if(bfind)
            {
                string v = Regex_FetchMatch(rgx_CodeIteratorClass, _title, 1);
                return v == func;
            }
            return false;
        }


        static Regex rgx_IEnumeratorFuncName = new Regex(@"([public|private|protected]) IEnumerator ([a-zA-Z0-9]+)\(");
        public static string _FetchIEnumeratorFuncName(string title)
        {
            return Regex_FetchMatch(rgx_IEnumeratorFuncName, title,2);
        }
    }

    //
    public class MemberBlockEx_t : MemberBlock_t
    {
        //复杂的表达式
        public MemberBlockEx_t(int deep)
            :base(deep)
        {
        }

    }
}
